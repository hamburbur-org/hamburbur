using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.ChangeMaterialLocal))]
public static class MatIndexChangePatch
{
    private static void Postfix(VRRig __instance, int materialIndex) => RigUtils.OnMatIndexChange?.Invoke(__instance);
}