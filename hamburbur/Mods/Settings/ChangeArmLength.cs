using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Arm Length: ", "Changes your arm length", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 2)]
public class ChangeArmLength : hamburburmod
{
    private const int MinIndex = 0;
    private const int MaxIndex = 13;

    private static readonly float[] Steps =
    [
            1.05f,
            1.1f,
            1.11f,
            1.12f,
            1.125f,
            1.3f,
            1.4f,
            1.5f,
            1.6f,
            1.7f,
            1.8f,
            1.9f,
            2f,
            10f,
    ];

    private static ChangeArmLength Instance;

    public override string ModName =>
            AssociatedAttribute.Name + Steps[IncrementalValue] + " m";

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