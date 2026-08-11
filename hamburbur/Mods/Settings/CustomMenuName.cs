using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Custom Menu Name", "Uses a saved custom title while enabled", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class CustomMenuName : hamburburmod
{
    private const string PlayerPrefsKey = "hamburbur.customMenuName";
    private const int    MaxLength      = 32;

    public static bool IsEnabled;

    public static string CurrentName => IsEnabled ? GetSavedName() : null;

    public override string ModName => string.IsNullOrEmpty(GetSavedName())
                                              ? AssociatedAttribute.Name
                                              : $"{AssociatedAttribute.Name}: {GetSavedName()}";

    protected override void OnEnable()
    {
        IsEnabled = true;
        MenuHandler.Instance?.RefreshMenuTitle();

        if (!IsUserInitiatedToggle || !string.IsNullOrEmpty(GetSavedName()) || ButtonHandler.Instance == null)
            return;

        ButtonHandler.Instance.Prompt(new PromptData(
                PromptType.AcceptAndDeny,
                "Would you like to set a custom menu name?",
                PromptForName,
                null,
                "Set Name",
                "Not Now"));
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        MenuHandler.Instance?.RefreshMenuTitle();
    }

    private static void PromptForName()
    {
        ButtonHandler.Instance?.Prompt(new PromptData(
                PromptType.Keyboard,
                "Enter a custom menu name",
                SaveName,
                null));
    }

    private static void SaveName(string input)
    {
        string name = input.WithoutRichText().Trim();

        if (string.IsNullOrEmpty(name))
            return;

        if (name.Length > MaxLength)
            name = name[..MaxLength];

        PlayerPrefs.SetString(PlayerPrefsKey, name);

        PlayerPrefs.Save();
        ButtonHandler.Instance?.UpdateButtons();
        MenuHandler.Instance?.RefreshMenuTitle();
    }

    private static string GetSavedName()
    {
        string value = PlayerPrefs.GetString(PlayerPrefsKey, "").WithoutRichText().Trim();

        return string.IsNullOrEmpty(value) ? null : value;
    }
}
