using System;
using hamburbur.Managers;
using hamburbur.Tools;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Patches;

[PatchManager.CriticalPatch]
[HarmonyPatch(typeof(VRRig), nameof(VRRig.SetColor))]
public static class SetColourPatch
{
    private static void Prefix(VRRig __instance, Color color)
    {
        if (__instance == null || __instance.IsLocalRig())
            return;

        RigUtils.OnRigColourChanged?.Invoke(__instance, color);

        if (RigUtils.LoadedRigs.Contains(__instance))
            return;

        RigUtils.LoadedRigs.Add(__instance);

        try
        {
            RigUtils.OnRigLoaded?.Invoke(__instance);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}