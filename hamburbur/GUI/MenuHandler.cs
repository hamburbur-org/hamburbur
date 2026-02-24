using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hamburbur.Components;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using TMPro;
using UnityEngine;

namespace hamburbur.GUI;

public class MenuHandler : Singleton<MenuHandler>
{
    public static List<(string, int)> LastCategories = [];
    public        string              Category       = "Main";
    public        int                 PageIndex;
    public        GameObject          ButtonPresser;
    public        bool                MenuOpen;

    public  bool        IsWaiting;
    private TextMeshPro menuName;
    private Coroutine   typingCoroutine;

    public GameObject Menu { get; private set; }

    private void Start()
    {
        ButtonPresser = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        ButtonPresser.name  = "hamburbur button presser";
        ButtonPresser.layer = 2;
        ButtonPresser.SetActive(false);

        ButtonPresser.transform.SetParent(Tools.Utils.RealRightController);
        ButtonPresser.transform.localPosition = new Vector3(0f, -0.01f, 0.13f);
        ButtonPresser.transform.localScale    = Vector3.zero;

        if (ButtonPresser.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer.material.shader = Plugin.Instance.UberShader;
            meshRenderer.material.color  = Plugin.Instance.MainColour;
        }

        ButtonPresser.GetComponent<SphereCollider>().isTrigger = true;
        ButtonPresser.AddComponent<Rigidbody>().isKinematic    = true;
        ButtonPresser.AddComponent<ButtonPresser>();

        List<hamburburmod> toRemove = [];

        foreach (KeyValuePair<string, ValueTuple<Type, hamburburmod>[]> category in Buttons.Categories)
            for (int button = 0; button < Buttons.Categories[category.Key].Length; button++)
            {
                ValueTuple<Type, hamburburmod> mod = Buttons.Categories[category.Key][button];

                object modComponent = Activator.CreateInstance(mod.Item1);

                if (modComponent is not hamburburmod hamburburMod)
                    continue;

                hamburburMod.AssociatedAttribute         = mod.Item1.GetCustomAttribute<hamburburmodAttribute>();
                mod.Item2                                = hamburburMod;
                hamburburMod.IncrementalValue            = hamburburMod.AssociatedAttribute.IncrementalValue;
                Buttons.Categories[category.Key][button] = mod;
                mod.Item2.InvokeStart();

                if (mod.Item2.AssociatedAttribute.AccessSetting.IsCurrentlyAccessible())
                    continue;

                if (!ButtonHandler.InaccessibleButtons.ContainsKey(mod.Item2.AssociatedAttribute.AccessSetting))
                    ButtonHandler.InaccessibleButtons.Add(mod.Item2.AssociatedAttribute.AccessSetting, []);

                ButtonHandler.InaccessibleButtons[mod.Item2.AssociatedAttribute.AccessSetting]
                             .Add((category.Key, mod.Item1));

                toRemove.Add(mod.Item2);
            }

        foreach (hamburburmod modComp in toRemove)
            ButtonHandler.RemoveButton(modComp);

        Plugin.Instance.ComponentHolder.AddComponent<GUIHandler>();

        StartCoroutine(OnStart());
    }

    private void Update()
    {
        if (KeyboardManager.Instance.KeyboardOpen)
            return;

        InputManager inputs = InputManager.Instance;

        if (RightHanded.IsEnabled ? inputs.RightSecondary.WasPressed : inputs.LeftSecondary.WasPressed)
            StartCoroutine(OpenMenu());

        if (RightHanded.IsEnabled ? inputs.RightSecondary.WasReleased : inputs.LeftSecondary.WasReleased)
            StartCoroutine(Menu.transform.parent == Tools.Utils.RealLeftController
                                   ? CloseMenu()
                                   : GUIHandler.Instance.CloseMenu());
    }

    private void OnDisable() => CoroutineManager.Instance.StartCoroutine(CloseMenu());

    public void SetUpMenu(GameObject menuPrefab, Transform menuParent, Vector3 position, Quaternion rotation,
                          Color      mainColour, bool      active)
    {
        if (Menu != null)
        {
            Menu.Obliterate();
            Menu = null;
        }

        if (ButtonPresser != null)
            ButtonPresser.GetComponent<Renderer>().material.color = mainColour;

        GameObject menu = Instantiate(menuPrefab, menuParent);
        menu.RecursivelySetLayer(UnityLayer.IgnoreRaycast);
        menu.SetActive(false);

        menu.transform.localPosition = position;
        menu.transform.localRotation = rotation;

        Menu = menu;
        Menu.transform.Find("Version").GetComponent<TextMeshPro>().text = $"v{Constants.PluginVersion}";
        menuName = Menu.transform.Find("Title").GetComponent<TextMeshPro>();

        Transform miscButtons = Menu.transform.Find("MiscButtons");
        miscButtons.Find("Disconnect").AddComponent<ButtonCollider>().OnPress =
                () => NetworkSystem.Instance.ReturnToSinglePlayer();

        miscButtons.Find("LastPage").AddComponent<ButtonCollider>().OnPress = LastPage;
        miscButtons.Find("NextPage").AddComponent<ButtonCollider>().OnPress = NextPage;
        miscButtons.Find("Return").AddComponent<ButtonCollider>().OnPress   = ReturnToLastCategory;

        Menu.transform.Find("ModButtons").AddComponent<ButtonHandler>().Initialize();

        if (active)
            StartCoroutine(SetActiveAfterAFrame());
    }

    private void ReturnToLastCategory()
    {
        if (LastCategories.Count < 2)
        {
            ButtonHandler.Instance.SetCategory("Main", false);

            return;
        }

        (string lastCategory, int lastPageIndex) = LastCategories[^2];
        ButtonHandler.Instance.SetCategory(lastCategory, false);
        PageIndex = lastPageIndex;
        ButtonHandler.Instance.UpdateButtons();
        LastCategories.RemoveAt(LastCategories.Count - 2);
    }

    private IEnumerator SetActiveAfterAFrame()
    {
        yield return new WaitForEndOfFrame();
        Menu.SetActive(true);
    }

    private IEnumerator OnStart()
    {
        yield return new WaitForSeconds(0.1f);
        Menu.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(CloseMenu());
        FileManager.Instance.LoadSaveData();
    }

    private void LastPage()
    {
        PageIndex--;

        switch (Category)
        {
            case "Enabled Mods":
            {
                ValueTuple<Type, hamburburmod>[] enabledMods = Buttons.GetEnabledMods();

                if (PageIndex < 0)
                    PageIndex = Mathf.CeilToInt((float)enabledMods.Length / ButtonHandler.ButtonsPerPage) - 1;

                break;
            }

            case "Search":
            {
                int count = ButtonHandler.GetAllMods()
                                         .Count(x =>
                                                        string.IsNullOrEmpty(ButtonHandler.SearchState.Query) ||
                                                        x.Item2.ModName.Contains(ButtonHandler.SearchState.Query,
                                                                StringComparison.OrdinalIgnoreCase) ||
                                                        x.Item2.AssociatedAttribute.Description.Contains(
                                                                ButtonHandler.SearchState.Query,
                                                                StringComparison.OrdinalIgnoreCase));

                if (PageIndex < 0)
                    PageIndex = Mathf.CeilToInt((float)count / ButtonHandler.ButtonsPerPage) - 1;

                break;
            }

            default:
            {
                if (PageIndex < 0)
                    PageIndex = Mathf.CeilToInt(
                                        (float)Buttons.Categories[Category].Length / ButtonHandler.ButtonsPerPage) - 1;

                break;
            }
        }

        ButtonHandler.Instance.UpdateButtons();
    }

    private void NextPage()
    {
        PageIndex++;

        switch (Category)
        {
            case "Enabled Mods":
            {
                ValueTuple<Type, hamburburmod>[] enabledMods = Buttons.GetEnabledMods();

                if (PageIndex >= Mathf.CeilToInt((float)enabledMods.Length / ButtonHandler.ButtonsPerPage))
                    PageIndex = 0;

                break;
            }

            case "Search":
            {
                int count = ButtonHandler.GetAllMods()
                                         .Count(x =>
                                                        string.IsNullOrEmpty(ButtonHandler.SearchState.Query) ||
                                                        x.Item2.ModName.Contains(ButtonHandler.SearchState.Query,
                                                                StringComparison.OrdinalIgnoreCase) ||
                                                        x.Item2.AssociatedAttribute.Description.Contains(
                                                                ButtonHandler.SearchState.Query,
                                                                StringComparison.OrdinalIgnoreCase));

                if (PageIndex >= Mathf.CeilToInt((float)count / ButtonHandler.ButtonsPerPage))
                    PageIndex = 0;

                break;
            }

            default:
            {
                if (PageIndex >= Mathf.CeilToInt(
                            (float)Buttons.Categories[Category].Length / ButtonHandler.ButtonsPerPage))
                    PageIndex = 0;

                break;
            }
        }

        ButtonHandler.Instance.UpdateButtons();
    }

    public IEnumerator OpenMenu()
    {
        while (IsWaiting)
            yield return null;

        MenuOpen = true;

        IsWaiting = true;

        Plugin.Instance.PlaySound(DynamicMenuSounds.IsEnabled
                                          ? MenuSoundsHandler.Instance.MenuDynamicOpenSound
                                          : MenuSoundsHandler.Instance.MenuOpenSound);

        Menu.SetActive(true);
        ButtonPresser.SetActive(true);

        float startTime = Time.time;

        while (Time.time - startTime < 0.1f)
        {
            float t = (Time.time - startTime) / 0.1f;
            Menu.transform.parent.localScale   = Vector3.Lerp(Vector3.zero, Vector3.one,         t);
            ButtonPresser.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 0.01f, t);

            yield return null;
        }

        Menu.transform.parent.localScale   = Vector3.one;
        ButtonPresser.transform.localScale = Vector3.one * 0.01f;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeMenuTitle(Constants.PluginName));

        IsWaiting = false;
    }

    public IEnumerator CloseMenu()
    {
        while (IsWaiting)
            yield return null;

        MenuOpen = false;

        IsWaiting = true;

        if (DynamicMenuSounds.IsEnabled)
            Plugin.Instance.PlaySound(MenuSoundsHandler.Instance.MenuDynamicCloseSound);

        float startTime = Time.time;

        while (Time.time - startTime < 0.1f)
        {
            float t = (Time.time - startTime) / 0.1f;
            Menu.transform.parent.localScale   = Vector3.Lerp(Menu.transform.parent.localScale,   Vector3.zero, t);
            ButtonPresser.transform.localScale = Vector3.Lerp(ButtonPresser.transform.localScale, Vector3.zero, t);

            yield return null;
        }

        Menu.transform.parent.localScale   = Vector3.zero;
        ButtonPresser.transform.localScale = Vector3.zero;

        Menu.SetActive(false);
        ButtonPresser.SetActive(false);
        IsWaiting = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        menuName.text = "";
    }

    private IEnumerator TypeMenuTitle(string text, float typingSpeed = 0.2f, float pauseTime = 2f)
    {
        while (Menu.activeSelf)
        {
            for (int i = 0; i <= text.Length && Menu.activeSelf; i++)
            {
                menuName.text = text.Substring(0, i);

                yield return new WaitForSeconds(typingSpeed);
            }

            float timer = 0f;
            while (timer < pauseTime && Menu.activeSelf)
            {
                timer += Time.deltaTime;

                yield return null;
            }

            for (int i = text.Length; i >= 0 && Menu.activeSelf; i--)
            {
                menuName.text = text.Substring(0, i);

                yield return new WaitForSeconds(typingSpeed / 2f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        menuName.text = "";
    }
}