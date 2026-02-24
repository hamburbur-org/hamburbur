using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Mods.Movement;
using hamburbur.Mods.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Managers;

public class KeyboardManager : Singleton<KeyboardManager>
{
    public bool KeyboardOpen;

    public GameObject Keyboard;

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

    private bool  isLower      = true;
    private bool  isShiftLower = true;
    private float lastTimePositionSerialized;

    private Coroutine  moveRoutine;
    private GameObject nonDominantButtonPresser;

    private       Action<string> onEnterPressed;
    public static string         TypedText { get; private set; } = "";

    private void Start()
    {
        GameObject keyboardPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("Keyboard");
        Keyboard = Instantiate(keyboardPrefab, MenuHandler.Instance.Menu.transform.parent);

        Keyboard.transform.localScale    = Vector3.one * 0.3f;
        Keyboard.transform.localPosition = new Vector3(-0.12f, 0f, -0.29f);
        Keyboard.transform.localRotation = Quaternion.Euler(0f, 250f, 270f);

        Keyboard.SetActive(false);

        nonDominantButtonPresser = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        nonDominantButtonPresser.name  = "hamburbur button presser";
        nonDominantButtonPresser.layer = 2;
        nonDominantButtonPresser.SetActive(false);

        nonDominantButtonPresser.transform.SetParent(Tools.Utils.RealLeftController);
        nonDominantButtonPresser.transform.localPosition = new Vector3(0f, -0.01f, 0.13f);
        nonDominantButtonPresser.transform.localScale    = Vector3.one * 0.01f;

        if (nonDominantButtonPresser.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer.material.shader = Plugin.Instance.UberShader;
            meshRenderer.material.color  = Plugin.Instance.MainColour;
        }

        nonDominantButtonPresser.GetComponent<SphereCollider>().isTrigger = true;
        nonDominantButtonPresser.AddComponent<Rigidbody>().isKinematic    = true;
        nonDominantButtonPresser.AddComponent<ButtonPresser>().isLeft     = true;

        foreach (Transform child in Keyboard.transform.GetChild(0))
        {
            if (child.name == "TMP")
                continue;

            foreach (Transform button in child)
                button.AddComponent<ButtonCollider>().OnPress =
                        () => HandleKeyboardButtonPress(button.GetComponentInChildren<TextMeshPro>().text);
        }
    }

    private void Update()
    {
        if (!MenuHandler.Instance.Menu.activeSelf && KeyboardOpen)
            CloseKeyboard();

        if (!KeyboardOpen)
            return;

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

        if (specialCharacters.TryGetValue(input, out Action special))
            special?.Invoke();
        else
            TypedText += isShiftLower ? isLower ? input.ToLower() : input.ToUpper() : input.ToUpper();

        if (!wasShiftLower)
            isShiftLower = true;

        Keyboard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text =
                string.IsNullOrEmpty(TypedText) ? "You haven't typed any text" : TypedText;

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
        nonDominantButtonPresser.SetActive(false);
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

        nonDominantButtonPresser.SetActive(true);
        nonDominantButtonPresser.transform.SetParent(RightHanded.IsEnabled
                                                             ? Tools.Utils.RealRightController
                                                             : Tools.Utils.RealLeftController);

        nonDominantButtonPresser.transform.localPosition = new Vector3(0f, -0.01f, 0.13f);

        Keyboard.SetActive(true);
    }
}