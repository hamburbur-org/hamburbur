using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Server Status Notifications",
        "Sends a notification every time you send and receive status updates to the server", ButtonType.Togglable,
        AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class ServerStatusNotifications : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}