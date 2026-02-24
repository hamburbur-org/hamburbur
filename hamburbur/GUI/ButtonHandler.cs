using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hamburbur.Components;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
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
    public static Dictionary<string, ModSaveInfo> SavedModInfo = new();

    public static int ButtonsPerPage = 1; // DO NOT MODIFY IT GETS AUTO SET AT RUNTIME

    public static readonly Dictionary<AccessSetting, List<(string, Type)>> InaccessibleButtons = new();

    private readonly List<PromptData> currentPrompts = [];

    public ModButton[] ModButtons;

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

    public void SetCategory(string category, bool cacheLastCategory = true)
    {
        if (cacheLastCategory)
            MenuHandler.LastCategories.Add((category, MenuHandler.Instance.PageIndex));

        MenuHandler.Instance.Category  = category;
        MenuHandler.Instance.PageIndex = 0;

        if (category == "Main")
            MenuHandler.LastCategories.Clear();

        UpdateButtons();
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
        List<ValueTuple<Type, hamburburmod>> mods    = Buttons.Categories[category].ToList();
        object                               modComp = Activator.CreateInstance(mod);

        if (modComp is not hamburburmod hamburburmodComp)
            return null;

        hamburburmodComp.LoadSavedDataWhenStartCalled = true;
        hamburburmodComp.AssociatedAttribute          = mod.GetCustomAttribute<hamburburmodAttribute>();
        hamburburmodComp.InvokeStart();
        mods.Add((mod, hamburburmodComp));
        Buttons.Categories[category] = mods.ToArray();

        if (category == "Main")
        {
            Transform categoryContent =
                    GUIHandler.Instance.Menu.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0);

            GameObject categoryButton = Instantiate(GUIHandler.Instance.CategoryButtonPrefab, categoryContent);
            categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = hamburburmodComp.ModName;
            categoryButton.GetComponent<Button>().onClick
                          .AddListener(() => hamburburmodComp.Toggle(ButtonState.Normal));

            hamburburmodComp.AssociatedGUIButton = categoryButton;
        }
        else
        {
            Transform  modContent = GUIHandler.Instance.Menu.transform.GetChild(2).GetChild(0).GetChild(0);
            GameObject modButton  = Instantiate(GUIHandler.Instance.ModButtonPrefab, modContent);
            modButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = hamburburmodComp.ModName;
            modButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                    hamburburmodComp.AssociatedAttribute.Description;

            modButton.transform.GetChild(2).GetChild(0).gameObject
                     .SetActive(hamburburmodComp.AssociatedAttribute.ButtonType != ButtonType.Incremental);

            modButton.transform.GetChild(2).GetChild(1).gameObject
                     .SetActive(hamburburmodComp.AssociatedAttribute.ButtonType == ButtonType.Incremental);

            modButton.transform.GetChild(2).GetChild(0).GetComponent<Button>().onClick
                     .AddListener(() => hamburburmodComp.Toggle(ButtonState.Normal));

            modButton.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Button>().onClick
                     .AddListener(() => hamburburmodComp.Toggle(ButtonState.Decrement));

            modButton.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<Button>().onClick
                     .AddListener(() => hamburburmodComp.Toggle(ButtonState.Increment));

            hamburburmodComp.AssociatedGUIButton = modButton;
        }

        Instance.UpdateButtons();

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

        if (modComp.Enabled && modComp.AssociatedAttribute.ButtonType == ButtonType.Togglable)
            modComp.Toggle(ButtonState.Normal, false, false);

        buttons.RemoveAll(button => button.Item2 == modComp);
        modComp.AssociatedGUIButton?.Obliterate();

        Buttons.Categories[categoryEntry.Key] = buttons.ToArray();
    }

    public void UpdateButtons()
    {
        GUIHandler.Instance?.UpdateButtons();

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
                    ModButtons[0].NormalButtonObject.GetComponent<Renderer>().enabled = false;
                    ModButtons[0].NormalButton.OnPress                                = null;
                    ModButtons[0].NormalTMP.text                                      = prompt.Title;

                    ModButtons[1].NormalButtonObject.SetActive(true);
                    ModButtons[1].NormalButtonObject.GetComponent<Renderer>().enabled = true;
                    ModButtons[1].NormalTMP.text                                      = prompt.TopButtonText;
                    ModButtons[1].NormalButton.OnPress = () =>
                                                         {
                                                             prompt.OnTopButtonPress?.Invoke();
                                                             currentPrompts.RemoveAt(0);
                                                             UpdateButtons();
                                                         };

                    ModButtons[2].NormalButtonObject.SetActive(true);
                    ModButtons[2].NormalButtonObject.GetComponent<Renderer>().enabled = true;
                    ModButtons[2].NormalTMP.text                                      = prompt.BottomButtonText;
                    ModButtons[2].NormalButton.OnPress = () =>
                                                         {
                                                             prompt.OnBottomButtonPress?.Invoke();
                                                             currentPrompts.RemoveAt(0);
                                                             UpdateButtons();
                                                         };

                    break;

                case PromptType.Continue:
                    ModButtons[0].NormalButtonObject.SetActive(true);
                    ModButtons[0].NormalButtonObject.GetComponent<Renderer>().enabled = false;
                    ModButtons[0].NormalButton.OnPress                                = null;
                    ModButtons[0].NormalTMP.text                                      = prompt.Title;

                    ModButtons[1].NormalButtonObject.SetActive(true);
                    ModButtons[1].NormalButtonObject.GetComponent<Renderer>().enabled = true;
                    ModButtons[1].NormalTMP.text                                      = prompt.TopButtonText;
                    ModButtons[1].NormalButton.OnPress = () =>
                                                         {
                                                             prompt.OnTopButtonPress?.Invoke();
                                                             currentPrompts.RemoveAt(0);
                                                             UpdateButtons();
                                                         };

                    break;

                case PromptType.Keyboard:
                    ModButtons[0].NormalButtonObject.SetActive(true);
                    ModButtons[0].NormalButtonObject.GetComponent<Renderer>().enabled = false;
                    ModButtons[0].NormalButton.OnPress                                = null;
                    ModButtons[0].NormalTMP.text                                      = prompt.Title;

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
                                ModButtons[slot].NormalButtonObject.GetComponent<Renderer>().enabled = !mod.Enabled;
                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.SetActive(false);

                                break;

                            case ButtonType.Fixed:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.GetComponent<Renderer>().enabled = true;
                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.SetActive(false);

                                break;

                            case ButtonType.Incremental:
                                ModButtons[slot].PlusButton.OnPress  = () => mod.Toggle(ButtonState.Increment);
                                ModButtons[slot].MinusButton.OnPress = () => mod.Toggle(ButtonState.Decrement);
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
                                ModButtons[slot].NormalButtonObject.GetComponent<Renderer>().enabled = !mod.Enabled;

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Fixed:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.GetComponent<Renderer>().enabled = true;

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Incremental:
                                ModButtons[slot].PlusButton.OnPress  = () => mod.Toggle(ButtonState.Increment);
                                ModButtons[slot].MinusButton.OnPress = () => mod.Toggle(ButtonState.Decrement);

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
                    for (int i = 0; i < Buttons.Categories[category].Length; i++)
                    {
                        int page = i / ButtonsPerPage;

                        if (page != MenuHandler.Instance.PageIndex)
                            continue;

                        int slot = i % ButtonsPerPage;

                        if (slot >= ModButtons.Length)
                            continue;

                        hamburburmod mod = Buttons.Categories[category][i].Item2;

                        if (mod == null)
                            continue;

                        ModButtons[slot].NormalTMP.text      = mod.ModName;
                        ModButtons[slot].IncrementalTMP.text = mod.ModName;

                        switch (mod.AssociatedAttribute.ButtonType)
                        {
                            case ButtonType.Togglable:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.GetComponent<Renderer>().enabled = !mod.Enabled;

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Category:
                            case ButtonType.Fixed:
                                ModButtons[slot].NormalButton.OnPress = () => mod.Toggle(ButtonState.Normal);
                                ModButtons[slot].NormalButtonObject.GetComponent<Renderer>().enabled = true;

                                ModButtons[slot].NormalButtonObject.SetActive(true);
                                ModButtons[slot].IncrementalButtonObject.gameObject.SetActive(false);

                                break;

                            case ButtonType.Incremental:
                                ModButtons[slot].PlusButton.OnPress  = () => mod.Toggle(ButtonState.Increment);
                                ModButtons[slot].MinusButton.OnPress = () => mod.Toggle(ButtonState.Decrement);
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
                IncrementalTMP          = incrementalButton.transform.Find("TMP").GetComponent<TextMeshPro>(),

                NormalButton       = normalButton.AddComponent<ButtonCollider>(),
                NormalTMP          = normalButton.GetComponentInChildren<TextMeshPro>(),
                NormalButtonObject = normalButton.gameObject,
        };
    }

    public static (Type, hamburburmod)[] GetAllMods()
    {
        return Buttons.Categories
                      .SelectMany(x => x.Value)
                      .Where(x => x.Item2 != null)
                      .ToArray();
    }

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

        public TextMeshPro NormalTMP;
        public TextMeshPro IncrementalTMP;
    }
}