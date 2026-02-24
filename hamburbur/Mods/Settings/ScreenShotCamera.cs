using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Screenshot Camera: ", "Change the camera that you take a screenshot with", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ScreenShotCamera : hamburburmod
{
    private Camera[] allCameras;

    public static ScreenShotCamera Instance { get; private set; }

    public Camera CurrentCamera
    {
        get
        {
            RefreshCameras();

            return allCameras.Length == 0 ? null : allCameras[IncrementalValue];
        }
    }

    public override string ModName => AssociatedAttribute.Name + (CurrentCamera != null ? CurrentCamera.name : "None");

    private void RefreshCameras()
    {
        allCameras = Camera.allCameras;

        if (allCameras.Length == 0)
            return;

        if (IncrementalValue >= allCameras.Length)
            IncrementalValue = allCameras.Length - 1;

        if (IncrementalValue < 0)
            IncrementalValue = 0;
    }

    protected override void Increment()
    {
        RefreshCameras();

        if (allCameras.Length == 0)
            return;

        IncrementalValue++;
        if (IncrementalValue >= allCameras.Length)
            IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        RefreshCameras();

        if (allCameras.Length == 0)
            return;

        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = allCameras.Length - 1;
    }

    protected override void OnIncrementalStateLoaded() => RefreshCameras();
}