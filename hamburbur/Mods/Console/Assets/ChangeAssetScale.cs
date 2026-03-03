using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Change Asset Scale: ", "Change the Asset Scale", ButtonType.Incremental, AccessSetting.AdminOnly, EnabledType.Disabled,
        5)]
public class ChangeAssetScale : hamburburmod
{
    private const int MinRange = 1;
    private const int MaxRange = 30;

    public static ChangeAssetScale Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + IncrementalValue;

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