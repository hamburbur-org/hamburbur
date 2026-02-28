using System.Linq;
using System.Threading.Tasks;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Btools", "Btools twin", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Btools : hamburburmod
{
    private string btoolsAnimation = "Grab";

    private int                             btoolsId = -1;
    private float                           btoolsUpdateCooldown;
    private Components.Console.ConsoleAsset grabbingObject;
    private float                           grabUpdateCooldown;
    private bool                            lastGripBtools;
    private bool                            lastTrigger;
    private int                             toolId;

    protected override void Update()
    {
        bool rightGrab    = InputManager.Instance.RightGrip.IsPressed;
        bool rightTrigger = InputManager.Instance.RightTrigger.IsPressed;

        if (btoolsId < 0)
        {
            btoolsId = Components.Console.GetFreeAssetID();
            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "btools", "Btools", btoolsId);
            Tools.Utils.RPCProtection();

            return;
        }

        GameObject gameObject = Components.Console.ConsoleAssets[btoolsId].assetObject;

        if (rightGrab && !lastGripBtools)
            toolId++;

        toolId         %= 3;
        lastGripBtools =  rightGrab;

        Vector3 startPos  = GorillaTagger.Instance.rightHandTransform.position;
        Vector3 direction = Tools.Utils.RealRightController.forward;

        Physics.Raycast(startPos + direction / 4f * GTPlayer.Instance.scale, direction, out RaycastHit ray, 512f,
                Tools.Utils.NoInvisLayerMask());

        Vector3 endPos = ray.point;
        gameObject.transform.position = endPos + Vector3.up * 0.1f;

        if (Time.time > btoolsUpdateCooldown)
        {
            btoolsUpdateCooldown = Time.time + 0.1f;
            Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, btoolsId,
                    gameObject.transform.position);
        }

        Components.Console.ConsoleAsset targetObject = GetAssetFromObject(ray.collider?.gameObject);

        string btoolState = toolId switch
                            {
                                    0     => "Grab",
                                    1     => "Clone",
                                    2     => "Hammer",
                                    var _ => "Grab",
                            };

        switch (toolId)
        {
            case 0: // Grab
                if (rightTrigger)
                {
                    if (grabbingObject == null && targetObject != null) grabbingObject = targetObject;
                    if (grabbingObject != null)
                    {
                        btoolState = "GrabClick";
                        if (Time.time > grabUpdateCooldown)
                        {
                            grabUpdateCooldown = Time.time + 0.05f;
                            Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All,
                                    grabbingObject.assetId, endPos + Vector3.up);
                        }
                    }
                }
                else
                {
                    grabbingObject = null;
                }

                break;

            case 1: // Clone
                if (targetObject != null)
                {
                    btoolState = "CloneHover";
                    if (rightTrigger && !lastTrigger)
                    {
                        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, btoolsId,
                                "IconHolder", "Clone");

                        int cloneId = Components.Console.GetFreeAssetID();
                        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All,
                                targetObject.assetBundle, targetObject.assetName, cloneId);

                        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, cloneId,
                                targetObject.assetObject.transform.position + Vector3.up);

                        Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, cloneId,
                                targetObject.assetObject.transform.rotation);

                        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, cloneId,
                                targetObject.assetObject.transform.localScale);
                    }
                }

                break;

            case 2: // Hammer
                if (targetObject != null)
                {
                    btoolState = "HammerHover";
                    if (rightTrigger && !lastTrigger)
                    {
                        Explode(targetObject.assetObject.transform.position);
                        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All,
                                targetObject.assetId);
                    }
                }

                break;
        }

        lastTrigger = rightTrigger;

        if (btoolState != btoolsAnimation)
        {
            Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, btoolsId, "IconHolder",
                    btoolState);

            btoolsAnimation = btoolState;
        }
    }

    protected override void OnDisable()
    {
        if (btoolsId >= 0)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, btoolsId);
            btoolsId = -1;
        }
    }

    private Components.Console.ConsoleAsset GetAssetFromObject(GameObject obj)
    {
        if (obj == null) return null;

        return Components.Console.ConsoleAssets.Values.FirstOrDefault(asset => asset.assetObject != null &&
                                                                               obj.transform.IsChildOf(asset
                                                                                      .assetObject.transform));
    }

    private void Explode(Vector3 position, Vector3? scale = null, bool sound = true)
    {
        int explosionId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn",       ReceiverGroup.All, "btools", "Explosion", explosionId);
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, explosionId, position);

        if (scale != null)
            Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, explosionId, scale);

        if (!sound)
            Components.Console.ExecuteCommand("asset-stopsound", ReceiverGroup.All, explosionId, "Sound");
        else
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, explosionId, "Sound",
                    "Explode");

        Task.Run(async () =>
                 {
                     await Task.Delay(1000);
                     Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, explosionId);
                 });
    }
}