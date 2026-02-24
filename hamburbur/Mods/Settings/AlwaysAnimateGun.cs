using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Always Animate Gun", "Makes the Gun always do the line animation even when not shooting",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class AlwaysAnimateGun : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}