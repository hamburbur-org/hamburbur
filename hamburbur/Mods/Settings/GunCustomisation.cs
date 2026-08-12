using System.Globalization;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

internal static class GunSettingSteps
{
    public static int Wrap(int value, int length) => (value % length + length) % length;
}

[hamburburmod("Gun Origin: ", "Changes where the gun line and raycast begin", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ChangeGunOrigin : hamburburmod
{
    private static readonly string[] Labels = ["Hand", "Head", "Body Bottom",];
    private static ChangeGunOrigin instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Labels.Length - 1);

    public static GunOrigin CurrentValue =>
            (GunOrigin)Mathf.Clamp(instance?.IncrementalValue ?? 0, 0, Labels.Length - 1);

    public override string ModName => AssociatedAttribute.Name + Labels[CurrentIndex];

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Labels.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Labels.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}

[hamburburmod("Gun Origin Offset: ", "Moves the gun start out from its selected origin", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ChangeGunOriginOffset : hamburburmod
{
    private static readonly float[] Steps = [0f, 0.05f, 0.1f, 0.2f, 0.35f, 0.5f,];
    private static ChangeGunOriginOffset instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Steps.Length - 1);

    public static float CurrentValue => Steps[Mathf.Clamp(instance?.IncrementalValue ?? 0, 0, Steps.Length - 1)];

    public override string ModName =>
            AssociatedAttribute.Name + Steps[CurrentIndex].ToString("0.00", CultureInfo.InvariantCulture) + " m";

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Steps.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Steps.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}

[hamburburmod("Gun Direction: ", "Changes which side of the selected hand aims the gun", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ChangeGunDirection : hamburburmod
{
    private static readonly string[] Labels =
    [
            "Forward",
            "Palm",
            "Knuckles",
            "Forward Slight Up",
            "Forward Up",
            "Forward Steep Up",
            "Forward Slight Down",
            "Forward Down",
            "Forward Steep Down",
    ];
    private static ChangeGunDirection instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Labels.Length - 1);

    public static GunDirection CurrentValue =>
            (GunDirection)Mathf.Clamp(instance?.IncrementalValue ?? 0, 0, Labels.Length - 1);

    public override string ModName => AssociatedAttribute.Name + Labels[CurrentIndex];

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Labels.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Labels.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}

[hamburburmod("Gun Line Size: ", "Changes the thickness of the gun line", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 2)]
public class ChangeGunLineThickness : hamburburmod
{
    private static readonly float[] Steps = [0.5f, 0.75f, 1f, 1.5f, 2f, 3f,];
    private static ChangeGunLineThickness instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Steps.Length - 1);

    public static float CurrentValue => Steps[Mathf.Clamp(instance?.IncrementalValue ?? 2, 0, Steps.Length - 1)];

    public override string ModName =>
            AssociatedAttribute.Name + Steps[CurrentIndex].ToString("0.##", CultureInfo.InvariantCulture) + "x";

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Steps.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Steps.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}

[hamburburmod("Gun Colours: ", "Changes the gun line and target marker colours", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ChangeGunColour : hamburburmod
{
    private static readonly string[] Labels =
    [
            "Theme Pulse",
            "Theme Gradient",
            "Rainbow",
            "Fire",
            "Ocean",
            "Neon",
            "White",
            "Red",
            "Green",
            "Blue",
            "Purple",
            "Pink",
    ];

    private static ChangeGunColour instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Labels.Length - 1);

    public static GunColourPreset CurrentValue =>
            (GunColourPreset)Mathf.Clamp(instance?.IncrementalValue ?? 0, 0, Labels.Length - 1);

    public override string ModName => AssociatedAttribute.Name + Labels[CurrentIndex];

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Labels.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Labels.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}

[hamburburmod("Gun Target Marker", "Shows a sphere at the gun's raycast hit or line end", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GunTargetMarker : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[hamburburmod("Marker Size: ", "Changes the size of the gun target sphere", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 2)]
public class ChangeGunMarkerSize : hamburburmod
{
    private static readonly float[] Steps = [0.025f, 0.05f, 0.075f, 0.1f, 0.15f, 0.25f,];
    private static ChangeGunMarkerSize instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Steps.Length - 1);

    public static float CurrentValue => Steps[Mathf.Clamp(instance?.IncrementalValue ?? 2, 0, Steps.Length - 1)];

    public override string ModName =>
            AssociatedAttribute.Name + (Steps[CurrentIndex] * 100f).ToString("0.#", CultureInfo.InvariantCulture) + " cm";

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Steps.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Steps.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}

[hamburburmod("Gun Vibrations", "Vibrates the aiming hand while the gun trigger is held", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GunVibrations : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[hamburburmod("Vibration Strength: ", "Changes the strength of gun haptics", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 2)]
public class ChangeGunVibrationStrength : hamburburmod
{
    private static readonly float[] Steps = [0.1f, 0.25f, 0.5f, 0.75f, 1f,];
    private static ChangeGunVibrationStrength instance;

    private int CurrentIndex => Mathf.Clamp(IncrementalValue, 0, Steps.Length - 1);

    public static float CurrentValue => Steps[Mathf.Clamp(instance?.IncrementalValue ?? 2, 0, Steps.Length - 1)];

    public override string ModName =>
            AssociatedAttribute.Name + (Steps[CurrentIndex] * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";

    protected override void Start() => instance = this;
    protected override void Increment() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue + 1, Steps.Length);
    protected override void Decrement() => IncrementalValue = GunSettingSteps.Wrap(IncrementalValue - 1, Steps.Length);
    protected override void OnIncrementalStateLoaded() => IncrementalValue = CurrentIndex;
}
