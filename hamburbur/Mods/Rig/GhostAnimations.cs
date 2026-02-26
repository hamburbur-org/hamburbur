using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod(                "Ghost Animations", "Makes your movement look more like a 'ghost'", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GhostAnimations : hamburburmod
{
    private static Vector3 headPos = Vector3.zero;
    private static Vector3 headRot = Vector3.zero;

    private static Vector3 handPosL = Vector3.zero;
    private static Vector3 handRotL = Vector3.zero;

    private static Vector3 handPosR = Vector3.zero;
    private static Vector3 handRotR = Vector3.zero;

    protected override void Update()
    {
        if (RigUtils.IsRigEnabled)
            RigUtils.ToggleRig(false);

        if (headPos == Vector3.zero)
            headPos = GorillaTagger.Instance.headCollider.transform.position;

        if (headRot == Vector3.zero)
            headRot = GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles;

        if (handPosL == Vector3.zero)
            handPosL = GorillaTagger.Instance.leftHandTransform.transform.position;

        if (handRotL == Vector3.zero)
            handRotL = GorillaTagger.Instance.leftHandTransform.transform.rotation.eulerAngles;

        if (handPosR == Vector3.zero)
            handPosR = GorillaTagger.Instance.rightHandTransform.transform.position;

        if (handRotR == Vector3.zero)
            handRotR = GorillaTagger.Instance.rightHandTransform.transform.rotation.eulerAngles;

        const float PositionSpeed     = 0.05f;
        const float RotationSpeed     = 2.0f;
        const float PositionThreshold = 0.05f;
        const float RotationThreshold = 11.5f;
        if (Vector3.Distance(headPos, GorillaTagger.Instance.headCollider.transform.position) > PositionThreshold)
            headPos += Vector3.Normalize(GorillaTagger.Instance.headCollider.transform.position - headPos) *
                       PositionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(headRot), GorillaTagger.Instance.headCollider.transform.rotation) >
            RotationThreshold)
            headRot = Quaternion.RotateTowards(Quaternion.Euler(headRot),
                    GorillaTagger.Instance.headCollider.transform.rotation, RotationSpeed).eulerAngles;

        if (Vector3.Distance(handPosL, GorillaTagger.Instance.leftHandTransform.transform.position) >
            PositionThreshold)
            handPosL += Vector3.Normalize(GorillaTagger.Instance.leftHandTransform.transform.position - handPosL) *
                         PositionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(handRotL), GorillaTagger.Instance.leftHandTransform.transform.rotation) >
            RotationThreshold)
            handRotL = Quaternion.RotateTowards(Quaternion.Euler(handRotL),
                    GorillaTagger.Instance.leftHandTransform.transform.rotation, RotationSpeed).eulerAngles;

        if (Vector3.Distance(handPosR, GorillaTagger.Instance.rightHandTransform.transform.position) >
            PositionThreshold)
            handPosR += Vector3.Normalize(GorillaTagger.Instance.rightHandTransform.transform.position - handPosR) *
                         PositionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(handRotR),
                    GorillaTagger.Instance.rightHandTransform.transform.rotation) > RotationThreshold)
            handRotR = Quaternion.RotateTowards(Quaternion.Euler(handRotR),
                    GorillaTagger.Instance.rightHandTransform.transform.rotation, RotationSpeed).eulerAngles;

        RigUtils.RigPosition = headPos - new Vector3(0f, 0.15f, 0f);
        RigUtils.RigRotation = Quaternion.Euler(new Vector3(0f, headRot.y, 0f));

        VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.Euler(headRot);

        VRRig.LocalRig.leftHand.rigTarget.transform.position  = handPosL;
        VRRig.LocalRig.rightHand.rigTarget.transform.position = handPosR;

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation  = Quaternion.Euler(handRotL);
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler(handRotR);

        VRRig.LocalRig.leftIndex.calcT  = ControllerInputPoller.instance.leftControllerIndexFloat;
        VRRig.LocalRig.leftMiddle.calcT = InputManager.Instance.LeftGrip.IsPressed ? 1 : 0;
        VRRig.LocalRig.leftThumb.calcT =
                InputManager.Instance.LeftPrimary.IsPressed || InputManager.Instance.LeftSecondary.IsPressed ? 1 : 0;

        VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
        VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
        VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

        VRRig.LocalRig.rightIndex.calcT  = ControllerInputPoller.instance.rightControllerIndexFloat;
        VRRig.LocalRig.rightMiddle.calcT = InputManager.Instance.RightGrip.IsPressed ? 1 : 0;
        VRRig.LocalRig.rightThumb.calcT =
                InputManager.Instance.RightPrimary.IsPressed || InputManager.Instance.RightSecondary.IsPressed ? 1 : 0;

        VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
        VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
        VRRig.LocalRig.rightThumb.LerpFinger(1f, false);

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation *=
                Quaternion.Euler(VRRig.LocalRig.leftHand.trackingRotationOffset);

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation *=
                Quaternion.Euler(VRRig.LocalRig.rightHand.trackingRotationOffset);
    }

    protected override void OnDisable()
    {
        headPos = Vector3.zero;
        headRot = Vector3.zero;

        handPosL = Vector3.zero;
        handRotL = Vector3.zero;

        handPosR = Vector3.zero;
        handRotR = Vector3.zero;

        RigUtils.ToggleRig(true);
    }
}