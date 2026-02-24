using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;
using UnityEngine.XR;

namespace hamburbur.Mods.Movement;

[hamburburmod("Long Arms", "Long Arms", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LogNarms : hamburburmod
{
    private Vector3? lastFramePosition;

    protected override void Update()
    {
        Vector3 headPosition      = ControllerInputPoller.DevicePosition(XRNode.Head);
        Vector3 leftHandPosition  = ControllerInputPoller.DevicePosition(XRNode.LeftHand);
        Vector3 rightHandPosition = ControllerInputPoller.DevicePosition(XRNode.RightHand);

        bool bothHandsNotMoving =
                GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0).magnitude < 2f &&
                GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0).magnitude  < 2f;

        if (Vector3.Distance(leftHandPosition,  headPosition) < 0.2f &&
            Vector3.Distance(rightHandPosition, headPosition) < 0.2f &&
            bothHandsNotMoving)
        {
            Vector3 position = GorillaTagger.Instance.bodyCollider.transform.position;
            DisableSteamLongArms();

            if (lastFramePosition == null)
                Tools.Utils.TeleportPlayer(position);

            lastFramePosition = position;

            return;
        }

        if (lastFramePosition != null)
        {
            Tools.Utils.TeleportPlayer(lastFramePosition.Value);
            lastFramePosition = null;
        }

        GTPlayer.Instance.transform.localScale =
                Vector3.one * (VRRig.LocalRig.NativeScale * ChangeArmLength.CurrentValue);
    }

    protected override void OnDisable() => DisableSteamLongArms();

    private static void DisableSteamLongArms() =>
            GTPlayer.Instance.transform.localScale = Vector3.one * VRRig.LocalRig.NativeScale;
}