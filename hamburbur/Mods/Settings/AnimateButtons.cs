using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Animate Buttons",   "Enables button press and menu transition animations", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Enabled, 0)]
public class AnimateButtons : hamburburmod
{
    public static bool IsEnabled = true;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}