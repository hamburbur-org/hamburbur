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

    private static Vector3 handPos_L = Vector3.zero;
    private static Vector3 handRot_L = Vector3.zero;

    private static Vector3 handPos_R = Vector3.zero;
    private static Vector3 handRot_R = Vector3.zero;

    protected override void Update()
    {
        if (RigUtils.IsRigEnabled)
            RigUtils.ToggleRig(false);

        if (headPos == Vector3.zero)
            headPos = GorillaTagger.Instance.headCollider.transform.position;

        if (headRot == Vector3.zero)
            headRot = GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles;

        if (handPos_L == Vector3.zero)
            handPos_L = GorillaTagger.Instance.leftHandTransform.transform.position;

        if (handRot_L == Vector3.zero)
            handRot_L = GorillaTagger.Instance.leftHandTransform.transform.rotation.eulerAngles;

        if (handPos_R == Vector3.zero)
            handPos_R = GorillaTagger.Instance.rightHandTransform.transform.position;

        if (handRot_R == Vector3.zero)
            handRot_R = GorillaTagger.Instance.rightHandTransform.transform.rotation.eulerAngles;

        const float positionSpeed     = 0.01f;
        const float rotationSpeed     = 2.0f;
        const float positionThreshold = 0.05f;
        const float rotationThreshold = 11.5f;
        if (Vector3.Distance(headPos, GorillaTagger.Instance.headCollider.transform.position) > positionThreshold)
            headPos += Vector3.Normalize(GorillaTagger.Instance.headCollider.transform.position - headPos) *
                       positionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(headRot), GorillaTagger.Instance.headCollider.transform.rotation) >
            rotationThreshold)
            headRot = Quaternion.RotateTowards(Quaternion.Euler(headRot),
                    GorillaTagger.Instance.headCollider.transform.rotation, rotationSpeed).eulerAngles;

        if (Vector3.Distance(handPos_L, GorillaTagger.Instance.leftHandTransform.transform.position) >
            positionThreshold)
            handPos_L += Vector3.Normalize(GorillaTagger.Instance.leftHandTransform.transform.position - handPos_L) *
                         positionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(handRot_L), GorillaTagger.Instance.leftHandTransform.transform.rotation) >
            rotationThreshold)
            handRot_L = Quaternion.RotateTowards(Quaternion.Euler(handRot_L),
                    GorillaTagger.Instance.leftHandTransform.transform.rotation, rotationSpeed).eulerAngles;

        if (Vector3.Distance(handPos_R, GorillaTagger.Instance.rightHandTransform.transform.position) >
            positionThreshold)
            handPos_R += Vector3.Normalize(GorillaTagger.Instance.rightHandTransform.transform.position - handPos_R) *
                         positionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(handRot_R),
                    GorillaTagger.Instance.rightHandTransform.transform.rotation) > rotationThreshold)
            handRot_R = Quaternion.RotateTowards(Quaternion.Euler(handRot_R),
                    GorillaTagger.Instance.rightHandTransform.transform.rotation, rotationSpeed).eulerAngles;

        RigUtils.RigPosition = headPos - new Vector3(0f, 0.15f, 0f);
        RigUtils.RigRotation = Quaternion.Euler(new Vector3(0f, headRot.y, 0f));

        VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.Euler(headRot);

        VRRig.LocalRig.leftHand.rigTarget.transform.position  = handPos_L;
        VRRig.LocalRig.rightHand.rigTarget.transform.position = handPos_R;

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation  = Quaternion.Euler(handRot_L);
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler(handRot_R);

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

        handPos_L = Vector3.zero;
        handRot_L = Vector3.zero;

        handPos_R = Vector3.zero;
        handRot_R = Vector3.zero;

        RigUtils.ToggleRig(true);
    }
}