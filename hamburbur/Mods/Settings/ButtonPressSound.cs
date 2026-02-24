using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Button Press Sound: ", "Change the sound that plays when buttons get pressed.", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ButtonPressSound : hamburburmod
{
    public static string[] Sounds =
    [
            "Default", //Bark
            "KeyboardClick",
            "Pop",
            "Discord",
            "SmoothClick",
            "HardClick",
            "UiEnter",
            "Wii",
            "Minecraft",
            "Untitled",
            "Destiny",
            "Watch",
            "Creamy",
            "Glass",
            "Krisp Wood",
            "Rustic Click",
            "Drip",
            "Jman Okay",
            "Jman Ahhhh",
    ];

    public static   ButtonPressSound Instance { get; private set; }
    public override string           ModName  => AssociatedAttribute.Name + Sounds[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue >= Sounds.Length)
            IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = Sounds.Length - 1;
    }
}