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
    public static List<(string, int)>     LastCategories     = [];
    public static Dictionary<string, int> CategoryPageMemory = new();

    public static bool       IsInitialised;
    public        string     Category = nameof(Main);
    public        int        PageIndex;
    public        GameObject ButtonPresser;
    public        bool       MenuOpen;

    public bool IsCanvasMenu;

    public Color currentMainColour, currentSecondaryColour;

    public  bool      IsWaiting;
    public  TMP_Text  MenuName;
    private Coroutine typingCoroutine;

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
            meshRenderer.material.shader = Shaders.UberShader;
            meshRenderer.material.color  = Plugin.Instance.MainColour;
        }

        ButtonPresser.GetComponent<SphereCollider>().isTrigger = true;
        ButtonPresser.AddComponent<Rigidbody>().isKinematic    = true;
        ButtonPresser.AddComponent<ButtonPresser>();

        List<hamburburmod> toRemove = [];

        ModRegistry.Clear();

        foreach (KeyValuePair<string, ValueTuple<Type, hamburburmod>[]> category in Buttons.Categories)
            for (int button = 0; button < category.Value.Length; button++)
            {
                Type modType = category.Value[button].Item1;

                if (Activator.CreateInstance(modType) is not hamburburmod hamburburMod)
                    continue;

                hamburburmodAttribute attribute = modType.GetCustomAttribute<hamburburmodAttribute>();

                if (attribute == null)
                {
                    Debug.LogError($"[hamburbur] {modType.FullName} is missing hamburburmodAttribute");

                    continue;
                }

                hamburburMod.AssociatedAttribute = attribute;

                ValueTuple<Type, hamburburmod> mod = category.Value[button];
                mod.Item2                                = hamburburMod;
                Buttons.Categories[category.Key][button] = mod;

                ModRegistry.Register(modType, hamburburMod);
            }

        foreach (KeyValuePair<string, ValueTuple<Type, hamburburmod>[]> category in Buttons.Categories)
        {
            foreach ((Type modType, hamburburmod modComp) in category.Value)
            {
                if (modComp == null)
                    continue;

                modComp.InvokeStart();

                if (modComp.AssociatedAttribute.AccessSetting.IsCurrentlyAccessible())
                    continue;

                if (!ButtonHandler.InaccessibleButtons.ContainsKey(modComp.AssociatedAttribute.AccessSetting))
                    ButtonHandler.InaccessibleButtons.Add(modComp.AssociatedAttribute.AccessSetting, []);

                ButtonHandler.InaccessibleButtons[modComp.AssociatedAttribute.AccessSetting]
                             .Add((category.Key, modType));

                toRemove.Add(modComp);
            }
        }

        foreach (hamburburmod modComp in toRemove)
            ButtonHandler.RemoveButton(modComp);

        Plugin.Instance.ComponentHolder.AddComponent<GUIHandler>();

        StartCoroutine(OnStart());
    }

    private void Update()
    {
        if (Menu == null || InputManager.Instance == null)
            return;

        if (KeyboardManager.Instance != null && KeyboardManager.Instance.KeyboardOpen)
            return;

        InputManager inputs = InputManager.Instance;

        if (!ToggleMenu.IsEnabled)
        {
            if (RightHanded.IsEnabled ? inputs.RightSecondary.WasPressed : inputs.LeftSecondary.WasPressed)
                StartCoroutine(OpenMenu());

            if (RightHanded.IsEnabled ? inputs.RightSecondary.WasReleased : inputs.LeftSecondary.WasReleased)
                StartCoroutine(Menu.transform.parent == Tools.Utils.RealLeftController
                                       ? CloseMenu()
                                       : GUIHandler.Instance.CloseMenu());
        }
        else if (RightHanded.IsEnabled ? inputs.RightSecondary.WasPressed : inputs.LeftSecondary.WasPressed)
        {
            if (MenuOpen)
                StartCoroutine(Menu.transform.parent == Tools.Utils.RealLeftController
                                       ? CloseMenu()
                                       : GUIHandler.Instance.CloseMenu());
            else
                StartCoroutine(OpenMenu());
        }
    }

    private void OnDisable() => CoroutineManager.Instance.StartCoroutine(CloseMenu());

    public void SetUpMenu(GameObject menuPrefab, Transform menuParent,      Vector3 position, Quaternion rotation,
                          Color      mainColour, Color     secondaryColour, float keyboardHeight, bool isCanvasMenu, bool    active)
    {
        if (menuPrefab == null || menuParent == null)
        {
            Debug.LogError("[hamburbur] Cannot create menu because its prefab or parent is null.");

            return;
        }

        CategoryPageMemory.Clear();

        IsCanvasMenu = isCanvasMenu;

        GameObject previousMenu = Menu;
        GameObject menu         = Instantiate(menuPrefab, menuParent);

        menu.RecursivelySetLayer(UnityLayer.IgnoreRaycast);
        menu.SetActive(false);

        menu.transform.localPosition = position;
        menu.transform.localRotation = rotation;

        Transform version     = menu.transform.Find(nameof(Version));
        Transform title       = menu.transform.Find("Title");
        Transform miscButtons = menu.transform.Find("MiscButtons");
        Transform modButtons  = menu.transform.Find("ModButtons");

        if (version == null || title == null || miscButtons == null || modButtons == null)
        {
            Debug.LogError($"[hamburbur] Theme {menuPrefab.name} is missing required objects.");
            menu.Obliterate();

            return;
        }

        Transform disconnect   = miscButtons.Find("Disconnect");
        Transform lastPage     = miscButtons.Find("LastPage");
        Transform nextPage     = miscButtons.Find("NextPage");
        Transform returnButton = miscButtons.Find("Return");

        if (disconnect == null || lastPage == null || nextPage == null || returnButton == null)
        {
            Debug.LogError($"[hamburbur] Theme {menuPrefab.name} is missing required misc buttons.");
            menu.Obliterate();

            return;
        }

        Menu     = menu;
        MenuName = title.GetComponent<TMP_Text>();

        currentMainColour      = mainColour;
        currentSecondaryColour = secondaryColour;

        if (ButtonPresser != null && ButtonPresser.TryGetComponent(out Renderer buttonPresserRenderer))
            buttonPresserRenderer.material.color = mainColour;

        KeyboardManager.Instance?.UpdateColoursAndHeight(mainColour, secondaryColour, keyboardHeight);

        version.GetComponent<TMP_Text>().text = $"v{Constants.PluginVersion}";

        disconnect.AddComponent<ButtonCollider>().OnPress   = () => NetworkSystem.Instance.ReturnToSinglePlayer();
        lastPage.AddComponent<ButtonCollider>().OnPress     = LastPage;
        nextPage.AddComponent<ButtonCollider>().OnPress     = NextPage;
        returnButton.AddComponent<ButtonCollider>().OnPress = ReturnToLastCategory;

        modButtons.gameObject.AddComponent<ButtonHandler>().Initialize();

        previousMenu?.Obliterate();

        if (active)
            StartCoroutine(SetActiveAfterAFrame());
    }

    private void ReturnToLastCategory()
    {
        if (LastCategories.Count < 2)
        {
            ButtonHandler.Instance.SetCategory(nameof(Main), false);

            return;
        }

        (string lastCategory, int lastPageIndex) = LastCategories[^2];

        if (Buttons.Categories.TryGetValue(lastCategory, out (Type, hamburburmod)[] category))
        {
            int maxPage =
                    Mathf.CeilToInt((float)category.Length / ButtonHandler.ButtonsPerPage) - 1;

            if (lastPageIndex > maxPage)
                lastPageIndex = 0;
        }

        LastCategories.RemoveAt(LastCategories.Count - 1);

        ButtonHandler.Instance.SetCategory(lastCategory, false);
        CategoryPageMemory[lastCategory] = lastPageIndex;
        Instance.PageIndex               = lastPageIndex;
        ButtonHandler.Instance.UpdateButtons();
    }

    private IEnumerator SetActiveAfterAFrame()
    {
        yield return new WaitForEndOfFrame();
        Menu.SetActive(true);

        IsInitialised = true;
    }

    private IEnumerator OnStart()
    {
        while (Menu == null || FileManager.Instance == null)
            yield return null;

        yield return new WaitForSeconds(0.1f);

        if (Menu == null)
            yield break;

        Menu.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        if (Menu != null)
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
            Menu.transform.parent.localScale = Vector3.Lerp(Vector3.zero,
                    Vector3.one * (ChangeMenuSize.Instance.IncrementalValue * 0.1f), t);

            ButtonPresser.transform.localScale = Vector3.Lerp(Vector3.zero,
                    Vector3.one * (ChangePointerSize.Instance.IncrementalValue * 0.002f), t);

            yield return null;
        }

        Menu.transform.parent.localScale   = Vector3.one * (ChangeMenuSize.Instance.IncrementalValue    * 0.1f);
        ButtonPresser.transform.localScale = Vector3.one * (ChangePointerSize.Instance.IncrementalValue * 0.002f);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeMenuTitle());

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

        MenuName.text = "";
    }

    private IEnumerator TypeMenuTitle(float typingSpeed = 0.2f, float pauseTime = 2f)
    {
        while (Menu.activeSelf)
        {
            string text = GetMenuTitle();

            for (int i = 0; i <= text.Length && Menu.activeSelf; i++)
            {
                MenuName.text = text[..i];

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
                MenuName.text = text[..i];

                yield return new WaitForSeconds(typingSpeed / 2f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        MenuName.text = "";
    }

    public void RefreshMenuTitle()
    {
        if (MenuName == null)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (Menu == null || !Menu.activeSelf)
        {
            MenuName.text = "";

            return;
        }

        typingCoroutine = StartCoroutine(TypeMenuTitle());
    }

    private static string GetMenuTitle()
    {
        if (!string.IsNullOrEmpty(CustomMenuName.CurrentName))
            return CustomMenuName.CurrentName;

        return MenuTitleThemeName.IsEnabled
                       ? Themes.AllThemes[Themes.Instance.IncrementalValue].Item2
                       : Constants.PluginName;
    }
}