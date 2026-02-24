using System.Globalization;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Pull Strength: ", "Change the pull strength", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, MinStep)]
public class ChangePullStrength : hamburburmod
{
    private const int MinStep = 1;
    private const int MaxStep = 10;

    public static ChangePullStrength Instance { get; private set; }

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