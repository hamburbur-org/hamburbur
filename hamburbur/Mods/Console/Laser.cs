using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod(                   "Laser", "Shoot a laser out of your palm that kicks people", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class Laser : hamburburmod
{
    private static float adminEventDelay;
    private        bool  lastLasering;

    protected override void LateUpdate()
    {
        if (InputManager.Instance.LeftPrimary.IsPressed || InputManager.Instance.RightPrimary.IsPressed)
        {
            Vector3 dir = InputManager.Instance.RightPrimary.IsPressed
                                  ? VRRig.LocalRig.rightHandTransform.right
                                  : -VRRig.LocalRig.leftHandTransform.right;

            Vector3 startPos =
                    (InputManager.Instance.RightPrimary.IsPressed
                             ? VRRig.LocalRig.rightHandTransform.position
                             : VRRig.LocalRig.leftHandTransform.position) + dir * 0.1f;

            try
            {
                Physics.Raycast(startPos + dir / 3f, dir, out RaycastHit Ray, 512f, Tools.Utils.NoInvisLayerMask());
                VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                if (gunTarget && !gunTarget.isLocal)
                    Components.Console.ExecuteCommand("silkick", ReceiverGroup.All,
                            gunTarget.Creator.UserId);
            }
            catch { }

            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.1f;
                Components.Console.ExecuteCommand("laser", ReceiverGroup.All, true,
                        InputManager.Instance.RightPrimary.IsPressed);
            }
        }

        bool isLasering = InputManager.Instance.LeftPrimary.IsPressed || InputManager.Instance.RightPrimary.IsPressed;
        if (lastLasering && !isLasering)
            Components.Console.ExecuteCommand("laser", ReceiverGroup.All, false, false);

        lastLasering = isLasering;
    }
}