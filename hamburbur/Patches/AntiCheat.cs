using System.Collections.Generic;
using hamburbur.Managers;
using hamburbur.Tools;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.SendReport))]
public static class AntiCheat
{
    private const           float                     PlayerReportLogCooldown = 1f;
    private static readonly Dictionary<string, float> LastLoggedReport        = [];

    private static bool Prefix(string susReason, string susId, string susNick)
    {
        if (LastLoggedReport.ContainsKey(susId) && LastLoggedReport[susId] > Time.time)
            return susId                                                   != PhotonNetwork.LocalPlayer.UserId;

        NotificationManager.SendNotification(
                "<color=red>Anti Cheat</color>",
                $"MonkeAgent reported {susNick} for: {susReason}",
                8f,
                true,
                true);

        LastLoggedReport[susId] = Time.time + PlayerReportLogCooldown;

        return susId != PhotonNetwork.LocalPlayer.UserId;
    }
}