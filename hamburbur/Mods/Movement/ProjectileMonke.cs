using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete

namespace hamburbur.Mods.Movement;

[hamburburmod("Projectile Monke", "You get propelled in the direction you throw projectiles", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ProjectileMonke : hamburburmod
{
    public static bool IsEnabled;

    private static float lastLaunch;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;

    public static void Launch(Vector3 velocity)
    {
        if (!(Time.realtimeSinceStartup > lastLaunch))
            return;

        lastLaunch = Time.realtimeSinceStartup + 0.4f;

        velocity                                                  *= 1f;
        GTPlayer.Instance.currentVelocity                         =  velocity;
        GTPlayer.Instance.bodyCollider.attachedRigidbody.velocity =  velocity;
    }
}

[HarmonyPatch(typeof(Slingshot), nameof(Slingshot.LateUpdateLocal))]
public class GrabPatch
{
    public static void Prefix(Slingshot __instance)
    {
        if (!ProjectileMonke.IsEnabled || __instance.itemState is not (TransferrableObject.ItemStates.State2
                                                                       or TransferrableObject.ItemStates.State3))
            return;

        Rigidbody rb       = GTPlayer.Instance.bodyCollider.attachedRigidbody;
        Vector3   velocity = rb.velocity * 0.995f;

        GTPlayer.Instance.currentVelocity = velocity;
        rb.velocity                       = velocity;
        rb.AddForce(Physics.gravity * -0.4f * rb.mass * GTPlayer.Instance.scale);
    }
}

[HarmonyPatch(typeof(SlingshotProjectile), nameof(SlingshotProjectile.Launch))]
public class LaunchPatch
{
    public static void Prefix(Vector3 velocity, NetPlayer player)
    {
        if (!player.IsLocal)
            return;

        ProjectileMonke.Launch(velocity);
    }
}