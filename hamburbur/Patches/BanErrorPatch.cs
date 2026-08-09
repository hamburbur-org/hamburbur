using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GorillaNetworking;
using hamburbur.Managers;
using hamburbur.Misc;
using HarmonyLib;
using PlayFab;
using Steamworks;
using UnityEngine;

namespace hamburbur.Patches;

public static class BanErrorPatch
{
    private const string IndefiniteExpiration = "Indefinite";

    private static string FormatTimeLeft(TimeSpan time)
    {
        if (time <= TimeSpan.Zero)
            return "Expired";

        List<string> parts = [];

        int totalDays = Mathf.Max(0, (int)time.TotalDays);
        int months    = totalDays      / 30;
        int weeks     = totalDays % 30 / 7;
        int days      = totalDays      % 7;

        AddTimePart(parts, months, "month");
        AddTimePart(parts, weeks,  "week");
        AddTimePart(parts, days,   "day");

        if (parts.Count > 0)
            return string.Join(" ", parts);

        int hours = Mathf.Max(0, (int)time.TotalHours);

        if (hours > 0)
            return $"{hours} hour{GetPluralSuffix(hours)}";

        int minutes = Mathf.Max(1, (int)time.TotalMinutes);

        return $"{minutes} minute{GetPluralSuffix(minutes)}";
    }

    private static void AddTimePart(
            ICollection<string> parts,
            int                 value,
            string              name)
    {
        if (value <= 0)
            return;

        parts.Add($"{value} {name}{GetPluralSuffix(value)}");
    }

    private static string GetPluralSuffix(int value) =>
            value == 1 ? string.Empty : "s";

    private static bool ContainsBanText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.IndexOf("ban", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsBanError(PlayFabError error)
    {
        if (error == null)
            return false;

        if (ContainsBanText(error.ErrorMessage))
            return true;

        if (ContainsBanText(error.Error.ToString()))
            return true;

        if (error.ErrorDetails == null)
            return false;

        foreach (KeyValuePair<string, List<string>> detail in error.ErrorDetails)
        {
            if (ContainsBanText(detail.Key))
                return true;

            if (detail.Value == null)
                continue;

            if (detail.Value.Any(ContainsBanText))
                return true;
        }

        return false;
    }

    private static bool TryGetBanDetails(
            PlayFabError error,
            out string   reason,
            out string   expiration)
    {
        reason     = "Unknown";
        expiration = IndefiniteExpiration;

        if (error?.ErrorDetails == null)
            return false;

        foreach (KeyValuePair<string, List<string>> detail in error.ErrorDetails)
        {
            if (!string.IsNullOrWhiteSpace(detail.Key))
                reason = detail.Key;

            if (detail.Value is { Count: > 0, } &&
                !string.IsNullOrWhiteSpace(detail.Value[0]))
                expiration = detail.Value[0];

            return true;
        }

        return false;
    }

    private static bool TryParseExpiration(
            string       expiration,
            out DateTime expirationUtc,
            out TimeSpan remaining)
    {
        expirationUtc = default(DateTime);
        remaining     = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(expiration))
            return false;

        if (string.Equals(
                    expiration,
                    IndefiniteExpiration,
                    StringComparison.OrdinalIgnoreCase))
            return false;

        if (!DateTime.TryParse(
                    expiration,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal |
                    DateTimeStyles.AdjustToUniversal,
                    out expirationUtc))
            return false;

        remaining = expirationUtc - DateTime.UtcNow;

        return true;
    }

    private static string CreateAccountBanMessage(
            string reason,
            string expiration)
    {
        string account = GetAccountDescription();

        if (!TryParseExpiration(
                    expiration,
                    out DateTime expirationUtc,
                    out TimeSpan remaining))
            return $"""
                    {account} has been INDEFINITELY banned.

                    Ban Reason: {reason}
                    """;

        AccountBanLogger.AddOrUpdateCurrentAccount(reason, expirationUtc);

        return $"""
                {account} has been banned.

                Ban Reason: {reason}

                Time Left: {FormatTimeLeft(remaining)}
                Unban Date: {expirationUtc.ToLocalTime():dd/MM/yyyy HH:mm:ss}
                """;
    }

    private static string CreateIpBanMessage(
            string reason,
            string expiration) =>
            !TryParseExpiration(
                    expiration,
                    out DateTime expirationUtc,
                    out TimeSpan remaining) ? $"""
                                               This IP has been INDEFINITELY banned.

                                               Ban Reason: {reason}
                                               """ : $"""
                                                      This IP has been banned.

                                                      Ban Reason: {reason}

                                                      Time Left: {FormatTimeLeft(remaining)}
                                                      Unban Date: {expirationUtc.ToLocalTime():dd/MM/yyyy HH:mm:ss}
                                                      """;

    private static string GetAccountDescription()
    {
        try
        {
            string nickname = SteamFriends.GetPersonaName();
            ulong  steamId  = SteamUser.GetSteamID().m_SteamID;

            if (string.IsNullOrWhiteSpace(nickname))
                nickname = "Your account";

            if (steamId == 0)
                return nickname;

            return $"{nickname} [{steamId}]";
        }
        catch
        {
            return "Your account";
        }
    }

    private static void ShowFailureMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (GorillaComputer.instance == null)
            return;

        GorillaComputer.instance.GeneralFailureMessage(message);
    }

    [HarmonyPatch(
            typeof(PlayFabAuthenticator),
            nameof(PlayFabAuthenticator.ShowBanMessage))]
    private static class ShowBanMessagePatch
    {
        private static bool Prefix(PlayFabAuthenticator.BanInfo banInfo)
        {
            if (banInfo == null)
                return true;

            if (string.IsNullOrWhiteSpace(banInfo.BanMessage))
                return true;

            try
            {
                string expiration = string.IsNullOrWhiteSpace(banInfo.BanExpirationTime)
                                            ? IndefiniteExpiration
                                            : banInfo.BanExpirationTime;

                string message = CreateAccountBanMessage(
                        banInfo.BanMessage,
                        expiration);

                ShowFailureMessage(message);

                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                        $"Failed to show custom ban message: {exception}");

                return true;
            }
        }
    }

    [HarmonyPatch(
            typeof(PlayFabAuthenticator),
            nameof(PlayFabAuthenticator.OnPlayFabError))]
    private static class OnPlayFabErrorPatch
    {
        private static void Postfix(PlayFabError obj)
        {
            if (!IsBanError(obj))
                return;

            try
            {
                TryGetBanDetails(
                        obj,
                        out string reason,
                        out string expiration);

                bool isIpBan = obj.ErrorMessage?.IndexOf(
                                       "IP",
                                       StringComparison.OrdinalIgnoreCase) >= 0;

                string message = isIpBan
                                         ? CreateIpBanMessage(reason, expiration)
                                         : CreateAccountBanMessage(reason, expiration);

                ShowFailureMessage(message);
                BanNotificationHandler.TryNotify(message);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                        $"Failed to handle PlayFab ban error: {exception}");
            }
        }
    }
    
    [HarmonyPatch(
            typeof(GorillaComputer),
            nameof(GorillaComputer.GeneralFailureMessage),
            typeof(string))]
    private static class GeneralFailureMessagePatch
    {
        private static void Postfix(string failMessage)
        {
            if (!ContainsBanText(failMessage))
                return;

            BanNotificationHandler.TryNotify(failMessage);
        }
    }

    private static class BanNotificationHandler
    {
        private const float DuplicateNotificationDelay = 3f;

        private static float  nextNotificationTime;
        private static string lastMessage = string.Empty;

        public static void TryNotify(string message)
        {

            if (!ContainsBanText(message))
                return;

            float currentTime = Time.realtimeSinceStartup;

            bool isDuplicate =
                    string.Equals(
                            lastMessage,
                            message,
                            StringComparison.Ordinal) &&
                    currentTime < nextNotificationTime;

            if (isDuplicate)
                return;

            lastMessage = message;
            nextNotificationTime =
                    currentTime + DuplicateNotificationDelay;

            NotificationManager.SendNotification(
                    "<color=red>Account Ban</color>",
                    "A ban response was received from Gorilla Tag",
                    5f,
                    true,
                    true);
        }
    }
}