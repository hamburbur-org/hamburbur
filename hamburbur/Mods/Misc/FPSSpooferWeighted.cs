using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Weighted FPS Spoofer", "Spoofs your FPS with a weighted distribution so no bitch can check you",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FPSSpooferWeighted : hamburburmod
{
    public static bool    IsEnabled;
    public static short[] FPSWeights = [87, 88, 88, 89, 89, 89, 90, 90, 90, 91, 91, 92,];

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PackCompetitiveData))]
public static class FPSSpooferWeightedPatch
{
    private static bool Prefix(ref short __result)
    {
        if (!FPSSpooferWeighted.IsEnabled)
            return true;

        __result = FPSSpooferWeighted.FPSWeights[Random.Range(0, FPSSpooferWeighted.FPSWeights.Length)];

        return false;
    }
}