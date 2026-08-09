using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Rapid Master Switch Protection", "Disconnects if master client changes five times in three seconds",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class RapidMasterSwitchProtection : hamburburmod
{
    public static bool IsEnabled = true;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}
