using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Black Background on Notifications", "Makes notifications have a black background", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class BlackBackgroundNotifs : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}