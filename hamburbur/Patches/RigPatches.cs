using hamburbur.Managers;
using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[PatchManager.CriticalPatch]
public static class RigPatches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
    public static class RigDisablePatch
    {
        private static bool Prefix(VRRig __instance) =>
                !__instance.IsLocalRig();
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public static class RigPostTickPatch
    {
        private static bool Prefix(VRRig __instance) =>
                !__instance.IsLocalRig() || __instance.enabled;
    }
}