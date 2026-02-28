using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Array List", "Shows the enabled mods in the top left of your screen",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class ArrayList : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable()
    {
        IsEnabled                         = false;
        GUIHandler.Instance.arrayListText.text = string.Empty;
    }
}