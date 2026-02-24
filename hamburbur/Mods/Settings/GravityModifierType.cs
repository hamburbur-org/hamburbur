using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

public enum GravityModifierTypes
{
    Zero,
    Low,
    High,
    Reverse,
}

[hamburburmod("Gravity Modifier: ", "Change what type of gravity modifier you have", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GravityModifierType : hamburburmod
{
    private static readonly GravityModifierTypes[] Types =
            (GravityModifierTypes[])Enum.GetValues(typeof(GravityModifierTypes));

    public static GravityModifierType Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + Types[IncrementalValue] + " Gravity";

    public static GravityModifierTypes Current =>
            Types[Instance.IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment() => IncrementalValue = (IncrementalValue + 1) % Types.Length;

    protected override void Decrement() => IncrementalValue = (IncrementalValue - 1 + Types.Length) % Types.Length;
}