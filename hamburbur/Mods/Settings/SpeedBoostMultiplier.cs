using System.Globalization;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Speed Boost Multiplier: ", "The speedboost multiplier!", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 120)]
public class SpeedBoostMultiplier : hamburburmod
{
    private const int MinMultiplier = 105;
    private const int MaxMultiplier = 200;

    public static SpeedBoostMultiplier Instance { get; private set; }

    public override string ModName =>
            AssociatedAttribute.Name + (IncrementalValue / 100f).ToString("F", CultureInfo.InvariantCulture);

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue += 5;
        if (IncrementalValue > MaxMultiplier)
            IncrementalValue = MinMultiplier;
    }

    protected override void Decrement()
    {
        IncrementalValue -= 5;
        if (IncrementalValue < MinMultiplier)
            IncrementalValue = MaxMultiplier;
    }
}