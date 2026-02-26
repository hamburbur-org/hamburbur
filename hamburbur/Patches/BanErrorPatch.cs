using System;
using System.Collections.Generic;
using System.Globalization;
using GorillaNetworking;
using hamburbur.Misc;
using HarmonyLib;
using PlayFab;
using Steamworks;
using UnityEngine;

namespace hamburbur.Patches;

public static class BanErrorPatch
{
    private static string FormatTimeLeft(TimeSpan time)
    {
        if (time <= TimeSpan.Zero)
            return "Expired";

        List<string> parts = [];

        int months = time.Days      / 30;
        int weeks  = time.Days % 30 / 7;
        int days   = time.Days      % 30 % 7;

        if (months > 0) parts.Add($"{months} month{(months == 1 ? "" : "s")}");
        if (weeks  > 0) parts.Add($"{weeks} week{(weeks    == 1 ? "" : "s")}");
        if (days   > 0) parts.Add($"{days} day{(days       == 1 ? "" : "s")}");

        return parts.Count == 0 ? $"{time.Hours} hour{(time.Hours == 1 ? "" : "s")}" : string.Join(" ", parts);
    }

    [HarmonyPatch(typeof(PlayFabAuthenticator), nameof(PlayFabAuthenticator.ShowBanMessage))]
    public static class ShowBanMessagePatch
    {
        private static bool Prefix(PlayFabAuthenticator.BanInfo banInfo)
        {
            try
            {
                if (banInfo.BanExpirationTime == null || banInfo.BanMessage == null)
                    return false;

                bool isIndefinite = string.Equals(banInfo.BanExpirationTime, "Indefinite",
                        StringComparison.OrdinalIgnoreCase);

                string   formattedUnban = "Never";
                TimeSpan remaining      = TimeSpan.Zero;

                if (!isIndefinite && DateTime.TryParse(banInfo.BanExpirationTime, null,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out DateTime unbanUtc))
                {
                    DateTime unbanLocal = unbanUtc.ToLocalTime();
                    formattedUnban = unbanLocal.ToString("dd/MM/yyyy HH:mm:ss");
                    remaining      = unbanUtc - DateTime.UtcNow;
                }

                GorillaComputer.instance.GeneralFailureMessage(
                        !isIndefinite
                                ? $"""
                                   Your account [{SteamUser.GetSteamID().m_SteamID}] has been banned.

                                   Ban Reason: {banInfo.BanMessage}

                                   Time Left: {FormatTimeLeft(remaining)}
                                   Unban Date: {formattedUnban}
                                   """
                                : $"""
                                   Your account [{SteamUser.GetSteamID().m_SteamID}] has been INDEFINITELY banned.

                                   Ban Reason: {banInfo.BanMessage}
                                   """);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to show custom ban message: {ex}");

                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayFabAuthenticator), nameof(PlayFabAuthenticator.OnPlayFabError))]
    public static class OnPlayFabErrorPatch
    {
        private static bool Prefix(PlayFabAuthenticator __instance, PlayFabError obj)
        {
            try
            {
                __instance.LogMessage(obj.ErrorMessage);
                Debug.Log("OnPlayFabError(): " + obj.ErrorMessage);

                __instance.loginFailed = true;

                switch (obj.ErrorMessage)
                {
                    case "The account making this request is currently banned":
                    case "The IP making this request is currently banned":
                    {
                        using Dictionary<string, List<string>>.Enumerator enumerator =
                                obj.ErrorDetails.GetEnumerator();

                        if (!enumerator.MoveNext())
                            return false;

                        KeyValuePair<string, List<string>> current    = enumerator.Current;
                        string                             reason     = current.Key;
                        string                             expiration = current.Value[0];

                        bool isIndefinite = string.Equals(expiration, "Indefinite",
                                StringComparison.OrdinalIgnoreCase);

                        string   formattedUnban = "Never";
                        TimeSpan remaining      = TimeSpan.Zero;

                        if (!isIndefinite && DateTime.TryParse(expiration, null,
                                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                    out DateTime unbanUtc))
                        {
                            DateTime unbanLocal = unbanUtc.ToLocalTime();
                            formattedUnban = unbanLocal.ToString("dd/MM/yyyy HH:mm:ss");
                            remaining      = unbanUtc - DateTime.UtcNow;

                            AccountBanLogger.AddOrUpdateCurrentAccount(reason, unbanUtc);
                        }

                        if (obj.ErrorMessage.Contains("account"))
                            GorillaComputer.instance.GeneralFailureMessage(
                                    !isIndefinite
                                            ? $"""
                                               {SteamFriends.GetPersonaName()} [{SteamUser.GetSteamID().m_SteamID}] has been banned for {reason.Split(" ")[0]}

                                               Time Left: {FormatTimeLeft(remaining)}
                                               Unban Date: {formattedUnban}
                                               """
                                            : $"""
                                               {SteamFriends.GetPersonaName()} [{SteamUser.GetSteamID().m_SteamID}] has been INDEFINITELY banned.

                                               Ban Reason: {reason}
                                               """);
                        else
                            GorillaComputer.instance.GeneralFailureMessage(
                                    !isIndefinite
                                            ? $"""
                                               This IP has been banned for {reason.Split(" ")[0]}

                                               Time Left: {FormatTimeLeft(remaining)}
                                               Unban Date: {formattedUnban}
                                               """
                                            : $"""
                                               This IP has been INDEFINITELY banned.

                                               Ban Reason: {reason}
                                               """);

                        break;
                    }

                    default:
                    {
                        if (GorillaComputer.instance != null)
                            GorillaComputer.instance.GeneralFailureMessage(
                                    GorillaComputer.instance.unableToConnect);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle custom PlayFab error: {ex}");
            }

            return false;
        }
    }
}