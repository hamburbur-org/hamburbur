using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Spider Walk", "You can stick to walls", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class SpiderWalk : hamburburmod
{
    internal static bool    Active;
    internal static Vector3 SurfaceNormal;

    private Vector3 walkPos;

    private bool wasWalking;

    protected override void FixedUpdate()
    {
        bool isWalking = false;

        if (GTPlayer.Instance.IsHandTouching(true) || GTPlayer.Instance.IsHandTouching(false))
        {
            RaycastHit ray = GTPlayer.Instance.lastHitInfoHand;
            walkPos       = ray.point;
            SurfaceNormal = ray.normal;
            Active        = true;
        }

        if (walkPos != Vector3.zero)
        {
            isWalking = true;
            GorillaTagger.Instance.rigidbody.AddForce(SurfaceNormal * Physics.gravity.y, ForceMode.Acceleration);

            Transform parent = GTPlayer.Instance.turnParent.transform;
            parent.rotation = Quaternion.Lerp(
                    parent.rotation,
                    Quaternion.LookRotation(SurfaceNormal) * Quaternion.Euler(90f, 0f, 0f),
                    Time.deltaTime
            );
        }

        if (isWalking != wasWalking)
        {
            if (wasWalking)
                RigUtils.DisableZeroGravity();
            else
                RigUtils.EnableZeroGravity();
        }

        wasWalking = isWalking;
    }

    protected override void OnDisable()
    {
        Active  = false;
        walkPos = Vector3.zero;
        RigUtils.DisableZeroGravity();
        RigUtils.FixRigRotations();
    }
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class SpiderWalkRigPatch
{
    private static void Postfix(VRRig __instance)
    {
        if (!SpiderWalk.Active || !__instance.isLocal)
            return;

        Quaternion target =
                Quaternion.FromToRotation(Vector3.up, SpiderWalk.SurfaceNormal) *
                __instance.transform.rotation;

        __instance.transform.rotation =
                Quaternion.Euler(GTPlayer.Instance.headCollider.transform.rotation.eulerAngles.x,
                        GTPlayer.Instance.headCollider.transform.rotation.eulerAngles.y, target.z);

        __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
        __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
        __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
        __instance.head.rigTarget.rotation = GTPlayer.Instance.headCollider.transform.rotation;
    }
}

[HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.LateUpdate))]
public static class SpiderWalkGTPlayerPatch
{
    private static void Postfix(GTPlayer __instance)
    {
        if (!SpiderWalk.Active)
            return;

        Quaternion target =
                Quaternion.FromToRotation(Vector3.up, SpiderWalk.SurfaceNormal);

        __instance.bodyCollider.transform.rotation      = target;
        __instance.bodyCollider.transform.localPosition = new Vector3(0f, -0.3f, 0f);
    }
}