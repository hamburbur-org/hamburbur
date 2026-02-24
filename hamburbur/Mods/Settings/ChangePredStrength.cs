using System.Globalization;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Pred Strength: ", "Change the pred strength", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 2)]
public class ChangePredStrength : hamburburmod
{
    private const int MinIndex = 0;
    private const int MaxIndex = 9;

    private static readonly float[] Steps =
    [
            0.01f,
            0.02f,
            0.03f,
            0.04f,
            0.05f,
            0.1f,
            0.125f,
            0.15f,
            0.175f,
            0.2f,
    ];

    public static ChangePredStrength Instance { get; private set; }

    public override string ModName =>
            AssociatedAttribute.Name + Steps[IncrementalValue].ToString("F3", CultureInfo.InvariantCulture);

    public static float CurrentValue =>
            Steps[Instance.IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxIndex)
            IncrementalValue = MinIndex;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinIndex)
            IncrementalValue = MaxIndex;
    }
}