using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Title Animation: ", "Changes how the menu title is animated.", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 1)]
public class MenuTitleAnimation : hamburburmod
{
    public static readonly string[] Animations =
    [
            "None",
            "Typewriter",
            "Fade",
            "Reveal",
            "Pulse",
    ];

    public static MenuTitleAnimation Instance { get; private set; }

    public static int CurrentIndex => NormalizeIndex(Instance?.IncrementalValue ?? 1);

    public static string CurrentAnimation => Animations[CurrentIndex];

    public override string ModName => AssociatedAttribute.Name + CurrentAnimation;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue = (IncrementalValue + 1) % Animations.Length;
        MenuHandler.Instance?.RefreshMenuTitle();
    }

    protected override void Decrement()
    {
        IncrementalValue = (IncrementalValue - 1 + Animations.Length) % Animations.Length;
        MenuHandler.Instance?.RefreshMenuTitle();
    }

    protected override void OnIncrementalStateLoaded() => IncrementalValue = NormalizeIndex(IncrementalValue);

    private static int NormalizeIndex(int index) => index >= 0 && index < Animations.Length ? index : 1;
}
