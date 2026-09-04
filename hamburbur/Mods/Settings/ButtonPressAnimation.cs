using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Button Press Animation: ", "Changes how buttons animate when pressed.", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled,       0)]
public class ButtonPressAnimation : hamburburmod
{
    public static readonly string[] Animations =
    [
            "Grow",
            "Shrink",
            "Bounce",
            "Pulse",
    ];

    public static ButtonPressAnimation Instance { get; private set; }

    public static int CurrentIndex => Instance?.IncrementalValue ?? 0;

    public override string ModName => AssociatedAttribute.Name + Animations[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment() => IncrementalValue = (IncrementalValue + 1) % Animations.Length;

    protected override void Decrement() =>
            IncrementalValue = (IncrementalValue - 1 + Animations.Length) % Animations.Length;
}