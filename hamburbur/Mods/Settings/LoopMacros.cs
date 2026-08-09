using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Loop Macros", "Repeats a macro while its play input is held", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class LoopMacros : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}
