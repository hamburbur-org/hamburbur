using System.Linq;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Leviathan Axe", "Spawns the Leviathan Axe asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class LeviathanAxe : hamburburmod
{
    private int   allocatedSwordId = -1;
    private bool  lastVelTooHigh;
    private float swingDelay;

    protected override void Update()
    {
        if (allocatedSwordId < 0)
        {
            allocatedSwordId = Components.Console.GetFreeAssetID();

            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "leviathan", "Sword",
                    allocatedSwordId);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedSwordId, 2);
        }

        if (!Components.Console.ConsoleAssets.TryGetValue(allocatedSwordId,
                    out Components.Console.ConsoleAsset asset) || asset.assetObject == null)
            return;

        bool velTooHigh = (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) -
                           GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;

        bool didHit = false;

        if (velTooHigh && !lastVelTooHigh && Time.time > swingDelay)
        {
            swingDelay = Time.time + 0.3f;

            foreach (VRRig rig in GorillaParent.instance.vrrigs.Where(r =>
                                                                              Vector3.Distance(
                                                                                      r.bodyRenderer.transform.position,
                                                                                      asset.assetObject.transform
                                                                                             .GetChild(1).position) <
                                                                              0.25f))
            {
                didHit = true;

                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId,
                        "Model", "Hit");

                Components.Console.ExecuteCommand("vel", rig.Creator.ActorNumber,
                        (rig.transform.position - GorillaTagger.Instance.rightHandTransform.position).normalized * 4f);

                break;
            }

            if (!didHit)
                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId,
                        "Model", "Swing");
        }

        lastVelTooHigh = velTooHigh;
    }

    protected override void OnDisable()
    {
        if (allocatedSwordId >= 0)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedSwordId);
            allocatedSwordId = -1;
        }
    }
}