using HarmonyLib;

namespace hamburbur.Patches;

public static class RigPatches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
    public static class RigDisablePatch
    {
        private static bool Prefix(VRRig __instance) =>
                !__instance.isLocal;
    }

    // II Stupid Added this but is apparently causing bans soo...
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public static class RigPostTickPatch
    {
        private static bool Prefix(VRRig __instance) =>
                !__instance.isLocal || __instance.enabled;
    }
}