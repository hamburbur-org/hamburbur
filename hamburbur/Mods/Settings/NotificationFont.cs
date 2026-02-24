using System;
using GorillaNotifications.Core;
using hamburbur.Managers;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Notification Font: ", "Change the font of the notifications", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled,  0)]
public class NotificationFont : hamburburmod
{
    private static FontType[] cachedFontTypes;

    public override string ModName => AssociatedAttribute.Name + NotificationManager.ChosenFont;

    protected override void Increment()
    {
        EnsureCache();

        IncrementalValue++;
        if (IncrementalValue >= cachedFontTypes.Length)
            IncrementalValue = 0;

        NotificationManager.ChosenFont = cachedFontTypes[IncrementalValue];
    }

    protected override void Decrement()
    {
        EnsureCache();

        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = cachedFontTypes.Length - 1;

        NotificationManager.ChosenFont = cachedFontTypes[IncrementalValue];
    }

    private static void EnsureCache()
    {
        if (cachedFontTypes != null)
            return;

        cachedFontTypes = (FontType[])Enum.GetValues(typeof(FontType));
    }

    protected override void OnIncrementalStateLoaded()
    {
        EnsureCache();
        NotificationManager.ChosenFont = cachedFontTypes[IncrementalValue];
    }
}