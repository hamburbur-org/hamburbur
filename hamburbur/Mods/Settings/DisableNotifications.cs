using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Disable Notifications", "Disables notifications fully",
        ButtonType.Togglable, AccessSetting.Public,    EnabledType.Disabled, 0)]
public class DisableNotifications : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}