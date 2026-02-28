using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(                      "Scythe", "Spawns the scythe asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class Scythe : hamburburmod
{
    private static int   scytheId = -1;
    private static float slashDelay;
    private static float pauseSfx;

    protected override void Update()
    {
        if (scytheId < 0)
        {
            scytheId = Components.Console.GetFreeAssetID();
            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "mistscythe", "Scythe",
                    scytheId);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, scytheId, 2);
            Tools.Utils.RPCProtection();
        }

        if (!Components.Console.ConsoleAssets.ContainsKey(scytheId))
            return;

        Components.Console.ConsoleAsset asset    = Components.Console.ConsoleAssets[scytheId];
        Transform                       RayPoint = asset.assetObject.transform;

        Physics.SphereCast(RayPoint.position, 0.1f, RayPoint.forward, out RaycastHit Ray, 0.7f,
                Tools.Utils.NoInvisLayerMask());

        if (!(Time.time > slashDelay) || Ray.collider == null)
            return;

        try
        {
            VRRig Target = Ray.collider.GetComponentInParent<VRRig>();

            if (Target == null || Target.isLocal)
                return;

            slashDelay = Time.time + 0.5f;
            pauseSfx   = Time.time + 1f;

            NetPlayer player = Target.Creator;
            Components.Console.ExecuteCommand("silkick", player.ActorNumber, player.UserId);
        }
        catch { }
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, scytheId);
        scytheId = -1;
    }
}