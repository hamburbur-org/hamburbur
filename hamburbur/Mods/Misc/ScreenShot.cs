using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Misc;

[hamburburmod("ScreenShot", "Takes a screenshot, you can change the camera in settings", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ScreenShot : hamburburmod
{
    protected override void Pressed() => CameraCapture.Capture(Camera.main);
}