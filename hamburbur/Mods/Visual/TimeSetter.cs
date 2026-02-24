using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Visual;

[hamburburmod(                "Time: ", "Lets you change the time of day", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class TimeSetter : hamburburmod
{
    public static bool NormalTime = true;

    public override string ModName => AssociatedAttribute.Name +
                                      (BetterDayNightManager.instance.currentSetting == TimeSettings.Normal
                                               ? "Normal"
                                               : IncrementalValue.ToString("N0"));

    protected override void Start()
    {
        Tools.Utils.OnFixedUpdate += () =>
                                     {
                                         if (!NormalTime)
                                             BetterDayNightManager.instance.SetTimeOfDay(IncrementalValue);
                                     };
    }

    protected override void Increment()
    {
        IncrementalValue = (IncrementalValue + 1) % BetterDayNightManager.instance.timeOfDayRange.Length;
        NormalTime       = false;
    }

    protected override void Decrement()
    {
        IncrementalValue = (IncrementalValue - 1 + BetterDayNightManager.instance.timeOfDayRange.Length) %
                           BetterDayNightManager.instance.timeOfDayRange.Length;

        NormalTime = false;
    }

    protected override void OnIncrementalStateLoaded() => NormalTime = false;
}