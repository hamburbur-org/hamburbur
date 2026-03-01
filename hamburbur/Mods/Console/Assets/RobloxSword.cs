using GorillaLocomotion;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(                      "Roblox Sword", "Spawns the roblox sword asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class RobloxSword : hamburburmod
{
    private static int   allocatedSwordId = -1;
    private static bool  lastVelTooHigh;
    private static float swingDelay;

    protected override void Update()
    {
        if (allocatedSwordId < 0)
        {
            allocatedSwordId = Components.Console.GetFreeAssetID();
            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Sword",
                    allocatedSwordId);

            if (BigAssets.isEnabled)
                Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedSwordId,
                        Vector3.one * 5);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedSwordId, 2);
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId, "Model",
                    "Unsheath");

            Tools.Utils.RPCProtection();
        }

        bool velTooHigh = (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) -
                           GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;

        if (velTooHigh && !lastVelTooHigh && Time.time > swingDelay)
        {
            swingDelay = Time.time + 0.3f;
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId, "Model",
                    "Slash");
        }

        lastVelTooHigh = velTooHigh;
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedSwordId);
        allocatedSwordId = -1;
    }
}