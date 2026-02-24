using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Dynamic Notification Sounds" /* "Dynamic Notificatio nSounds" T-T */,
        "Replaces the notification sound with an alternative one",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DynamicNotificationSounds : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}