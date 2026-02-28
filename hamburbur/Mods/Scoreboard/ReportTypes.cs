using hamburbur.Mod_Backend;
using hamburbur.Mods.Scoreboard;

namespace hamburbur.Mods.Settings;

[hamburburmod("Change Report Type: ", "Change the report type", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
    0)]
public class ChangeReportType : hamburburmod
{
    private const int MinRange = 0;
    private const int MaxRange = 2;

    public static ChangeReportType Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + Report.ReportTypes[IncrementalValue].name;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxRange) IncrementalValue = MinRange;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinRange) IncrementalValue = MaxRange;
    }
}