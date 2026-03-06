/*using hamburbur.Tools;
using HarmonyLib;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VRRigCache), nameof(VRRigCache.r))]
public static class RigCachedPatch
{
    private static void Postfix(NetPlayer player, VRRig vrrig)
    {
        if (vrrig.isLocal)
            return;
        
        RigUtils.OnRigUnloaded?.Invoke(vrrig);
        RigUtils.LoadedRigs.Remove(vrrig);
    }
}*/