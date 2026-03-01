using GorillaLocomotion;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Karambit", "Spawns the karambit asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class Karambit : hamburburmod
{
    private int   allocatedAssetId = -1;
    private bool  lastVelTooHighRS;
    private float pauseSfx;
    private float slashDelay;

    protected override void Update()
    {
        if (allocatedAssetId < 0)
        {
            allocatedAssetId = Components.Console.GetFreeAssetID();

            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "karambit", "karambit",
                    allocatedAssetId);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedAssetId, 2);
            Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, allocatedAssetId,
                    new Vector3(0.045f, 0.065f, 0f));

            Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, allocatedAssetId,
                    Quaternion.Euler(270f, 60f, 0f));

            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedAssetId, "Collider",
                    "csgo knife");
        }

        if (!Components.Console.ConsoleAssets.TryGetValue(allocatedAssetId,
                    out Components.Console.ConsoleAsset asset) || asset.assetObject == null)
            return;

        Transform rayPoint = asset.assetObject.transform.Find("Collider");

        if (rayPoint == null) return;

        Physics.SphereCast(rayPoint.position, 0.1f, rayPoint.forward, out RaycastHit Ray, 0.7f,
                Tools.Utils.NoInvisLayerMask());

        if (Time.time > slashDelay && Ray.collider != null)
            try
            {
                VRRig Target = Ray.collider.GetComponentInParent<VRRig>();
                if (Target != null && !Target.isLocal)
                {
                    slashDelay = Time.time + 0.5f;
                    pauseSfx   = Time.time + 1f;

                    Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedAssetId,
                            "Collider", "Stab");

                    Components.Console.ExecuteCommand("vel", Target.Creator.ActorNumber,
                            (Target.transform.position - GorillaTagger.Instance.rightHandTransform.position)
                           .normalized * 1.2f);
                }
            }
            catch { }

        bool velTooHigh = (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) -
                           GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;

        if (velTooHigh && !lastVelTooHighRS && Time.time > pauseSfx)
        {
            pauseSfx = Time.time + 0.3f;
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedAssetId, "Stab",
                    "csgo knife");
        }

        lastVelTooHighRS = velTooHigh;
    }

    protected override void OnDisable()
    {
        if (allocatedAssetId >= 0)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedAssetId);
            allocatedAssetId = -1;
        }
    }
}