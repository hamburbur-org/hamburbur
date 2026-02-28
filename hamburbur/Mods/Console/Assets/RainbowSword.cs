using GorillaLocomotion;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Rainbow Sword", "Spawns the rainbow sword asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class RainbowSword : hamburburmod
{
    private static int   allocatedRSwordId = -1;
    private static bool  lastVelTooHighRS;
    private static float pauseSfx;
    private static float slashDelay;

    protected override void Update()
    {
        if (allocatedRSwordId < 0)
        {
            allocatedRSwordId = Components.Console.GetFreeAssetID();
            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "rbsword", "Sword",
                    allocatedRSwordId);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedRSwordId, 2);
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedRSwordId, "Sword",
                    "Music");

            if (BigAssets.isEnabled)
                Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedRSwordId,
                        Vector3.one * 5);

            Tools.Utils.RPCProtection();
        }

        if (!Components.Console.ConsoleAssets.TryGetValue(allocatedRSwordId,
                    out Components.Console.ConsoleAsset asset))
            return;

        Transform rayPoint = asset.assetObject.transform.Find("Sword/HitBox");

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
                    Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedRSwordId,
                            "Sword/SFX", $"Slash{Random.Range(1, 3)}");

                    Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All,
                            allocatedRSwordId, "Sword", "Particles");

                    NetPlayer player = Target.Creator;
                    Components.Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
                }
            }
            catch { }

        bool velTooHigh = (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) -
                           GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;

        if (velTooHigh && !lastVelTooHighRS && Time.time > pauseSfx)
        {
            pauseSfx = Time.time + 0.3f;
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedRSwordId,
                    "Sword/SFX", $"Swing{Random.Range(1, 3)}");
        }

        lastVelTooHighRS = velTooHigh;
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedRSwordId);
        allocatedRSwordId = -1;
    }
}