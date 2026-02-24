using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRigCache), nameof(VRRigCache.RemoveRigFromGorillaParent))]
public static class RigCachedPatch
{
    private static void Postfix(NetPlayer player, VRRig vrrig)
    {
        RigUtils.OnRigUnloaded?.Invoke(vrrig);
        RigUtils.LoadedRigs.Remove(vrrig);
    }
}