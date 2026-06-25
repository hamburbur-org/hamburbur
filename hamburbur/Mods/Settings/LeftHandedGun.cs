using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Left Handed Gun", "Makes the gun left handed", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LeftHandedGun : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}