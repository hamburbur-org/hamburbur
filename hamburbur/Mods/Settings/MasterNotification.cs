using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Master Notifications", "Notifies you if you or a hamburbur user becomes a master client.",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class MasterNotification : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}