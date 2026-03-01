using System.Collections;
using System.Linq;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Ban Hammer", "Spawns the ban hammer asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class BanHammer : hamburburmod
{
    private int   allocatedBanHammerId = -1;
    private bool  lastVelTooHighRS;
    private float pauseSfx;
    private float slashDelay;

    protected override void OnEnable()
    {
        if (allocatedBanHammerId >= 0)
            return;

        allocatedBanHammerId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "banhammer", "BanHammer",
                allocatedBanHammerId);

        if (BigAssets.isEnabled)
            Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedBanHammerId,
                    Vector3.one * 5);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedBanHammerId, 2);

        Tools.Utils.RPCProtection();
    }

    protected override void OnDisable()
    {
        if (allocatedBanHammerId >= 0)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedBanHammerId);
            allocatedBanHammerId = -1;
        }
    }

    protected override void Update()
    {
        if (allocatedBanHammerId < 0) return;
        if (!Components.Console.ConsoleAssets.ContainsKey(allocatedBanHammerId)) return;

        Components.Console.ConsoleAsset asset    = Components.Console.ConsoleAssets[allocatedBanHammerId];
        Transform                       RayPoint = asset.assetObject.transform.Find("Model/HitBox");

        if (!RayPoint.TryGetComponent(out MeshCollider _))
            RayPoint.gameObject.AddComponent<MeshCollider>();

        Physics.SphereCast(RayPoint.position, 0.2f, RayPoint.forward, out RaycastHit Ray, 0.4f,
                Tools.Utils.NoInvisLayerMask());

        Physics.SphereCast(RayPoint.position, 0.2f, RayPoint.forward, out RaycastHit ColliderRay, 0.4f,
                GTPlayer.Instance.locomotionEnabledLayers);

        bool velTooHigh =
                (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) -
                 GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;

        if (Time.time > slashDelay)
        {
            if (Ray.collider != null)
            {
                VRRig Target = Ray.collider.GetComponentInParent<VRRig>();
                if (Target != null && !Target.isLocal)
                {
                    slashDelay = Time.time + 1f;
                    pauseSfx   = Time.time + 1f;

                    CoroutineManager.Instance.StartCoroutine(KillFX());

                    NetPlayer player = Target.Creator;
                    Components.Console.ExecuteCommand("block", player.ActorNumber, 100L);
                }
            }

            if (ColliderRay.collider != null)
            {
                slashDelay = Time.time + 0.3f;
                pauseSfx   = Time.time + 0.5f;

                Vector3 surfaceNormal = ColliderRay.normal;
                Vector3 handVelocity  = GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
                Vector3 bodyVelocity  = GorillaTagger.Instance.rigidbody.linearVelocity;
                float   totalVelocity = handVelocity.magnitude + bodyVelocity.magnitude;
                float   pushStrength  = Mathf.Clamp(totalVelocity, 1f, 14f);
                GorillaTagger.Instance.rigidbody.linearVelocity += surfaceNormal * pushStrength;

                CoroutineManager.Instance.StartCoroutine(HitFX());
            }
        }

        if (velTooHigh && !lastVelTooHighRS && Time.time > pauseSfx)
        {
            pauseSfx = Time.time + 0.3f;
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedBanHammerId,
                    "Model/SwingSFX", "Swing");
        }

        lastVelTooHighRS = velTooHigh;
    }

    private IEnumerator HitFX()
    {
        Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedBanHammerId,
                "Model", "Default");

        yield return null;
        yield return null;
        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedBanHammerId,
                "Model/SwingSFX", "HammerHit");

        Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedBanHammerId,
                "Model", "HitGround");

        foreach (VRRig rig in GorillaParent.instance.vrrigs.Where(rig => Vector3.Distance(
                                                                                 GorillaTagger.Instance
                                                                                        .rightHandTransform.position,
                                                                                 rig.transform.position) < 2f))
            Components.Console.ExecuteCommand("vel", rig.Creator.ActorNumber,
                    (rig.transform.position - GorillaTagger.Instance.rightHandTransform.position).normalized * 5f);
    }

    private IEnumerator KillFX()
    {
        Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedBanHammerId,
                "Model", "Default");

        yield return null;
        yield return null;
        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedBanHammerId,
                "Model/KillSFX", "HammerKill");

        Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedBanHammerId,
                "Model", "HitPlayer");
    }
}