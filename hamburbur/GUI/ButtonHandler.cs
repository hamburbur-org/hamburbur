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
using hamburbur.Plugins;
using hamburbur.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace hamburbur.GUI;

public enum PromptType
{
    AcceptAndDeny,
    Continue,
    Keyboard,
}

/// <summary>
///     Represents data required to configure and display a user prompt, including its type, title, button visibility,
///     and associated actions triggered by user interaction.
/// </summary>
public class PromptData
{
    public PromptData(PromptType type, string title, Action onAcceptPress, Action onDenyPress, string acceptButtonText,
                      string     denyButtonText)
    {
        Type                = type;
        Title               = title;
        TopButtonText       = acceptButtonText;
        BottomButtonText    = denyButtonText;
        OnTopButtonPress    = onAcceptPress;
        OnBottomButtonPress = onDenyPress;

        HandleButtonVisibility(type);
    }

    public PromptData(PromptType type, string title, Action onContinuePress, string continueButtonText)
    {
        Type             = type;
        Title            = title;
        TopButtonText    = continueButtonText;
        OnTopButtonPress = onContinuePress;

        HandleButtonVisibility(type);
    }

    public PromptData(PromptType type, string title, Action<string> onKeyboardEnterPress, Action onKeyboardCancelPress)
    {
        Type                  = type;
        Title                 = title;
        OnKeyboardEnterPress  = onKeyboardEnterPress;
        OnKeyboardCancelPress = onKeyboardCancelPress;

        HandleButtonVisibility(type);
    }

    public PromptType Type { get; }

    public string Title { get; }

    public string TopButtonText    { get; }
    public string BottomButtonText { get; }

    public bool ShowTopButton    { get; private set; }
    public bool ShowBottomButton { get; private set; }

    public Action OnTopButtonPress    { get; }
    public Action OnBottomButtonPress { get; }

    public Action<string> OnKeyboardEnterPress  { get; }
    public Action         OnKeyboardCancelPress { get; }

    private void HandleButtonVisibility(PromptType type)
    {
        switch (type)
        {
            case PromptType.AcceptAndDeny:
                ShowTopButton    = true;
                ShowBottomButton = true;

                break;

            case PromptType.Continue:
                ShowTopButton    = true;
                ShowBottomButton = false;

                break;

            case PromptType.Keyboard:
                ShowTopButton    = false;
                ShowBottomButton = false;

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}

public class ButtonHandler : Singleton<ButtonHandler>
{
    private const float TransitionSlideDistance = 0.08f;

    public static Dictionary<string, ModSaveInfo> SavedModInfo = new();

    public static int ButtonsPerPage = 1; // DO NOT MODIFY IT GETS AUTO SET AT RUNTIME

    public static readonly Dictionary<AccessSetting, List<(string, Type)>> InaccessibleButtons  = new();
    private readonly       Dictionary<Transform, ButtonTransformState>     animatedButtonStates = new();

    private readonly List<PromptData> currentPrompts           = [];
    private readonly List<GameObject> visibleTransitionButtons = [];

    public ModButton[] ModButtons;

    private       Coroutine transitionCoroutine;
    public static int       UpdateRevision { get; private set; }

    private void OnDisable() => StopTransitionAnimation();

    public void Initialize()
    {
        Instance = this;

        List<ModButton> buttons = [];
        ButtonsPerPage = 1;

        while (transform.Find($"Button{ButtonsPerPage}") != null)
        {
            buttons.Add(GetButton(ButtonsPerPage));
            ButtonsPerPage++;
        }

        ButtonsPerPage--;

        ModButtons = buttons.ToArray();

        UpdateButtons();
    }

    [AccessSettingsAllowedCheck(AccessSetting.Public)]
    public static bool EnsurePublicIsAccessible() => true;

    [AccessSettingsAllowedCheck(AccessSetting.BetaBuildOnly)]
    public static bool BetaModsAccessible() => Constants.BetaBuild;

    public void SetCategory(string category, bool cacheLastCategory = true, bool animateTransition = true)
    {
        bool returningHome = category == nameof(Main);

        if (returningHome && MenuHandler.Instance.HasDedicatedCategoryButtons)
            category = nameof(Movement);

        MenuHandler.CategoryPageMemory[MenuHandler.Instance.Category] = MenuHandler.Instance.PageIndex;

        if (cacheLastCategory)
            MenuHandler.LastCategories.Add((category, MenuHandler.Instance.PageIndex));

        MenuHandler.Instance.Category = category;

        if (RememberLastCategory.IsEnabled && MenuHandler.CategoryPageMemory.TryGetValue(category, out int savedPage))
            MenuHandler.Instance.PageIndex = savedPage;
        else
            MenuHandler.Instance.PageIndex = 0;

        if (returningHome)
            MenuHandler.LastCategories.Clear();

        UpdateButtons(animateTransition);
    }

    /// <summary>
    ///     Displays a prompt on the user interface using the provided configuration data.
    ///     The prompt can include various buttons, user-defined actions triggered by interactions,
    ///     and input handling based on the specified <see cref="PromptData" />.
    /// </summary>
    /// <param name="promptData">
    ///     An object containing the configuration for the prompt, including type, title, button visibility,
    ///     text for top and bottom buttons, and associated actions triggered by button presses or keyboard inputs.
    /// </param>
    public void Prompt(PromptData promptData)
    {
        Debug.Log($"Prompt: {promptData.Title}");
        currentPrompts.Add(promptData);
        UpdateButtons();
    }

    public static hamburburmod AddButton(string category, Type mod)
    {
        object modComp = Activator.CreateInstance(mod);

        if (modComp is not hamburburmod hamburburmodComp)
            return null;

        return AddButton(category, hamburburmodComp, true, mod);
    }

    public static hamburburmod AddButton(string category,       hamburburmod hamburburmodComp, bool register = true,
                                         Type   modType = null, bool         loadSavedData = true)
    {
        if (hamburburmodComp == null)
            return null;

        modType ??= hamburburmodComp.GetType();

        if (!Buttons.Categories.ContainsKey(category))
            Buttons.Categories[category] = [];

        List<ValueTuple<Type, hamburburmod>> mods = Buttons.Categories[category].ToList();

        hamburburmodComp.LoadSavedDataWhenStartCalled =   loadSavedData;
        hamburburmodComp.AssociatedAttribute          ??= modType.GetCustomAttribute<hamburburmodAttribute>();

        if (hamburburmodComp.AssociatedAttribute == null)
        {
            Debug.LogError($"[hamburbur] {modType.FullName} is missing hamburburmodAttribute");
            return null;
        }

        mods.Add((modType, hamburburmodComp));
        Buttons.Categories[category] = mods.ToArray();

        if (register)
            ModRegistry.Register(modType, hamburburmodComp);
        hamburburmodComp.InvokeStart();

        GUIHandler guiHandler = GUIHandler.Instance;

        if (guiHandler                      != null &&
            guiHandler.Menu                 != null &&
            guiHandler.CategoryButtonPrefab != null &&
            guiHandler.ModButtonPrefab      != null)
        {
            if (category == nameof(Main))
            {
                Transform categoryContent =
                        guiHandler.Menu.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0);

                GameObject categoryButton = Instantiate(guiHandler.CategoryButtonPrefab, categoryContent);
                categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = hamburburmodComp.ModName;
                categoryButton.GetComponent<Button>().onClick
                              .AddListener(() => hamburburmodComp.Toggle(ButtonState.Normal));

                categoryButton.GetComponent<Button>().gameObject.GetOrAddComponent<ButtonPressAnimator>();
                hamburburmodComp.AssociatedGUIButton = categoryButton;
            }
            else
            {
                Transform  modContent = guiHandler.Menu.transform.GetChild(2).GetChild(0).GetChild(0);
                GameObject modButton  = Instantiate(guiHandler.ModButtonPrefab, modContent);
                modButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = hamburburmodComp.ModName;
                modButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                        hamburburmodComp.AssociatedAttribute.Description;

                modButton.transform.GetChild(2).GetChild(0).gameObject
                         .SetActive(hamburburmodComp.AssociatedAttribute.ButtonType != ButtonType.Incremental);

                modButton.transform.GetChild(2).GetChild(1).gameObject
                         .SetActive(hamburburmodComp.AssociatedAttribute.ButtonType == ButtonType.Incremental);

                Button normalButton = modButton.transform.GetChild(2).GetChild(0).GetComponent<Button>();
                Button minusButton  = modButton.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Button>();
                Button plusButton   = modButton.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<Button>();

                normalButton.onClick.AddListener(() => hamburburmodComp.Toggle(ButtonState.Normal));
                normalButton.gameObject.GetOrAddComponent<ButtonPressAnimator>();

                minusButton.gameObject.GetOrAddComponent<HoldButtonRepeater>().Configure(
                        () => hamburburmodComp.Toggle(ButtonState.Decrement),
                        () => hamburburmodComp.Toggle(ButtonState.Decrement, false));

                plusButton.gameObject.GetOrAddComponent<HoldButtonRepeater>().Configure(
                        () => hamburburmodComp.Toggle(ButtonState.Increment),
                        () => hamburburmodComp.Toggle(ButtonState.Increment, false));

                hamburburmodComp.AssociatedGUIButton = modButton;
            }
        }

        Instance?.UpdateButtons();

        return hamburburmodComp;
    }

    public static void RemoveButton(hamburburmod modComp)
    {
        KeyValuePair<string, (Type, hamburburmod)[]> categoryEntry =
                Buttons.Categories
                       .FirstOrDefault(cat => cat.Value.Any(button => button.Item2 == modComp));

        if (categoryEntry.Value == null)
            return;

        List<(Type, hamburburmod)> buttons = categoryEntry.Value.ToList();

        Type modType = buttons.FirstOrDefault(button => button.Item2 == modComp).Item1;

        if (modComp.Enabled && modComp.AssociatedAttribute.ButtonType == ButtonType.Togglable)
            modComp.Toggle(ButtonState.Normal, false, false);

        ModRuntime.Unregister(modComp);

        if (modType != null)
            ModRegistry.Unregister(modType);

        buttons.RemoveAll(button => button.Item2 == modComp);
        modComp.AssociatedGUIButton?.Obliterate();

        Buttons.Categories[categoryEntry.Key] = buttons.ToArray();
    }

    public void UpdateButtons(bool animateTransition = false)
    {
        UpdateRevision++;
        StopTransitionAnimation();
        GUIHandler.Instance?.UpdateButtons();
        MenuHandler.Instance?.UpdateCategoryButtons();

        for (int i = 0; i < ModButtons.Length; i++)
        {
            ModButtons[i].NormalButtonObject.SetActive(false);
            ModButtons[i].IncrementalButtonObject.SetActive(false);
        }

        if (currentPrompts.Count > 0)
        {
            PromptData prompt = currentPrompts[0];

            switch (prompt.Type)
            {
                case PromptType.AcceptAndDeny:
                    ModButtons[0].NormalButtonObject.SetActive(true);
                    ModButtons[0].NormalButtonObject.SetButtonRendererActive(false);
                    ModButtons[0].NormalButton.OnPress = null;
                    ModButtons[0].NormalTMP.text       = prompt.Title;

                    ModButtons[1].NormalButtonObject.SetActive(true);
                    ModButtons[1].NormalButtonObject.SetButtonRendererActive(true);
                    ModButtons[1].NormalTMP.text = prompt.TopButtonText;
                    ModButtons[1].NormalButton.OnPress = () =>
                                                         {
                                                             prompt.OnTopButtonPress?.Invoke();
                                                             currentPrompts.RemoveAt(0);
                                                             UpdateButtons();
                                                         };

                    ModButtons[2].NormalButtonObject.SetActive(true);
                    ModButtons[2].NormalButtonObject.SetButtonRendererActive(true);
                    ModButtons[2].NormalTMP.text = prompt.BottomButtonText;
                    ModButtons[2].NormalButton.OnPress = () =>
                                                         {
                                                             prompt.OnBottomButtonPress?.Invoke();
                                                             currentPrompts.RemoveAt(0);
                                                             UpdateButtons();
                                                         };

                    break;

                case PromptType.Continue:
                    ModButtons[0].NormalButtonObject.SetActive(true);
                    ModButtons[0].NormalButtonObject.SetButtonRendererActive(false);
                    ModButtons[0].NormalButton.OnPress = null;
                    ModButtons[0].NormalTMP.text       = prompt.Title;

                    ModButtons[1].NormalButtonObject.SetActive(true);
                    ModButtons[1].NormalButtonObject.SetButtonRendererActive(true);
                    ModButtons[1].NormalTMP.text = prompt.TopButtonText;
                    ModButtons[1].NormalButton.OnPress = () =>
                                                         {
                                                             prompt.OnTopButtonPress?.Invoke();
                                                             currentPrompts.RemoveAt(0);
                                                             UpdateButtons();
                                                         };

                    break;

                case PromptType.Keyboard:
                    ModButtons[0].NormalButtonObject.SetActive(true);
                    ModButtons[0].NormalButtonObject.SetButtonRendererActive(false);
                    ModButtons[0].NormalButton.OnPress = null;
                    ModButtons[0].NormalTMP.text       = prompt.Title;

                    if (KeyboardManager.Instance != null && !KeyboardManager.Instance.KeyboardOpen)
                    {
                        KeyboardManager.Instance.SpawnKeyboard(typedText =>
                                                               {
                                                                   currentPrompts.RemoveAt(0);
                                                                   UpdateButtons();
                                                                   prompt.OnKeyboardEnterPress?.Invoke(typedText);
                                                               });

                        KeyboardManager.Instance.OnKeyboardClose += () =>
                                                                    {
                                                                        if (!KeyboardManager.TypedText.IsNullOrEmpty())
                                                                            return;

                                                                        currentPrompts.RemoveAt(0);
                                                                        UpdateButtons();
                                                                        prompt.OnKeyboardCancelPress?.Invoke();
                                                                    };
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;
        }

        if (MenuHandler.Instance != null)
        {
            string category = MenuHandler.Instance.Category;

            if (category != "Search")
                SearchState.Query = "";

            switch (category)
            {
                case "Search":
                {
                    string query = SearchState.Query;

                    (Type, hamburburmod)[] results = GetAllMods()
                                                    .Where(x =>
                                                                   x.Item2.AssociatedAttribute.ButtonType !=
                                                                   ButtonType.Category &&
                                                                   (string.IsNullOrEmpty(query) ||
                                                                    x.Item2.ModName.Contains(query,
                                                                            StringComparison.OrdinalIgnoreCase) ||
                                                                    x.Item2.AssociatedAttribute.Description.Contains(
                                                                            query,
                                                                            StringComparison.OrdinalIgnoreCase)))
                                                    .ToArray();

                    for (int i = 0; i < results.Length; i++)
                    {
                        int page = i / ButtonsPerPage;

                        if (page != MenuHandler.Instance.PageIndex)
                            continue;

                        int slot = i % ButtonsPerPage;

                        if (slot >= ModButtons.Length)
                            continue;

                        hamburburmod mod = results[i].Item2;

                        if (mod == null)
                            continue;

                        ModButtons[slot].NormalTMP.text      = mod.ModName;
                        ModButtons[slot].IncrementalTMP.text = mod.ModName;


                        switch (mod.AssociatedAttribute.ButtonType)
                        {
                            case ButtonType.Togglable:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.SetButtonRendererActive(!mod.Enabled);
                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.SetActive(false);

                                break;

                            case ButtonType.Fixed:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.SetButtonRendererActive(true);
                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.SetActive(false);

                                break;

                            case ButtonType.Incremental:
                                ModButtons[slot].PlusButton.OnPress  = () => mod.Toggle(ButtonState.Increment);
                                ModButtons[slot].MinusButton.OnPress = () => mod.Toggle(ButtonState.Decrement);
                                ModButtons[slot].PlusButton.OnHold   = () => mod.Toggle(ButtonState.Increment, false);
                                ModButtons[slot].MinusButton.OnHold  = () => mod.Toggle(ButtonState.Decrement, false);
                                ModButtons[slot].IncrementalButtonObject.SetActive(true);
                                ModButtons[slot].NormalButtonObject.SetActive(false);

                                break;
                        }
                    }

                    break;
                }

                case "Enabled Mods":
                {
                    ValueTuple<Type, hamburburmod>[] enabledMods = Buttons.GetEnabledMods();

                    for (int i = 0; i < enabledMods.Length; i++)
                    {
                        int page = i / ButtonsPerPage;

                        if (page != MenuHandler.Instance.PageIndex)
                            continue;

                        int slot = i % ButtonsPerPage;

                        if (slot >= ModButtons.Length)
                            continue;

                        hamburburmod mod = enabledMods[i].Item2;

                        if (mod == null)
                            continue;

                        ModButtons[slot].NormalTMP.text      = mod.ModName;
                        ModButtons[slot].IncrementalTMP.text = mod.ModName;

                        switch (mod.AssociatedAttribute.ButtonType)
                        {
                            case ButtonType.Togglable:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.SetButtonRendererActive(!mod.Enabled);

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Fixed:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.SetButtonRendererActive(true);

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Incremental:
                                ModButtons[slot].PlusButton.OnPress  = () => mod.Toggle(ButtonState.Increment);
                                ModButtons[slot].MinusButton.OnPress = () => mod.Toggle(ButtonState.Decrement);
                                ModButtons[slot].PlusButton.OnHold   = () => mod.Toggle(ButtonState.Increment, false);
                                ModButtons[slot].MinusButton.OnHold  = () => mod.Toggle(ButtonState.Decrement, false);

                                ModButtons[slot].IncrementalButtonObject.SetActive(true);
                                ModButtons[slot].NormalButtonObject.gameObject.SetActive(false);

                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                }

                default:
                {
                    (Type, hamburburmod)[] visibleMods = Buttons.GetVisibleCategory(category);

                    for (int i = 0; i < visibleMods.Length; i++)
                    {
                        int page = i / ButtonsPerPage;

                        if (page != MenuHandler.Instance.PageIndex)
                            continue;

                        int slot = i % ButtonsPerPage;

                        if (slot >= ModButtons.Length)
                            continue;

                        hamburburmod mod = visibleMods[i].Item2;

                        if (mod == null)
                            continue;

                        ModButtons[slot].NormalTMP.text      = mod.ModName;
                        ModButtons[slot].IncrementalTMP.text = mod.ModName;


                        switch (mod.AssociatedAttribute.ButtonType)
                        {
                            case ButtonType.Togglable:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.SetButtonRendererActive(!mod.Enabled);

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Category:
                            case ButtonType.Fixed:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.SetButtonRendererActive(true);

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Incremental:
                                ModButtons[slot].PlusButton.OnPress  = () => mod.Toggle(ButtonState.Increment);
                                ModButtons[slot].MinusButton.OnPress = () => mod.Toggle(ButtonState.Decrement);
                                ModButtons[slot].PlusButton.OnHold   = () => mod.Toggle(ButtonState.Increment, false);
                                ModButtons[slot].MinusButton.OnHold  = () => mod.Toggle(ButtonState.Decrement, false);
                                ModButtons[slot].IncrementalButtonObject.SetActive(true);
                                ModButtons[slot].NormalButtonObject.gameObject.SetActive(false);

                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                }
            }
        }

        if (animateTransition)
            PlayTransitionAnimation(true);
    }

    public void PlayTransitionAnimation(bool waitForNextFrame = false)
    {
        StopTransitionAnimation();

        if (waitForNextFrame)
        {
            transitionCoroutine = StartCoroutine(BeginTransitionAfterFrame());
            return;
        }

        BeginTransition();
    }

    private IEnumerator BeginTransitionAfterFrame()
    {
        yield return null;

        transitionCoroutine = null;
        BeginTransition();
    }

    private void BeginTransition()
    {

        if (!AnimateButtons.IsEnabled                      ||
            ButtonTransitionAnimation.CurrentIndex == 4    ||
            MenuHandler.Instance?.MenuOpen         != true ||
            ModButtons                             == null ||
            !gameObject.activeInHierarchy)
            return;

        visibleTransitionButtons.Clear();

        foreach (ModButton modButton in ModButtons)
        {
            if (modButton.NormalButtonObject != null && modButton.NormalButtonObject.activeSelf)
                visibleTransitionButtons.Add(modButton.NormalButtonObject);
            else if (modButton.IncrementalButtonObject != null && modButton.IncrementalButtonObject.activeSelf)
                visibleTransitionButtons.Add(modButton.IncrementalButtonObject);
        }

        if (visibleTransitionButtons.Count == 0)
            return;

        transitionCoroutine = StartCoroutine(AnimateTransition(
                visibleTransitionButtons,
                ButtonTransitionAnimation.CurrentIndex));
    }

    private IEnumerator AnimateTransition(IReadOnlyList<GameObject> buttons, int animationIndex)
    {
        bool slide  = animationIndex is 1 or 3;
        bool shrink = animationIndex is 2 or 3;

        foreach (GameObject button in buttons)
        {
            if (button == null)
                continue;

            Transform transformToAnimate = button.transform;
            ButtonTransformState state = new(
                    transformToAnimate.localPosition,
                    transformToAnimate.localScale);

            animatedButtonStates[transformToAnimate] = state;

            if (slide)
                transformToAnimate.localPosition = state.LocalPosition + Vector3.up * TransitionSlideDistance;

            if (shrink)
                transformToAnimate.localScale = Vector3.zero;

            button.SetActive(false);
        }

        float duration = ButtonTransitionSpeed.Duration;
        float delay    = ButtonTransitionSpeed.StaggerDelay;
        float totalDuration = delay * Mathf.Max(0, buttons.Count - 1) +
                              (animationIndex == 0 ? 0.01f : duration);
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < buttons.Count; i++)
            {
                GameObject button = buttons[i];

                if (button == null)
                    continue;

                float buttonElapsed = elapsed - delay * i;

                if (buttonElapsed < 0f)
                    continue;

                button.SetActive(true);

                Transform transformToAnimate = button.transform;

                if (!animatedButtonStates.TryGetValue(transformToAnimate, out ButtonTransformState state))
                    continue;

                if (animationIndex == 0)
                {
                    transformToAnimate.localPosition = state.LocalPosition;
                    transformToAnimate.localScale    = state.LocalScale;
                    continue;
                }

                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(buttonElapsed / duration));

                if (slide)
                    transformToAnimate.localPosition = Vector3.Lerp(
                            state.LocalPosition + Vector3.up * TransitionSlideDistance,
                            state.LocalPosition,
                            progress);

                if (shrink)
                    transformToAnimate.localScale = Vector3.Lerp(Vector3.zero, state.LocalScale, progress);
            }

            yield return null;
        }

        RestoreAnimatedButtons();
        visibleTransitionButtons.Clear();
        transitionCoroutine = null;
    }

    private void StopTransitionAnimation()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        RestoreAnimatedButtons();
        visibleTransitionButtons.Clear();
    }

    private void RestoreAnimatedButtons()
    {
        foreach ((Transform transformToRestore, ButtonTransformState state) in animatedButtonStates)
        {
            if (transformToRestore == null)
                continue;

            transformToRestore.localPosition = state.LocalPosition;
            transformToRestore.localScale    = state.LocalScale;
            transformToRestore.gameObject.SetActive(true);
        }

        animatedButtonStates.Clear();
    }

    public void RefreshButtonText(hamburburmod mod)
    {
        int firstIndex = MenuHandler.Instance.PageIndex * ButtonsPerPage;

        for (int slot = 0; slot < ButtonsPerPage; slot++)
        {
            int index = firstIndex + slot;

            (Type, hamburburmod)[] visibleMain = Buttons.GetVisibleCategory(nameof(Main));

            if (index >= visibleMain.Length)
                return;

            if (visibleMain[index].Item2 != mod)
                continue;

            ModButtons[slot].NormalTMP.text      = mod.ModName;
            ModButtons[slot].IncrementalTMP.text = mod.ModName;

            return;
        }
    }

    private ModButton GetButton(int index)
    {
        Transform button = transform.Find($"Button{index}");

        Transform normalButton      = button.Find("NormalButton");
        Transform incrementalButton = button.Find("IncrementalButton");

        return new ModButton
        {
                PlusButton              = incrementalButton.Find("PlusButton").AddComponent<ButtonCollider>(),
                MinusButton             = incrementalButton.Find("MinusButton").AddComponent<ButtonCollider>(),
                IncrementalButtonObject = incrementalButton.gameObject,
                IncrementalTMP          = incrementalButton.transform.Find("TMP").GetComponent<TMP_Text>(),

                NormalButton       = normalButton.AddComponent<ButtonCollider>(),
                NormalTMP          = normalButton.transform.Find("TMP").GetComponent<TMP_Text>(),
                NormalButtonObject = normalButton.gameObject,
        };
    }

    public static (Type, hamburburmod)[] GetAllMods() =>
            Buttons.Categories
                   .SelectMany(x => x.Value)
                   .Where(x => x.Item2 != null && PluginManager.IsModVisible(x.Item2))
                   .ToArray();

    public static class SearchState
    {
        public static string Query = "";
    }

    public struct ModButton
    {
        public ButtonCollider NormalButton;
        public ButtonCollider PlusButton;
        public ButtonCollider MinusButton;

        public GameObject NormalButtonObject;
        public GameObject IncrementalButtonObject;

        public TMP_Text NormalTMP;
        public TMP_Text IncrementalTMP;
    }

    private readonly struct ButtonTransformState(Vector3 localPosition, Vector3 localScale)
    {
        public readonly Vector3 LocalPosition = localPosition;
        public readonly Vector3 LocalScale    = localScale;
    }
}