using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
public class OnRigDisabledPatch
{
    private static void Prefix(VRRig __instance)
    {
        if (__instance.isLocal)
            return;
        
        RigUtils.OnRigUnloaded?.Invoke(__instance);
        RigUtils.LoadedRigs.Remove(__instance);
    }
}