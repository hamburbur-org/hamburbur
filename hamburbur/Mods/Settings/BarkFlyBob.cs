using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Bark Fly Bob", "Makes it so you bob up and down when using bark fly", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class BarkFlyBob : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}