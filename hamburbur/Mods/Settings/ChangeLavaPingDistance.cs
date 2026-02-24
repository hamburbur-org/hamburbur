using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Lava Ping Distance: ", "Changes the distance at which lava pings are sent", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 10)]
public class ChangeLavaPingDistance : hamburburmod
{
    private const int MinRange = 1;
    private const int MaxRange = 20;

    public static ChangeLavaPingDistance Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + IncrementalValue + " m";

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