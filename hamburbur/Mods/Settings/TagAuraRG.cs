using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Tag Aura RG", "Makes it so you have to hold down right grip to use tag aura.", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagAuraRG : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}