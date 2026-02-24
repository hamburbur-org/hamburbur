using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRig))]
[HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics", MethodType.Normal)]
public static class OnCosmeticsLoadedPatch
{
    private static void Postfix(VRRig __instance) => RigUtils.OnRigCosmeticsLoaded?.Invoke(__instance);
}