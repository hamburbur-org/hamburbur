using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod(                "FPS Spoofer", "Lets you spoof your FPS!", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class FPSSpoofer : hamburburmod
{
    public static      FPSSpoofer Instance { get; private set; }
    protected override void       Start()  => Instance = this;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PackCompetitiveData))]
public static class FPSSpooferPatch
{
    private static bool Prefix(ref short __result)
    {
        if (!FPSSpoofer.Instance.Enabled)
            return true;

        __result = (short)Random.Range(FPSChangerLowest.Instance.IncrementalValue,
                FPSChangerLowest.Instance.IncrementalValue);

        return false;
    }
}