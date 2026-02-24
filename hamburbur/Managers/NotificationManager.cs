using System.Collections.Generic;
using GorillaNotifications.Core;
using hamburbur.Libs;
using hamburbur.Mods.Settings;
using hamburbur.Tools;

namespace hamburbur.Managers;

public static class NotificationManager
{
    public static FontType ChosenFont = FontType.JetBrains_Mono;

    public static NotificationEntry SendNotification(
            string source, string notification, float duration, bool playSfx, bool jarvisSpeak)
    {
        notification = notification.NormaliseString();
        source       = source.NormaliseString();

        if (jarvisSpeak && JarvisSpeak.IsEnabled)
            AudioLib.Instance.SpeakText(notification.WithoutRichText());

        if (playSfx)
            Plugin.Instance.PlaySound(DynamicNotificationSounds.IsEnabled
                                              ? MenuSoundsHandler.Instance.DynamicNotificationSound
                                              : MenuSoundsHandler.Instance.NotificationSound);

        List<StylingOptions> stylingOptions = [];
        if (FirstPersonVisuals.FirstPersonOnly) stylingOptions.Add(StylingOptions.OnlyVR);
        if (BlackBackgroundNotifs.IsEnabled) stylingOptions.Add(StylingOptions.BlackBox);

        return NotificationController.SendNotification(source, notification, duration, ChosenFont,
                stylingOptions.ToArray());
    }
}