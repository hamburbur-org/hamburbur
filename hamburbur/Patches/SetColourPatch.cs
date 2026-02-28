using hamburbur.Tools;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SetColor))]
public static class SetColourPatch
{
    private static void Postfix(VRRig __instance, Color color)
    {
        if (__instance.isLocal)
            return;
        
        RigUtils.OnRigColourChanged?.Invoke(__instance, color);

        if (RigUtils.LoadedRigs.Contains(__instance))
            return;

        RigUtils.LoadedRigs.Add(__instance);
        RigUtils.OnRigLoaded?.Invoke(__instance);
    }
}