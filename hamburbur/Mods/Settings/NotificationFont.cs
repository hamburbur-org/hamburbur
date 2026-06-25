using hamburbur.Managers;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Notification Font: ", "Change the font of the notifications", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled,  0)]
public class NotificationFont : hamburburmod
{
    private static string[] cachedFontTypes;

    public override string ModName => AssociatedAttribute.Name + NotificationManager.ChosenFontName;

    protected override void Increment()
    {
        EnsureCache();

        IncrementalValue++;
        if (IncrementalValue >= cachedFontTypes.Length)
            IncrementalValue = 0;

        NotificationManager.ChosenFontName = cachedFontTypes[IncrementalValue];
    }

    protected override void Decrement()
    {
        EnsureCache();

        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = cachedFontTypes.Length - 1;

        NotificationManager.ChosenFontName = cachedFontTypes[IncrementalValue];
    }

    private static void EnsureCache()
    {
        if (cachedFontTypes != null)
            return;

        cachedFontTypes = NotificationManager.GetAvailableFontNames();

        if (cachedFontTypes.Length == 0)
            cachedFontTypes = ["JetBrains_Mono",];
    }

    protected override void OnIncrementalStateLoaded()
    {
        EnsureCache();

        if (IncrementalValue < 0 || IncrementalValue >= cachedFontTypes.Length)
            IncrementalValue = 0;

        NotificationManager.ChosenFontName = cachedFontTypes[IncrementalValue];
    }
}