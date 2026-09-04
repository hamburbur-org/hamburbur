using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(               "Use Rig Manager", "Makes you use the rig manager, which forces your rig to a state set by the menu", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Enabled, 0)]
public class UseRigManager : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}