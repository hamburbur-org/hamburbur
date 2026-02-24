using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Fly Speed: ", "Change the fly speed", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
        5)]
public class ChangeFlySpeed : hamburburmod
{
    private const int MinRange = 1;
    private const int MaxRange = 20;

    public static ChangeFlySpeed Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + IncrementalValue + " m/s";

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