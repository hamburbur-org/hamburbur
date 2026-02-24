using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.Movement;

[hamburburmod("No Slip", "Stops you from slipping", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class NoSlip : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.GetSlidePercentage))]
public class NoSlipPatch
{
    public static void Postfix(GTPlayer __instance, ref float __result)
    {
        if (NoSlip.IsEnabled)
            __result = 0f;
    }
}