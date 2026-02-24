using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Jarvis Notifications", "Sends a notification of what Jarvis says when its been said.",
        ButtonType.Togglable, AccessSetting.Public,   EnabledType.Enabled, 0)]
public class JarvisNotifications : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}