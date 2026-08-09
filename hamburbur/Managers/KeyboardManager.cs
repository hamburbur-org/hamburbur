using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Mods.Movement;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Managers;

public class KeyboardManager : Singleton<KeyboardManager>
{
    public bool KeyboardOpen;

    public GameObject Keyboard;
    
    private float keyboardHeight = -0.29f;

    public GameObject NonDominantButtonPresser;
    public Renderer   NonDominantButtonPresserRenderer;

    private readonly List<TextMeshPro> letterKeyLabels = [];

    private readonly Dictionary<Key, string> physicalKeyMap = new()
    {
            { Key.A, "a" }, { Key.B, "b" }, { Key.C, "c" }, { Key.D, "d" },
            { Key.E, "e" }, { Key.F, "f" }, { Key.G, "g" }, { Key.H, "h" },
            { Key.I, "i" }, { Key.J, "j" }, { Key.K, "k" }, { Key.L, "l" },
            { Key.M, "m" }, { Key.N, "n" }, { Key.O, "o" }, { Key.P, "p" },
            { Key.Q, "q" }, { Key.R, "r" }, { Key.S, "s" }, { Key.T, "t" },
            { Key.U, "u" }, { Key.V, "v" }, { Key.W, "w" }, { Key.X, "x" },
            { Key.Y, "y" }, { Key.Z, "z" },

            { Key.Digit0, "0" }, { Key.Digit1, "1" }, { Key.Digit2, "2" },
            { Key.Digit3, "3" }, { Key.Digit4, "4" }, { Key.Digit5, "5" },
            { Key.Digit6, "6" }, { Key.Digit7, "7" }, { Key.Digit8, "8" },
            { Key.Digit9, "9" },

            { Key.Space, "space" },
            { Key.Backspace, "backspace" },
            { Key.Enter, "enter" },
            { Key.Escape, "esc" },
            { Key.Tab, "tab" },
            { Key.LeftShift, "shift" },
            { Key.RightShift, "shift" },
            { Key.CapsLock, "caps lock" },
    };

    private readonly Dictionary<string, Action> specialCharacters = new()
    {
            { "backspace", () => TypedText = TypedText[..^1] },
            { "esc", () => Instance.CloseKeyboard() },
            {
                    "enter", () =>
                             {
                                 Instance.onEnterPressed?.Invoke(TypedText);
                                 Instance.CloseKeyboard();
                             }
            },
            { "caps lock", () => Instance.isLower = !Instance.isLower },
            { "shift", () => Instance.isShiftLower = !Instance.isShiftLower },
            { "space", () => TypedText += " " },
            { "tab", () => TypedText += "   " },
    };

    private Coroutine closeRoutine;

    private bool isLower      = true;
    private bool isShiftLower = true;

    private Material keyboardMainMaterial, keyboardSecondaryMaterial;
    private float    lastTimePositionSerialized;

    private Coroutine moveRoutine;

    private       Action<string> onEnterPressed;
    private       bool           ShouldUseUppercase => isLower != isShiftLower;
    public static string         TypedText          { get; private set; } = "";

    private IEnumerator Start()
    {
        while (Plugin.Instance                            == null ||
               Plugin.Instance.HamburburBundle            == null ||
               MenuHandler.Instance                       == null ||
               MenuHandler.Instance.Menu                  == null ||
               MenuHandler.Instance.Menu.transform.parent == null)
            yield return null;

        GameObject keyboardPrefab =
                Plugin.Instance.HamburburBundle.LoadAsset<GameObject>(nameof(Keyboard));

        if (keyboardPrefab == null)
        {
            Debug.LogError("Failed to load the keyboard prefab from HamburburBundle.");

            yield break;
        }

        Keyboard = Instantiate(
                keyboardPrefab,
                MenuHandler.Instance.Menu.transform.parent
        );

        Keyboard.transform.localScale    = Vector3.one * 0.3f;
        Keyboard.transform.localPosition = new Vector3(-0.12f, 0f, keyboardHeight);
        Keyboard.transform.localRotation = Quaternion.Euler(0f, 250f, 270f);

        keyboardMainMaterial = Keyboard.transform.TakeChild(0).gameObject.GetComponent<Renderer>().sharedMaterial;
        keyboardSecondaryMaterial =
                Keyboard.transform.TakeChild(0, 1, 0).gameObject.GetComponent<Renderer>().sharedMaterial;

        Keyboard.SetActive(false);

        NonDominantButtonPresser = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        NonDominantButtonPresser.name  = "hamburbur button presser";
        NonDominantButtonPresser.layer = 2;
        NonDominantButtonPresser.SetActive(false);

        NonDominantButtonPresser.transform.SetParent(Tools.Utils.RealLeftController);
        NonDominantButtonPresser.transform.localPosition = new Vector3(0f, -0.01f, 0.13f);
        NonDominantButtonPresser.transform.localScale    = Vector3.one * 0.01f;

        if (NonDominantButtonPresser.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer.material.shader = Shaders.UberShader;
            meshRenderer.material.color  = Plugin.Instance.MainColour;

            NonDominantButtonPresserRenderer = meshRenderer;
        }

        NonDominantButtonPresser.GetComponent<SphereCollider>().isTrigger = true;
        NonDominantButtonPresser.AddComponent<Rigidbody>().isKinematic    = true;
        NonDominantButtonPresser.AddComponent<ButtonPresser>().isLeft     = true;

        foreach (Transform child in Keyboard.transform.GetChild(0))
        {
            if (child.name == "TMP")
                continue;

            foreach (Transform button in child)
            {
                TextMeshPro keyText = button.GetComponentInChildren<TextMeshPro>();

                if (keyText == null)
                    continue;
                
                keyText.text = keyText.text.Trim();

                if (keyText.text.Length == 1 && char.IsLetter(keyText.text[0]))
                    letterKeyLabels.Add(keyText);

                button.AddComponent<ButtonCollider>().OnPress =
                        () => HandleKeyboardButtonPress(keyText.text);
            }
        }

        UpdateKeyCapitalisation();

        UpdateColoursAndHeight(
                MenuHandler.Instance.currentMainColour,
                MenuHandler.Instance.currentSecondaryColour,
                keyboardHeight
        );
    }

    private void Update()
    {
        if (!KeyboardOpen)
            return;

        if (Keyboard                       == null ||
            MenuHandler.Instance           == null ||
            MenuHandler.Instance.Menu      == null ||
            GTPlayer.Instance              == null ||
            GTPlayer.Instance.bodyCollider == null)
            return;

        if (!MenuHandler.Instance.Menu.activeSelf)
        {
            CloseKeyboard();

            return;
        }

        Transform menuParent = MenuHandler.Instance.Menu.transform.parent;

        Vector3 targetPos =
                GTPlayer.Instance.bodyCollider.transform.position       +
                GTPlayer.Instance.bodyCollider.transform.forward * 0.7f +
                Vector3.up                                       * 0.2f;

        float distance = Vector3.Distance(menuParent.position, targetPos);

        if (distance > 2f && moveRoutine == null)
            moveRoutine = StartCoroutine(MoveMenu());

        Vector3 pos = menuParent.position;
        pos.y               = Mathf.Lerp(pos.y, targetPos.y, Time.deltaTime * 1.5f);
        menuParent.position = pos;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera cameraToUse = Tools.Utils.GetActiveCamera();

            if (Physics.Raycast(
                        cameraToUse.ScreenPointToRay(Mouse.current.position.ReadValue()),
                        out RaycastHit hit,
                        0.6f,
                        1 << 2,
                        QueryTriggerInteraction.Collide))
                if (hit.collider.TryGetComponent(out ButtonCollider buttonCollider))
                    buttonCollider.OnPress?.Invoke();
        }

        HandlePhysicalKeyboard();

        if (lastTimePositionSerialized + 0.1f > Time.time)
            return;

        lastTimePositionSerialized = Time.time;
    }

    public void UpdateColoursAndHeight(Color mainColour, Color secondaryColour, float height)
    {
        keyboardHeight = height;
        
        if (NonDominantButtonPresserRenderer != null)
            NonDominantButtonPresserRenderer.material.color = mainColour;

        if (keyboardMainMaterial != null)
            keyboardMainMaterial.color = mainColour;

        if (keyboardSecondaryMaterial != null)
            keyboardSecondaryMaterial.color = secondaryColour;
    }

    private void UpdateKeyCapitalisation()
    {
        foreach (TextMeshPro keyText in letterKeyLabels)
            keyText.text = ShouldUseUppercase
                                   ? keyText.text.ToUpper()
                                   : keyText.text.ToLower();
    }

    private IEnumerator MoveMenu()
    {
        Transform menuParent = MenuHandler.Instance.Menu.transform.parent;

        while (true)
        {
            Vector3 targetPos =
                    GTPlayer.Instance.bodyCollider.transform.position       +
                    GTPlayer.Instance.bodyCollider.transform.forward * 0.7f +
                    Vector3.up                                       * 0.2f;

            Quaternion targetRot =
                    Quaternion.LookRotation(GTPlayer.Instance.bodyCollider.transform.forward) *
                    Quaternion.Euler(270f, 180f, 90f);

            menuParent.position = Vector3.Lerp(
                    menuParent.position,
                    targetPos,
                    Time.deltaTime * 6f
            );

            menuParent.rotation = Quaternion.Slerp(
                    menuParent.rotation,
                    targetRot,
                    Time.deltaTime * 6f
            );

            if (Vector3.Distance(menuParent.position, targetPos) < 0.05f)
            {
                menuParent.position = targetPos;
                menuParent.rotation = targetRot;

                break;
            }

            yield return null;
        }

        moveRoutine = null;
    }

    public event Action<string> OnTextChanged;
    public event Action         OnKeyboardClose;

    private void HandleKeyboardButtonPress(string input)
    {
        bool wasShiftLower = isShiftLower;

        input = input.Trim();

        if (specialCharacters.TryGetValue(input.ToLowerInvariant(), out Action special))
            special?.Invoke();
        else
            TypedText += ShouldUseUppercase
                                 ? input.ToUpperInvariant()
                                 : input.ToLowerInvariant();

        if (!wasShiftLower)
            isShiftLower = true;

        UpdateKeyCapitalisation();

        Keyboard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text =
                string.IsNullOrEmpty(TypedText)
                        ? "You haven't typed any text"
                        : TypedText;

        OnTextChanged?.Invoke(TypedText);
    }

    private void HandlePhysicalKeyboard()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null)
            return;

        foreach (KeyValuePair<Key, string> pair in physicalKeyMap.Where(pair => UnityEngine.InputSystem.Keyboard
                                                                               .current[pair.Key]
                                                                               .wasPressedThisFrame))
            HandleKeyboardButtonPress(pair.Value);
    }

    public void CloseKeyboard()
    {
        if (!KeyboardOpen)
            return;

        KeyboardOpen = false;

        WASDFly.DisableMovement = false;

        OnKeyboardClose?.Invoke();

        OnKeyboardClose = null;
        onEnterPressed  = null;
        OnTextChanged   = null;

        Keyboard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = "You haven't typed any text";
        TypedText                                                                   = "";
        Keyboard.SetActive(false);
        NonDominantButtonPresser.SetActive(false);
        if (MenuHandler.Instance.Menu.activeSelf)
            closeRoutine = StartCoroutine(GUIHandler.Instance.CloseMenu());
    }

    public void SpawnKeyboard(Action<string> onEnterPressed)
    {
        this.onEnterPressed = onEnterPressed;

        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            MenuHandler.Instance.IsWaiting = false;
        }

        StartCoroutine(SpawnKeyboard());
    }

    private IEnumerator SpawnKeyboard()
    {
        while (Keyboard                       == null ||
               MenuHandler.Instance           == null ||
               MenuHandler.Instance.Menu      == null ||
               GTPlayer.Instance              == null ||
               GTPlayer.Instance.bodyCollider == null)
            yield return null;
        
        Keyboard.transform.localPosition = new Vector3(-0.12f, 0f, keyboardHeight);

        KeyboardOpen = true;
        TypedText    = "";

        WASDFly.DisableMovement = true;
        Transform menuParent = MenuHandler.Instance.Menu.transform.parent;

        menuParent.transform.SetParent(null);
        menuParent.transform.position = GTPlayer.Instance.bodyCollider.transform.position       +
                                        GTPlayer.Instance.bodyCollider.transform.forward * 0.5f + Vector3.up * 0.2f;

        menuParent.transform.rotation = Quaternion.LookRotation(GTPlayer.Instance.bodyCollider.transform.forward) *
                                        Quaternion.Euler(270f, 180f, 90f);

        if (!MenuHandler.Instance.Menu.activeSelf)
            yield return MenuHandler.Instance.OpenMenu();

        NonDominantButtonPresser.SetActive(true);
        NonDominantButtonPresser.transform.SetParent(RightHanded.IsEnabled
                                                             ? Tools.Utils.RealRightController
                                                             : Tools.Utils.RealLeftController);

        NonDominantButtonPresser.transform.localPosition = new Vector3(0f, -0.01f, 0.13f);

        Keyboard.SetActive(true);
    }
}