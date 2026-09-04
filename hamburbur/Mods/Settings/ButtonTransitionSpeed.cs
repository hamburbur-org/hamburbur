using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                  "Button Transition Speed: ", "Changes the speed of menu button transition animations.",
        ButtonType.Incremental, AccessSetting.Public,        EnabledType.Disabled, 1)]
public class ButtonTransitionSpeed : hamburburmod
{
    public static readonly string[] Speeds =
    [
            "Slow",
            "Normal",
            "Fast",
    ];

    public static ButtonTransitionSpeed Instance { get; private set; }

    public static float Duration => (Instance?.IncrementalValue ?? 1) switch
                                    {
                                            0     => 0.18f,
                                            2     => 0.075f,
                                            var _ => 0.12f,
                                    };

    public static float StaggerDelay => (Instance?.IncrementalValue ?? 1) switch
                                        {
                                                0     => 0.075f,
                                                2     => 0.025f,
                                                var _ => 0.045f,
                                        };

    public override string ModName => AssociatedAttribute.Name + Speeds[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment() => IncrementalValue = (IncrementalValue + 1) % Speeds.Length;

    protected override void Decrement() => IncrementalValue = (IncrementalValue - 1 + Speeds.Length) % Speeds.Length;
}