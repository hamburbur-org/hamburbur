using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Hold Incremental Buttons", "Hold plus or minus to change values quickly", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Enabled, 0)]
public class HoldIncrementalButtons : hamburburmod
{
    public static bool IsEnabled = true;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}
