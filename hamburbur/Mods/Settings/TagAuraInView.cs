using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Tag Aura in View",   "Only tags them when in view (for better tag aura only!)", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagAuraInView : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}