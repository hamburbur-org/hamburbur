using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Toggle Menu", "Allows you to press secondary to toggle the menu being open",
    ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class ToggleMenu : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}