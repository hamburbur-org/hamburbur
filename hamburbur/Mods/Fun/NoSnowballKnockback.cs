using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod("No Snowball Knockback", "Disable snowball knockback", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Enabled, 0)]
public class NoSnowballKnockback : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.ApplyKnockback))]
public static class NoSnowballKnockbackPatch
{
    private static bool Prefix(Vector3 direction, float speed) => !NoSnowballKnockback.IsEnabled;
}