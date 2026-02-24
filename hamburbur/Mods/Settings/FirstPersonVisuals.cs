using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "First Person Visuals", "Makes all visuals mod only visible in VR", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled,   0)]
public class FirstPersonVisuals : hamburburmod
{
    public static bool         FirstPersonOnly;
    public static Action<bool> OnFirstPersonOnlyChange;

    protected override void OnEnable()
    {
        FirstPersonOnly = true;
        OnFirstPersonOnlyChange?.Invoke(FirstPersonOnly);
    }

    protected override void OnDisable()
    {
        FirstPersonOnly = false;
        OnFirstPersonOnlyChange?.Invoke(FirstPersonOnly);
    }
}