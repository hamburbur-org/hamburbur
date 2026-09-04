using System.Collections.Generic;
using GorillaTagScripts;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Patches;

[PatchManager.CriticalPatch]
[HarmonyPatch(typeof(MonkeAgent), nameof(MonkeAgent.SendReport))]
public static class AntiCheat
{
    private const           float                     PlayerReportLogCooldown = 1f;
    private static readonly Dictionary<string, float> LastLoggedReport        = [];

    private static bool Prefix(string susReason, string susId, string susNick)
    {
        if (!AntiCheatNotification.IsEnabled)
            return true;
        
        if (LastLoggedReport.ContainsKey(susId) && LastLoggedReport[susId] > Time.time)
            return susId != PhotonNetwork.LocalPlayer.UserId;

        NotificationManager.SendNotification(
                "<color=red>Anti Cheat</color>",
                $"MonkeAgent reported {susNick} for: {susReason}",
                8f,
                true,
                false);

        LastLoggedReport[susId] = Time.time + PlayerReportLogCooldown;

        return susId != PhotonNetwork.LocalPlayer.UserId;
    }
}