using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Tracker Notifications", "Gives you a notification and a prompt when someone famous gets player tracked", ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class TrackerNotifications : hamburburmod
{
    public static bool IsEnabled;
    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}