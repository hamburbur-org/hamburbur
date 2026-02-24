using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Jarvis Voice: ", "Change Jarvis' voice", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JarvisVoice : hamburburmod
{
    public static readonly string[] Voices =
    [
            "Brian",
            "Amy",
            "Russell",
            "Nicole",
            "Matthew",
            "Joanna",
            "Raveena",
    ];

    public override string ModName => AssociatedAttribute.Name + Voices[IncrementalValue];

    public static      JarvisVoice Instance { get; private set; }
    protected override void        Start()  => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue >= Voices.Length)
            IncrementalValue = 0;

        AudioLib.Instance.SpeakText($"Hello, my name is {Voices[IncrementalValue]}, nice to meet you!");
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = Voices.Length - 1;

        AudioLib.Instance.SpeakText($"Hello, my name is {Voices[IncrementalValue]}, nice to meet you!");
    }
}