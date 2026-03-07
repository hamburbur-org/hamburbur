using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRig))]
internal static class OnCosmeticsLoadedPatch
{
    [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics")]
    [HarmonyPostfix]
    private static void OnGetRigCosmetics(VRRig __instance) =>
            RigUtils.OnRigCosmeticsLoaded?.Invoke(__instance);
}