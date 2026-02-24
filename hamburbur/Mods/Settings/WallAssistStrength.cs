using System.Globalization;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Wall Assist Strength: ", "Change the strength of the wall assist", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, MinStep)]
public class WallAssistStrength : hamburburmod
{
    private const int MinStep = 3;  // corresponds to -10
    private const int MaxStep = 10; // corresponds to -3

    public static WallAssistStrength Instance { get; private set; }

    public override string ModName =>
            AssociatedAttribute.Name + IncrementalValue.ToString("N0", CultureInfo.InvariantCulture);

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxStep)
            IncrementalValue = MinStep;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinStep)
            IncrementalValue = MaxStep;
    }
}