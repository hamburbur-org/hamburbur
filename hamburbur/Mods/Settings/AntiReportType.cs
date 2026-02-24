using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Anti Report Type: ", "Change what happens when someone attempts to report you", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class AntiReportType : hamburburmod
{
    public enum AntiReportTypes
    {
        Disconnect,
        Reconnect,
        JoinRandom,
        Notify,
    }

    private static readonly AntiReportTypes[] Types =
            (AntiReportTypes[])Enum.GetValues(typeof(AntiReportTypes));

    public static AntiReportType Instance { get; private set; }

    public static AntiReportTypes Current =>
            Types[Instance.IncrementalValue];

    public override string ModName => AssociatedAttribute.Name + Types[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue = (IncrementalValue + 1) % Types.Length;
    }

    protected override void Decrement()
    {
        IncrementalValue = (IncrementalValue - 1 + Types.Length) % Types.Length;
    }
}