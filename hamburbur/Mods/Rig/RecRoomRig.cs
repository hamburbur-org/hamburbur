using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Rec Room Rig", "The Rec Room Rig", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RecRoomRig : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class RecRoomRigPatch
{
    private static void Postfix(VRRig __instance)
    {
        if (!RecRoomRig.IsEnabled || !__instance.isLocal)
            return;

        Transform cameraTransform = GorillaTagger.Instance.mainCamera.transform;
        Transform leftHand        = GTPlayer.Instance.leftHand.controllerTransform;
        Transform rightHand       = GTPlayer.Instance.rightHand.controllerTransform;

        Vector3 headForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 handCenter  = (leftHand.position + rightHand.position) * 0.5f;
        Vector3 handDirection =
                new Vector3(handCenter.x - cameraTransform.position.x, 0, handCenter.z - cameraTransform.position.z)
                       .normalized;

        bool leftHandBehind  = Vector3.Dot(leftHand.position  - cameraTransform.position, headForward) < 0f;
        bool rightHandBehind = Vector3.Dot(rightHand.position - cameraTransform.position, headForward) < -0f;

        float reductionFactor = 0.4f;
        if (leftHandBehind || rightHandBehind)
            reductionFactor = 0.2f;

        Vector3 torsoDirection = Vector3.Lerp(headForward, handDirection, reductionFactor);

        __instance.transform.rotation =
                Quaternion.Euler(0f, Quaternion.LookRotation(torsoDirection, Vector3.up).eulerAngles.y, 0f);

        __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
        __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
        __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
    }
}