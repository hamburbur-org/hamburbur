using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod(                "Unlimit FPS", "Unlimits your FPS on PC", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class UnlimitFPS : hamburburmod
{
    protected override void OnEnable()
    {
        QualitySettings.vSyncCount  = 0;
        Application.targetFrameRate = int.MaxValue;
    }

    protected override void OnDisable() => Application.targetFrameRate = 144;
}