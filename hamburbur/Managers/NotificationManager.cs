using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Misc;
using hamburbur.Mods.Settings;
using hamburbur.Tools;

namespace hamburbur.Managers;

public class NotificationManager : Singleton<NotificationManager>
{
    private const string GorillaNotificationsGuid = "hansolo1000falcon.gorillanotifications";

    public static string ChosenFontName = "JetBrains_Mono";

    private static bool gorillaNotificationsPresentInChainloader;

    private static Type fontType;
    private static Type stylingOptionsType;
    private static Type notificationControllerType;

    private static MethodInfo sendNotificationMethod;

    private IEnumerator Start()
    {
        while (!MenuHandler.IsInitialised)
            yield return null;

        gorillaNotificationsPresentInChainloader = IsGorillaNotificationsInChainLoader();

        if (gorillaNotificationsPresentInChainloader)
            yield break;

        RemoveNotificationSettingButtons();

        ButtonHandler.Instance.Prompt(new PromptData(PromptType.AcceptAndDeny,
                "It seems you don't have Gorilla Notifications installed, notifications will not work without this mod.",
                () => ButtonHandler.Instance.SetCategory(nameof(Main)),
                () =>
                {
                    FileManager.Instance.DownloadGorillaNotifications();

                    ButtonHandler.Instance.Prompt(new PromptData(PromptType.AcceptAndDeny,
                            "You must restart your game to apply the changes",
                            () => ButtonHandler.Instance.SetCategory(nameof(Main)),
                            RestartGame.Restart,
                            "Ok",
                            "Restart it for me"));
                },
                "Ok",
                "Download it for me"));
    }

    private static bool IsGorillaNotificationsInChainLoader() =>
            Chainloader.PluginInfos.ContainsKey(GorillaNotificationsGuid);

    private static void RemoveNotificationSettingButtons()
    {
        const string Category = "Notification Settings";

        if (!Buttons.Categories.TryGetValue(Category, out (Type, hamburburmod)[] notificationSettings))
            return;

        Buttons.Categories[Category] = notificationSettings
                                      .Where(mod => mod.Item1 != typeof(NotificationFont) &&
                                                    mod.Item1 != typeof(BlackBackgroundNotifs))
                                      .ToArray();
    }

    private static bool TryCacheGorillaNotificationsTypes()
    {
        if (!IsGorillaNotificationsInChainLoader())
            return false;

        if (fontType                   != null &&
            stylingOptionsType         != null &&
            notificationControllerType != null &&
            sendNotificationMethod     != null)
            return true;

        if (!Chainloader.PluginInfos.TryGetValue(GorillaNotificationsGuid, out PluginInfo pluginInfo))
            return false;

        if (pluginInfo?.Instance == null)
            return false;

        Assembly gorillaNotificationsAssembly = pluginInfo.Instance.GetType().Assembly;

        fontType           = gorillaNotificationsAssembly.GetType("GorillaNotifications.Core.FontType");
        stylingOptionsType = gorillaNotificationsAssembly.GetType("GorillaNotifications.Core.StylingOptions");
        notificationControllerType =
                gorillaNotificationsAssembly.GetType("GorillaNotifications.Core.NotificationController");

        if (fontType == null || stylingOptionsType == null || notificationControllerType == null)
            return false;

        Type stylingOptionsArrayType = stylingOptionsType.MakeArrayType();

        sendNotificationMethod = notificationControllerType
                                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(method =>
                                                {
                                                    if (method.Name != "SendNotification")
                                                        return false;

                                                    ParameterInfo[] parameters = method.GetParameters();

                                                    return parameters.Length           == 5              &&
                                                           parameters[0].ParameterType == typeof(string) &&
                                                           parameters[1].ParameterType == typeof(string) &&
                                                           parameters[2].ParameterType == typeof(float)  &&
                                                           parameters[3].ParameterType == fontType       &&
                                                           parameters[4].ParameterType == stylingOptionsArrayType;
                                                });

        return sendNotificationMethod != null;
    }

    public static string[] GetAvailableFontNames()
    {
        if (!IsGorillaNotificationsInChainLoader())
            return [];

        return TryCacheGorillaNotificationsTypes()
                       ? Enum.GetNames(fontType)
                       : [];
    }

    private static object GetEnumValue(Type enumType, string enumName)
    {
        if (enumType == null || string.IsNullOrEmpty(enumName))
            return null;

        string[] names = Enum.GetNames(enumType);

        if (!names.Contains(enumName))
            enumName = names.Length > 0 ? names[0] : null;

        return enumName == null ? null : Enum.Parse(enumType, enumName);
    }

    public static object SendNotification(
            string source, string notification, float duration, bool playSfx, bool jarvisSpeak)
    {
        if (DisableNotifications.IsEnabled)
            return null;

        gorillaNotificationsPresentInChainloader = IsGorillaNotificationsInChainLoader();

        if (!gorillaNotificationsPresentInChainloader)
            return null;

        if (!TryCacheGorillaNotificationsTypes())
            return null;

        notification = notification.NormaliseString();
        source       = source.NormaliseString();

        if (jarvisSpeak && JarvisDictate.IsEnabled)
            AudioLib.Instance.SpeakText(notification.WithoutRichText());

        if (playSfx)
            Plugin.Instance.PlaySound(DynamicNotificationSounds.IsEnabled
                                              ? MenuSoundsHandler.Instance.DynamicNotificationSound
                                              : MenuSoundsHandler.Instance.NotificationSound);

        object chosenFont = GetEnumValue(fontType, ChosenFontName);

        if (chosenFont == null)
            return null;

        List<object> stylingOptions = new();

        if (FirstPersonVisuals.FirstPersonOnly)
            stylingOptions.Add(GetEnumValue(stylingOptionsType, "OnlyVR"));

        if (BlackBackgroundNotifs.IsEnabled)
            stylingOptions.Add(GetEnumValue(stylingOptionsType, "BlackBox"));

        stylingOptions.RemoveAll(option => option == null);

        Array stylingOptionsArray = Array.CreateInstance(stylingOptionsType, stylingOptions.Count);

        for (int i = 0; i < stylingOptions.Count; i++)
            stylingOptionsArray.SetValue(stylingOptions[i], i);

        return sendNotificationMethod.Invoke(null, [
                                                           source,
                                                           notification,
                                                           duration,
                                                           chosenFont,
                                                           stylingOptionsArray,
                                                   ]);
    }

    public static void UpdateNotificationEntry(object notificationEntry, string source, string notification,
                                               float  duration)
    {
        if (notificationEntry == null)
            return;

        MethodInfo updateMethod = notificationEntry.GetType().GetMethod(
                "UpdateNotification",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [
                        typeof(string),
                        typeof(string),
                        typeof(float),
                ],
                null);

        updateMethod?.Invoke(notificationEntry, [
                                                        source.NormaliseString(),
                                                        notification.NormaliseString(),
                                                        duration,
                                                ]);
    }

    public static void RemoveNotificationEntry(object notificationEntry)
    {
        if (notificationEntry == null)
            return;

        MethodInfo removeMethod = notificationEntry.GetType().GetMethod(
                "RemoveNotification",
                BindingFlags.Public | BindingFlags.Instance);

        removeMethod?.Invoke(notificationEntry, null);
    }
}