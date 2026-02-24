using System;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Rig;

public enum RigMode
{
    Skellon,
    Wings,
    Lemming,
    Head,
    RecRoom,
}

[hamburburmod("PC Rig", "A customizable PC rig.", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class PCRig : hamburburmod
{
    public static RigMode CurrentRigMode = RigMode.Wings;
    public static bool    IsEnabled;

    private Vector3 skellonLeftPos;
    private Vector3 skellonRightPos;

    protected override void Update()
    {
        if (Tools.Utils.InVR)
            return;

        switch (CurrentRigMode)
        {
            case RigMode.Skellon:
            {
                Vector3 originLeft = VRRig.LocalRig.bodyRenderer.transform.position +
                                     VRRig.LocalRig.bodyRenderer.transform.right * -0.25f;

                Vector3 originRight = VRRig.LocalRig.bodyRenderer.transform.position +
                                      VRRig.LocalRig.bodyRenderer.transform.right * 0.25f;

                bool leftRaycast = Physics.Raycast(originLeft, Vector3.down, out RaycastHit leftHitInfo, 0.7f,
                        GTPlayer.LocomotionEnabledLayers);

                bool rightRaycast = Physics.Raycast(originRight, Vector3.down, out RaycastHit rightHitInfo, 0.7f,
                        GTPlayer.LocomotionEnabledLayers);

                Vector3 leftPos  = leftRaycast ? leftHitInfo.point : originLeft    + Vector3.down * 0.7f;
                Vector3 rightPos = rightRaycast ? rightHitInfo.point : originRight + Vector3.down * 0.7f;

                skellonLeftPos =
                        Vector3.Lerp(skellonLeftPos, leftPos, Time.deltaTime * PCRigPatch.SkellonRotationSpeed);

                skellonRightPos = Vector3.Lerp(skellonRightPos, rightPos,
                        Time.deltaTime * PCRigPatch.SkellonRotationSpeed);

                GTPlayer.Instance.leftHand.controllerTransform.transform.position  = skellonLeftPos;
                GTPlayer.Instance.rightHand.controllerTransform.transform.position = skellonRightPos;

                GTPlayer.Instance.leftHand.controllerTransform.transform.rotation =
                        VRRig.LocalRig.bodyRenderer.transform.rotation;

                GTPlayer.Instance.rightHand.controllerTransform.transform.rotation =
                        VRRig.LocalRig.bodyRenderer.transform.rotation;

                break;
            }

            case RigMode.Wings:
            {
                GTPlayer.Instance.leftHand.controllerTransform.transform.position =
                        VRRig.LocalRig.bodyRenderer.transform.position +
                        VRRig.LocalRig.bodyRenderer.transform.right * -0.7f;

                GTPlayer.Instance.leftHand.controllerTransform.transform.rotation =
                        VRRig.LocalRig.bodyRenderer.transform.rotation;

                GTPlayer.Instance.rightHand.controllerTransform.transform.position =
                        VRRig.LocalRig.bodyRenderer.transform.position +
                        VRRig.LocalRig.bodyRenderer.transform.right * 0.7f;

                GTPlayer.Instance.rightHand.controllerTransform.transform.rotation =
                        VRRig.LocalRig.bodyRenderer.transform.rotation;

                break;
            }

            case RigMode.Lemming:
            {
                VRRig     localRig = VRRig.LocalRig;
                Transform head     = localRig.head.rigTarget;

                head.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                GTPlayer.Instance.leftHand.controllerTransform.position =
                        localRig.bodyRenderer.transform.TransformPoint(-0.2f, -0.05f, 0.6f);

                GTPlayer.Instance.rightHand.controllerTransform.position =
                        localRig.bodyRenderer.transform.TransformPoint(0.2f, -0.05f, 0.6f);

                Quaternion headRot = head.rotation;

                Quaternion tilt = Quaternion.Euler(-90f, 0f, 0f);
                GTPlayer.Instance.leftHand.controllerTransform.rotation  = headRot * tilt;
                GTPlayer.Instance.rightHand.controllerTransform.rotation = headRot * tilt;

                break;
            }

            case RigMode.Head:

            case RigMode.RecRoom:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void OnEnable() => IsEnabled = true;

    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class PCRigPatch
{
    private const float RotationThreshold    = 60f;
    private const float RecRoomRotationSpeed = 4f;
    public const  float SkellonRotationSpeed = 40f;

    private static float yRotation;

    private static void Postfix(VRRig __instance)
    {
        if (Tools.Utils.InVR || !PCRig.IsEnabled || !__instance.isLocal)
            return;

        switch (PCRig.CurrentRigMode)
        {
            case RigMode.RecRoom:
            {
                float cameraY = GorillaTagger.Instance.mainCamera.transform.eulerAngles.y;
                if (Mathf.Abs(Mathf.DeltaAngle(yRotation, cameraY)) > RotationThreshold)
                    yRotation = Mathf.LerpAngle(yRotation, cameraY, Time.deltaTime * RecRoomRotationSpeed);

                __instance.transform.rotation = Quaternion.Euler(__instance.transform.eulerAngles.x, yRotation,
                        __instance.transform.eulerAngles.z);

                __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.head.rigTarget.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                break;
            }

            case RigMode.Wings:
            case RigMode.Head:
                goto case RigMode.Skellon;

            case RigMode.Skellon:
            {
                __instance.head.rigTarget.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
                yRotation = Mathf.LerpAngle(yRotation, GorillaTagger.Instance.mainCamera.transform.eulerAngles.y,
                        Time.deltaTime * SkellonRotationSpeed);

                __instance.transform.rotation = Quaternion.Euler(__instance.transform.eulerAngles.x, yRotation,
                        __instance.transform.eulerAngles.z);

                __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.head.rigTarget.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                break;
            }
        }
    }
}