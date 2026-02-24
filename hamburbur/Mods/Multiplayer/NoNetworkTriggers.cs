using GorillaNetworking;
using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("No Network Triggers", "Disables network triggers", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class NoNetworkTriggers : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(GorillaNetworkJoinTrigger), nameof(GorillaNetworkJoinTrigger.OnBoxTriggered))]
public static class NoNetworkTriggersPatch
{
    private static bool Prefix() => !NoNetworkTriggers.IsEnabled;
}