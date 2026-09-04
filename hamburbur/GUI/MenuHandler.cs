using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hamburbur.Components;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Categories;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using TMPro;
using UnityEngine;

namespace hamburbur.GUI;

public class MenuHandler : Singleton<MenuHandler>
{
    private const string DedicatedCategoryDefault = nameof(Movement);

    public static List<(string, int)>     LastCategories     = [];
    public static Dictionary<string, int> CategoryPageMemory = new();

    public static bool       IsInitialised;
    public        string     Category = nameof(Main);
    public        int        PageIndex;
    public        GameObject ButtonPresser;
    public        bool       MenuOpen;

    public bool IsCanvasMenu;

    public Color currentMainColour, currentSecondaryColour;

    public bool     IsWaiting;
    public TMP_Text MenuName;

    private readonly List<CategoryButton> categoryButtonSlots = [];
    private          Transform            categoryLastPage;
    private          Transform            categoryNextPage;
    private          int                  categoryPageIndex;
    private          Coroutine            typingCoroutine;
    public           bool                 HasDedicatedSearchButton    { get; private set; }
    public           bool                 HasDedicatedCategoryButtons { get; private set; }

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
                Type         modType      = category.Value[button].Item1;
                hamburburmod hamburburMod = category.Value[button].Item2;

                if (hamburburMod == null)
                {
                    if (Activator.CreateInstance(modType) is not hamburburmod createdMod)
                        continue;

                    hamburburMod = createdMod;
                }

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

        AssignDuplicateConfigKeys();

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
        InputManager.ControllerButton menuButton = RightHanded.IsEnabled
                                                           ? PrimaryMenuButton.IsEnabled
                                                                     ? inputs.RightPrimary
                                                                     : inputs.RightSecondary
                                                           : PrimaryMenuButton.IsEnabled
                                                                   ? inputs.LeftPrimary
                                                                   : inputs.LeftSecondary;

        if (!ToggleMenu.IsEnabled)
        {
            if (menuButton.WasPressed)
                StartCoroutine(OpenMenu());

            if (menuButton.WasReleased)
                StartCoroutine(Menu.transform.parent == Tools.Utils.RealLeftController
                                       ? CloseMenu()
                                       : GUIHandler.Instance.CloseMenu());
        }
        else if (menuButton.WasPressed)
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

    private static void AssignDuplicateConfigKeys()
    {
        IEnumerable<IGrouping<string, (Type, hamburburmod)>> duplicateNames =
                Buttons.Categories
                       .SelectMany(category => category.Value)
                       .Where(button => button.Item2?.AssociatedAttribute != null)
                       .GroupBy(button => button.Item2.PreferencesKey)
                       .Where(group => group.Count() > 1);

        foreach (IGrouping<string, (Type, hamburburmod)> duplicateName in duplicateNames)
            foreach ((Type type, hamburburmod mod) in duplicateName)
                mod.ConfigKey = $"{duplicateName.Key}_{type.FullName ?? type.Name}";
    }

    public void SetUpMenu(GameObject menuPrefab, Transform menuParent,      Vector3 position,       Quaternion rotation,
                          Color      mainColour, Color     secondaryColour, float   keyboardHeight, bool       isCanvasMenu, bool active)
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

        Transform version         = menu.transform.Find(nameof(Version));
        Transform title           = menu.transform.Find("Title");
        Transform miscButtons     = menu.transform.Find("MiscButtons");
        Transform modButtons      = menu.transform.Find("ModButtons");
        Transform categoryButtons = menu.transform.Find("CategoryButtons");

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
        Transform searchButton = miscButtons.Find("Search");

        if (disconnect == null || lastPage == null || nextPage == null || returnButton == null)
        {
            Debug.LogError($"[hamburbur] Theme {menuPrefab.name} is missing required misc buttons.");
            menu.Obliterate();

            return;
        }

        StopMenuTitleAnimation(false);

        Menu     = menu;
        MenuName = title.GetComponent<TMP_Text>();
        ResetMenuTitleVisuals(MenuName);
        MenuName.text = GetMenuTitle();

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

        HasDedicatedSearchButton = searchButton != null;
        if (HasDedicatedSearchButton)
            searchButton.AddComponent<ButtonCollider>().OnPress = Search.OpenSearch;

        InitializeCategoryButtons(categoryButtons);

        if (HasDedicatedCategoryButtons && Category == nameof(Main))
        {
            Category  = DedicatedCategoryDefault;
            PageIndex = 0;
            LastCategories.Clear();
        }

        modButtons.gameObject.AddComponent<ButtonHandler>().Initialize();

        previousMenu?.Obliterate();

        if (active)
            StartCoroutine(SetActiveAfterAFrame(menu));
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

        ButtonHandler.Instance.SetCategory(lastCategory, false, false);
        CategoryPageMemory[lastCategory] = lastPageIndex;
        Instance.PageIndex               = lastPageIndex;
        ButtonHandler.Instance.UpdateButtons(true);
    }

    private void InitializeCategoryButtons(Transform categoryButtons)
    {
        categoryButtonSlots.Clear();
        categoryLastPage            = null;
        categoryNextPage            = null;
        categoryPageIndex           = 0;
        HasDedicatedCategoryButtons = categoryButtons != null;

        if (!HasDedicatedCategoryButtons)
            return;

        for (int index = 1;; index++)
        {
            Transform button = categoryButtons.Find($"Button{index}");

            if (button == null)
                break;

            Transform text = button.Find("TMP");

            if (text == null || !text.TryGetComponent(out TMP_Text tmp))
            {
                Debug.LogWarning($"[hamburbur] CategoryButtons/Button{index} is missing its TMP text object.");
                button.gameObject.SetActive(false);

                continue;
            }

            categoryButtonSlots.Add(new CategoryButton(
                    button.gameObject,
                    button.gameObject.AddComponent<ButtonCollider>(),
                    tmp));
        }

        categoryLastPage = categoryButtons.Find("LastPage");
        categoryNextPage = categoryButtons.Find("NextPage");

        if (categoryLastPage != null)
            categoryLastPage.gameObject.AddComponent<ButtonCollider>().OnPress = LastCategoryPage;

        if (categoryNextPage != null)
            categoryNextPage.gameObject.AddComponent<ButtonCollider>().OnPress = NextCategoryPage;

        UpdateCategoryButtons();
    }

    public void UpdateCategoryButtons()
    {
        if (!HasDedicatedCategoryButtons || categoryButtonSlots.Count == 0)
            return;

        (Type, hamburburmod)[] mainButtons = Buttons.GetVisibleCategory(nameof(Main));
        int                    pageCount   = Mathf.Max(1, Mathf.CeilToInt((float)mainButtons.Length / categoryButtonSlots.Count));
        categoryPageIndex = Mathf.Clamp(categoryPageIndex, 0, pageCount - 1);

        foreach (CategoryButton slot in categoryButtonSlots)
            slot.GameObject.SetActive(false);

        int firstIndex = categoryPageIndex * categoryButtonSlots.Count;

        for (int slotIndex = 0; slotIndex < categoryButtonSlots.Count; slotIndex++)
        {
            int buttonIndex = firstIndex + slotIndex;

            if (buttonIndex >= mainButtons.Length)
                break;

            hamburburmod mod = mainButtons[buttonIndex].Item2;

            if (mod == null)
                continue;

            CategoryButton slot = categoryButtonSlots[slotIndex];
            slot.Text.text        = mod.ModName;
            slot.Collider.OnPress = () => mod.Toggle(ButtonState.Normal);
            slot.GameObject.SetActive(true);
        }

        bool hasMultiplePages = pageCount > 1;
        categoryLastPage?.gameObject.SetActive(hasMultiplePages);
        categoryNextPage?.gameObject.SetActive(hasMultiplePages);
    }

    private void LastCategoryPage()
    {
        int pageCount = GetCategoryPageCount();
        categoryPageIndex = (categoryPageIndex - 1 + pageCount) % pageCount;
        UpdateCategoryButtons();
    }

    private void NextCategoryPage()
    {
        int pageCount = GetCategoryPageCount();
        categoryPageIndex = (categoryPageIndex + 1) % pageCount;
        UpdateCategoryButtons();
    }

    private int GetCategoryPageCount() => categoryButtonSlots.Count == 0
                                                  ? 1
                                                  : Mathf.Max(1, Mathf.CeilToInt(
                                                          (float)Buttons.GetVisibleCategory(nameof(Main)).Length /
                                                          categoryButtonSlots.Count));

    private IEnumerator SetActiveAfterAFrame(GameObject menu)
    {
        yield return new WaitForEndOfFrame();

        if (menu == null || menu != Menu)
            yield break;

        Menu.SetActive(MenuOpen);

        if (MenuOpen)
        {
            ButtonHandler.Instance?.PlayTransitionAnimation();
            RefreshMenuTitle();
        }

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
            StartCoroutine(CloseMenu(false, false));

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
                                        (float)Buttons.GetVisibleCategory(Category).Length /
                                        ButtonHandler.ButtonsPerPage) - 1;

                break;
            }
        }

        ButtonHandler.Instance.UpdateButtons(true);
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
                            (float)Buttons.GetVisibleCategory(Category).Length / ButtonHandler.ButtonsPerPage))
                    PageIndex = 0;

                break;
            }
        }

        ButtonHandler.Instance.UpdateButtons(true);
    }

    public IEnumerator OpenMenu()
    {
        while (IsWaiting)
            yield return null;

        MenuOpen = true;

        IsWaiting = true;

        MenuSoundsHandler.Instance?.PlayMenuOpenSound();

        Menu.SetActive(true);
        ButtonPresser.SetActive(true);
        ButtonHandler.Instance?.PlayTransitionAnimation();

        GameObject animatedMenu     = Menu;
        int        animationIndex   = MenuOpenCloseAnimation.CurrentIndex;
        Vector3    targetLocalScale = animatedMenu.transform.localScale;
        Vector3    targetMenuScale  = Vector3.one * (ChangeMenuSize.Instance.IncrementalValue * 0.1f);
        Vector3 targetPointerScale =
                Vector3.one * (ChangePointerSize.Instance.IncrementalValue * 0.002f);

        float duration = GetMenuAnimationDuration(animationIndex);
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            if (animatedMenu == null || animatedMenu != Menu)
                break;

            elapsed += Time.unscaledDeltaTime;
            ApplyMenuAnimation(
                    animatedMenu,
                    animationIndex,
                    true,
                    Mathf.Clamp01(elapsed / duration),
                    targetLocalScale,
                    targetMenuScale,
                    targetPointerScale);

            yield return null;
        }

        if (animatedMenu == Menu)
        {
            ApplyMenuAnimation(
                    animatedMenu,
                    animationIndex,
                    true,
                    1f,
                    targetLocalScale,
                    targetMenuScale,
                    targetPointerScale);
        }
        else
        {
            Menu.transform.parent.localScale = Vector3.one * (ChangeMenuSize.Instance.IncrementalValue * 0.1f);
            ButtonPresser.transform.localScale =
                    Vector3.one * (ChangePointerSize.Instance.IncrementalValue * 0.002f);
        }

        RefreshMenuTitle();

        IsWaiting = false;
    }

    public IEnumerator CloseMenu(bool animate = true, bool playSound = true)
    {
        while (IsWaiting)
            yield return null;

        MenuOpen = false;

        IsWaiting = true;

        if (playSound)
            MenuSoundsHandler.Instance?.PlayMenuCloseSound();

        GameObject animatedMenu     = Menu;
        int        animationIndex   = animate ? MenuOpenCloseAnimation.CurrentIndex : 5;
        Vector3    targetLocalScale = animatedMenu.transform.localScale;
        Vector3    targetMenuScale  = Vector3.one * (ChangeMenuSize.Instance.IncrementalValue * 0.1f);
        Vector3 targetPointerScale =
                Vector3.one * (ChangePointerSize.Instance.IncrementalValue * 0.002f);

        float duration = GetMenuAnimationDuration(animationIndex);
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            if (animatedMenu == null || animatedMenu != Menu)
                break;

            elapsed += Time.unscaledDeltaTime;
            ApplyMenuAnimation(
                    animatedMenu,
                    animationIndex,
                    false,
                    Mathf.Clamp01(elapsed / duration),
                    targetLocalScale,
                    targetMenuScale,
                    targetPointerScale);

            yield return null;
        }

        Menu.transform.parent.localScale   = Vector3.zero;
        ButtonPresser.transform.localScale = Vector3.zero;

        if (animatedMenu != null)
            animatedMenu.transform.localScale = targetLocalScale;

        Menu.SetActive(false);
        ButtonPresser.SetActive(false);
        IsWaiting = false;

        StopMenuTitleAnimation(true);
    }

    private void ApplyMenuAnimation(
            GameObject animatedMenu,
            int        animationIndex,
            bool       opening,
            float      progress,
            Vector3    targetLocalScale,
            Vector3    targetMenuScale,
            Vector3    targetPointerScale)
    {
        if (animatedMenu == null)
            return;

        float   eased          = Mathf.SmoothStep(0f, 1f, progress);
        float   visibility     = opening ? eased : 1f - eased;
        Vector3 menuScale      = targetMenuScale;
        Vector3 menuLocalScale = targetLocalScale;

        switch (animationIndex)
        {
            case 0:
                menuScale = targetMenuScale * visibility;

                break;

            case 1:
                menuScale = targetMenuScale * GetPopScale(progress, opening);

                break;

            case 5:
                menuScale  = opening ? targetMenuScale : Vector3.zero;
                visibility = opening ? 1f : 0f;

                break;

            case 6:
                menuLocalScale = Vector3.Scale(
                        targetLocalScale,
                        new Vector3(Mathf.Lerp(0.025f, 1f, visibility), 1f, 1f));

                break;

            case 7:
                menuLocalScale = Vector3.Scale(
                        targetLocalScale,
                        new Vector3(1f, Mathf.Lerp(0.025f, 1f, visibility), 1f));

                break;

            case 8:
                menuLocalScale = Vector3.Scale(
                        targetLocalScale,
                        new Vector3(
                                Mathf.Lerp(1.3f,  1f, visibility),
                                Mathf.Lerp(0.04f, 1f, visibility),
                                1f));

                break;

            case 9:
                menuLocalScale = Vector3.Scale(
                        targetLocalScale,
                        new Vector3(
                                Mathf.Lerp(0.04f, 1f, visibility),
                                Mathf.Lerp(1.3f,  1f, visibility),
                                1f));

                break;

        }

        animatedMenu.transform.parent.localScale = menuScale;
        animatedMenu.transform.localScale        = menuLocalScale;
        ButtonPresser.transform.localScale       = targetPointerScale * visibility;
    }

    private static float GetPopScale(float progress, bool opening)
    {
        if (opening)
        {
            if (progress < 0.7f)
                return Mathf.Lerp(0f, 1.12f, Mathf.SmoothStep(0f, 1f, progress / 0.7f));

            return Mathf.Lerp(1.12f, 1f, Mathf.SmoothStep(0f, 1f, (progress - 0.7f) / 0.3f));
        }

        if (progress < 0.25f)
            return Mathf.Lerp(1f, 1.08f, Mathf.SmoothStep(0f, 1f, progress / 0.25f));

        return Mathf.Lerp(1.08f, 0f, Mathf.SmoothStep(0f, 1f, (progress - 0.25f) / 0.75f));
    }

    private static float GetMenuAnimationDuration(int animationIndex) => animationIndex switch
                                                                         {
                                                                                 1                => 0.16f,
                                                                                 6 or 7 or 8 or 9 => 0.18f,
                                                                                 5                => 0f,
                                                                                 var _            => 0.1f,
                                                                         };

    private IEnumerator AnimateMenuTitle()
    {
        TMP_Text   title = MenuName;
        GameObject menu  = Menu;

        if (!IsTitleAnimationActive(title, menu))
            yield break;

        ResetMenuTitleVisuals(title);

        switch (MenuTitleAnimation.CurrentAnimation)
        {
            case "None":
                title.text = GetMenuTitle();

                break;

            case "Fade":
                yield return FadeMenuTitle(title, menu);

                break;

            case "Reveal":
                yield return RevealMenuTitle(title, menu);

                break;

            case "Pulse":
                yield return PulseMenuTitle(title, menu);

                break;

            default:
                yield return TypewriterMenuTitle(title, menu);

                break;
        }
    }

    private IEnumerator TypewriterMenuTitle(TMP_Text title, GameObject menu)
    {
        while (IsTitleAnimationActive(title, menu))
        {
            string text = GetMenuTitle();

            for (int i = 0; i <= text.Length && IsTitleAnimationActive(title, menu); i++)
            {
                title.text = text[..i];

                yield return WaitForTitleDelay(title, menu, 0.2f);
            }

            yield return WaitForTitleDelay(title, menu, 2f);

            for (int i = text.Length; i >= 0 && IsTitleAnimationActive(title, menu); i--)
            {
                title.text = text[..i];

                yield return WaitForTitleDelay(title, menu, 0.1f);
            }

            yield return WaitForTitleDelay(title, menu, 0.5f);
        }
    }

    private IEnumerator FadeMenuTitle(TMP_Text title, GameObject menu)
    {
        while (IsTitleAnimationActive(title, menu))
        {
            title.text = GetMenuTitle();

            yield return FadeTitle(title, menu, 0f, 1f, 0.45f);
            yield return WaitForTitleDelay(title, menu, 2f);
            yield return FadeTitle(title, menu, 1f, 0f, 0.35f);
            yield return WaitForTitleDelay(title, menu, 0.35f);
        }
    }

    private IEnumerator RevealMenuTitle(TMP_Text title, GameObject menu)
    {
        while (IsTitleAnimationActive(title, menu))
        {
            string text = GetMenuTitle();
            title.text  = text;
            title.alpha = 1f;

            for (int visible = 0; visible <= text.Length && IsTitleAnimationActive(title, menu); visible++)
            {
                title.maxVisibleCharacters = visible;

                yield return WaitForTitleDelay(title, menu, 0.075f);
            }

            yield return WaitForTitleDelay(title, menu, 2f);

            for (int visible = text.Length; visible >= 0 && IsTitleAnimationActive(title, menu); visible--)
            {
                title.maxVisibleCharacters = visible;

                yield return WaitForTitleDelay(title, menu, 0.05f);
            }

            yield return WaitForTitleDelay(title, menu, 0.4f);
        }
    }

    private IEnumerator PulseMenuTitle(TMP_Text title, GameObject menu)
    {
        title.text = GetMenuTitle();
        float elapsed = 0f;

        while (IsTitleAnimationActive(title, menu))
        {
            elapsed     += Time.unscaledDeltaTime;
            title.alpha =  Mathf.Lerp(0.45f, 1f, (Mathf.Sin(elapsed * 3f) + 1f) * 0.5f);

            yield return null;
        }
    }

    private IEnumerator FadeTitle(
            TMP_Text   title,
            GameObject menu,
            float      from,
            float      to,
            float      duration)
    {
        float elapsed = 0f;

        while (elapsed < duration && IsTitleAnimationActive(title, menu))
        {
            elapsed     += Time.unscaledDeltaTime;
            title.alpha =  Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));

            yield return null;
        }

        if (IsTitleAnimationActive(title, menu))
            title.alpha = to;
    }

    private IEnumerator WaitForTitleDelay(TMP_Text title, GameObject menu, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration && IsTitleAnimationActive(title, menu))
        {
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    public void RefreshMenuTitle()
    {
        if (MenuName == null)
            return;

        StopMenuTitleAnimation(false);
        MenuName.text = GetMenuTitle();

        if (Menu == null || !Menu.activeSelf)
            return;

        typingCoroutine = StartCoroutine(AnimateMenuTitle());
    }

    private void StopMenuTitleAnimation(bool clearText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (MenuName == null)
            return;

        ResetMenuTitleVisuals(MenuName);

        if (clearText)
            MenuName.text = "";
    }

    private static void ResetMenuTitleVisuals(TMP_Text title)
    {
        if (title == null)
            return;

        title.alpha                = 1f;
        title.maxVisibleCharacters = int.MaxValue;
    }

    private static bool IsTitleAnimationActive(TMP_Text title, GameObject menu) =>
            title             != null  &&
            menu              != null  &&
            Instance          != null  &&
            Instance.Menu     == menu  &&
            Instance.MenuName == title &&
            menu.activeSelf;

    private static string GetMenuTitle()
    {
        if (!string.IsNullOrEmpty(CustomMenuName.CurrentName))
            return CustomMenuName.CurrentName;

        return MenuTitleThemeName.IsEnabled && Themes.Instance != null
                       ? Themes.AllThemes[Themes.Instance.IncrementalValue].Item2
                       : Constants.PluginName;
    }

    private readonly struct CategoryButton(GameObject gameObject, ButtonCollider collider, TMP_Text text)
    {
        public readonly GameObject     GameObject = gameObject;
        public readonly ButtonCollider Collider   = collider;
        public readonly TMP_Text       Text       = text;
    }
}