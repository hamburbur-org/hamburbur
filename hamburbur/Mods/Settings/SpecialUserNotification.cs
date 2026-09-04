using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Special User Notification", "If you should receive notifications for special users",
        ButtonType.Togglable, AccessSetting.Public,        EnabledType.Enabled, 0)]
public class SpecialUserNotification : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}