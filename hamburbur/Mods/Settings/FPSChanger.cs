using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Highest FPS: ", "Change the FPS", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
        MaxFPS)]
public class FPSChangerHighest : hamburburmod
{
    private const int MaxFPS = 255;

    public override string ModName => AssociatedAttribute.Name + IncrementalValue;

    public static FPSChangerHighest Instance { get; private set; }

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxFPS)
            IncrementalValue = FPSChangerLowest.Instance.IncrementalValue;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < FPSChangerLowest.Instance.IncrementalValue)
            IncrementalValue = MaxFPS;
    }
}

[hamburburmod("Lowest FPS: ", "Change the FPS", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
        MinFPS)]
public class FPSChangerLowest : hamburburmod
{
    private const int MinFPS = 0;

    public static FPSChangerLowest Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + IncrementalValue;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > FPSChangerHighest.Instance.IncrementalValue)
            IncrementalValue = MinFPS;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinFPS)
            IncrementalValue = FPSChangerHighest.Instance.IncrementalValue;
    }
}