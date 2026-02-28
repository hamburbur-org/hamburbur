using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Physics Gun", "Spawns physics gun asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class PhysicsGun : hamburburmod
{
    private static int   allocatedPhysId = -1;
    private static bool  physGunLastGrip;
    private static VRRig rigTargetHold;
    private static float rigDistance;
    private static float physGunStandaloneTriggerDelay;
    private static float positionDelay;

    protected override void Update()
    {
        if (allocatedPhysId < 0)
        {
            allocatedPhysId = Components.Console.GetFreeAssetID();
            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "PhysicsGun",
                    allocatedPhysId);

            if (BigAssets.isEnabled)
                Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedPhysId,
                        Vector3.one * 5);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedPhysId, 2);
            Tools.Utils.RPCProtection();
        }

        if (!Components.Console.ConsoleAssets.ContainsKey(allocatedPhysId))
            return;

        Components.Console.ConsoleAsset asset    = Components.Console.ConsoleAssets[allocatedPhysId];
        Transform                       RayPoint = asset.assetObject.transform.Find("raypoint");

        Physics.Raycast(RayPoint.position, RayPoint.forward, out RaycastHit CrosshairRay, 512f,
                Tools.Utils.NoInvisLayerMask());

        GameObject crosshair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crosshair.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        crosshair.transform.position = CrosshairRay.point == Vector3.zero
                                               ? RayPoint.transform.position + RayPoint.transform.forward * 20f
                                               : CrosshairRay.point;

        crosshair.GetComponent<Renderer>().material.color = Plugin.Instance.MainColour;
        crosshair.Obliterate(Time.deltaTime);
        crosshair.GetComponent<Collider>().Obliterate();

        if (InputManager.Instance.RightGrip.IsPressed)
        {
            if (rigTargetHold == null)
            {
                Physics.Raycast(RayPoint.position, RayPoint.forward, out RaycastHit Ray, 512f,
                        Tools.Utils.NoInvisLayerMask());

                VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                if (gunTarget && !gunTarget.isLocal)
                {
                    rigTargetHold = gunTarget;
                    rigDistance   = Ray.distance;
                    Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedPhysId,
                            "model", "bright");

                    Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedPhysId,
                            "oneshot", "zap");

                    Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedPhysId,
                            "constant", "hold");
                }
            }
            else
            {
                if (Mathf.Abs(InputManager.Instance.RightJoystick.Axis.y) > 0.2f)
                    rigDistance += Time.deltaTime * (InputManager.Instance.RightJoystick.Axis.y > 0 ? 1f : -1f) * 4f;

                Vector3 targetPosition = RayPoint.transform.position + RayPoint.transform.forward * rigDistance;
                rigTargetHold.syncPos = targetPosition;

                if (Time.time > positionDelay)
                {
                    positionDelay = Time.time + 0.05f;
                    Components.Console.ExecuteCommand("tpnv", rigTargetHold.Creator.ActorNumber,
                            targetPosition);

                    Tools.Utils.RPCProtection();
                }
            }
        }

        if (physGunLastGrip && !InputManager.Instance.RightGrip.IsPressed && rigTargetHold != null)
        {
            if (InputManager.Instance.RightTrigger.IsPressed)
                Components.Console.ExecuteCommand("vel", rigTargetHold.Creator.ActorNumber,
                        RayPoint.transform.forward * 30f);

            Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedPhysId, "model",
                    InputManager.Instance.RightTrigger.IsPressed ? "flash" : "default");

            Components.Console.ExecuteCommand("asset-stopsound", ReceiverGroup.All, allocatedPhysId, "constant");
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedPhysId, "oneshot",
                    InputManager.Instance.RightTrigger.IsPressed ? $"launch{Random.Range(1, 4)}" : "drop");

            physGunStandaloneTriggerDelay = Time.time + 0.5f;
            rigTargetHold                 = null;
        }

        physGunLastGrip = InputManager.Instance.RightGrip.IsPressed;

        if (!InputManager.Instance.RightTrigger.IsPressed || InputManager.Instance.RightGrip.IsPressed ||
            !(Time.time > physGunStandaloneTriggerDelay))
            return;

        Physics.Raycast(RayPoint.position, RayPoint.forward, out RaycastHit Ray2, 512f, Tools.Utils.NoInvisLayerMask());
        VRRig gunTarget2 = Ray2.collider.GetComponentInParent<VRRig>();

        if (!gunTarget2 || gunTarget2.isLocal)
            return;

        physGunStandaloneTriggerDelay = Time.time + 0.5f;
        Components.Console.ExecuteCommand("vel", gunTarget2.Creator.ActorNumber,
                RayPoint.transform.forward * 30f);

        Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedPhysId, "model",
                "flash");

        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedPhysId, "oneshot",
                $"launch{Random.Range(1, 4)}");
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedPhysId);
        allocatedPhysId = -1;
    }
}