using System;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Custom Menu Name", "Set a custom title. Type RESET to use the normal title", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class CustomMenuName : hamburburmod
{
    private const string PlayerPrefsKey = "hamburbur.customMenuName";
    private const int    MaxLength      = 32;

    public static string CurrentName
    {
        get
        {
            string value = PlayerPrefs.GetString(PlayerPrefsKey, "").Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }
    }

    public override string ModName => string.IsNullOrEmpty(CurrentName)
                                              ? AssociatedAttribute.Name
                                              : $"{AssociatedAttribute.Name}: {CurrentName}";

    protected override void Pressed()
    {
        ButtonHandler.Instance.Prompt(new PromptData(
                PromptType.Keyboard,
                "Enter a custom menu name, or type RESET",
                SaveName,
                null));
    }

    private static void SaveName(string input)
    {
        string name = input.WithoutRichText().Trim();

        if (name.Equals("RESET", StringComparison.OrdinalIgnoreCase))
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
        }
        else
        {
            if (name.Length > MaxLength)
                name = name[..MaxLength];

            PlayerPrefs.SetString(PlayerPrefsKey, name);
        }

        PlayerPrefs.Save();
        ButtonHandler.Instance?.UpdateButtons();
        MenuHandler.Instance?.RefreshMenuTitle();
    }
}