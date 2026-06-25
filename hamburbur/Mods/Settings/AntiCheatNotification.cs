using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "AntiCheat Notification", "If you should be notified when the anti cheat (monke agent) reports someone",
        ButtonType.Togglable, AccessSetting.Public,  EnabledType.Enabled, 0)]
public class AntiCheatNotification : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}