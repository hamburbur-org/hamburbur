using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Sticky Platforms", "Makes the platforms sticky", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class StickyPlatforms : hamburburmod
{
    public static bool         IsEnabled;
    public static Action<bool> ToggledPlatformsSticky;

    protected override void OnEnable()
    {
        IsEnabled = true;
        ToggledPlatformsSticky?.Invoke(true);
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        ToggledPlatformsSticky?.Invoke(false);
    }
}