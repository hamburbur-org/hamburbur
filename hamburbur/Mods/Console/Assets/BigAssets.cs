using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Big Assets", "Makes most of the assets bigger", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class BigAssets : hamburburmod
{
    public static bool isEnabled;

    protected override void OnEnable()  => isEnabled = true;
    protected override void OnDisable() => isEnabled = false;
}