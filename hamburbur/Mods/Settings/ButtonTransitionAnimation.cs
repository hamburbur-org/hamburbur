using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                  "Button Transition: ", "Changes how buttons appear when opening the menu or changing pages.",
        ButtonType.Incremental, AccessSetting.Public,  EnabledType.Disabled, 4)]
public class ButtonTransitionAnimation : hamburburmod
{
    public static readonly string[] Animations =
    [
            "Top To Bottom",
            "Slide",
            "Shrink",
            "Slide + Shrink",
            "None",
    ];

    public static ButtonTransitionAnimation Instance { get; private set; }

    public static int CurrentIndex => Instance?.IncrementalValue ?? 0;

    public override string ModName => AssociatedAttribute.Name + Animations[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment() => IncrementalValue = (IncrementalValue + 1) % Animations.Length;

    protected override void Decrement() =>
            IncrementalValue = (IncrementalValue - 1 + Animations.Length) % Animations.Length;
}