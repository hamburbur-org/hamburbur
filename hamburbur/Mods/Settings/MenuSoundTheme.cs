using System;
using hamburbur.Managers;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Menu Sounds: ", "Changes the optional sounds played when opening and closing the menu.",
        ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled, 1)]
public class MenuSoundTheme : hamburburmod
{
    private static readonly string[] FallbackSounds =
    [
            "Silent",
            "Default",
            "ZlothY",
            "Untitled",
            "Seralyth",
            "UI",
    ];

    public static MenuSoundTheme Instance { get; private set; }

    public static int CurrentIndex => Instance?.IncrementalValue ?? 0;

    private static int SoundCount => Math.Max(MenuSoundsHandler.Instance?.MenuSoundSetCount ?? 0, FallbackSounds.Length);

    public override string ModName => AssociatedAttribute.Name + GetSoundName(IncrementalValue);

    protected override void Start() => Instance = this;

    protected override void Increment() => IncrementalValue = (IncrementalValue + 1) % SoundCount;

    protected override void Decrement() => IncrementalValue = (IncrementalValue - 1 + SoundCount) % SoundCount;

    protected override void OnIncrementalStateLoaded() =>
            IncrementalValue = (IncrementalValue % SoundCount + SoundCount) % SoundCount;

    private static string GetSoundName(int index)
    {
        if (MenuSoundsHandler.Instance?.MenuSoundSetCount > 0)
            return MenuSoundsHandler.Instance.GetMenuSoundSetName(index);

        int wrappedIndex = (index % FallbackSounds.Length + FallbackSounds.Length) % FallbackSounds.Length;

        return FallbackSounds[wrappedIndex];
    }
}
