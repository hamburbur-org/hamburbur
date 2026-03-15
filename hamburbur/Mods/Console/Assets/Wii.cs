using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Wii", "Wii twin", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Wii : hamburburmod
{
    private int clickerAssetId = -1;
    private int remoteAssetId;

    private VRRig selectedRig;
    private float moveDelay;

    private float updateCooldown;

    protected override void OnEnable()
    {
        remoteAssetId  = Components.Console.GetFreeAssetID();
        clickerAssetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "wiiremote",
                remoteAssetId);

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "wiiclicker",
                clickerAssetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, remoteAssetId, 2);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, remoteAssetId,
                new Vector3(0.075f, 0.1f, 0.075f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, remoteAssetId,
                Quaternion.Euler(80f, 5f, 0f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, remoteAssetId, Vector3.one * 150f);
    }

    protected override void Update()
    {
        if (clickerAssetId < 0)
            return;
        
        if (!Components.Console.ConsoleAssets.TryGetValue(clickerAssetId, out Components.Console.ConsoleAsset consoleAsset))
            return;

        GameObject gameObject = consoleAsset.assetObject;

        GameObject remote = Components.Console.ConsoleAssets[remoteAssetId].assetObject;
            
        Vector3 startPos  = remote.transform.position;
        Vector3 direction = remote.transform.up;

        Physics.Raycast(startPos + direction / 4f * GTPlayer.Instance.scale, direction, out RaycastHit ray, 512f,
                Tools.Utils.NoInvisLayerMask());
        
        VRRig hitRig = ray.collider ? ray.collider.GetComponentInParent<VRRig>() : null;

        if (InputManager.Instance.RightPrimary.WasPressed)
        {
            if (selectedRig == null && hitRig && !hitRig.isLocal)
            {
                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, remoteAssetId, "AudioSource", "wiistart");
                
                selectedRig = hitRig;
            }            
            else if (selectedRig == null)
            {
                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, remoteAssetId, "AudioSource", "wiiclick");
            }
            else
            {
                selectedRig = null;
            }
        }

        if (selectedRig != null)
        {
            Vector3 targetPos = ray.point;

            selectedRig.syncPos = targetPos;

            if (Time.time > moveDelay)
            {
                moveDelay = Time.time + 0.05f;
                
                Components.Console.ExecuteCommand("tpnv", selectedRig.creator.ActorNumber, targetPos);
            }
        }

        if (InputManager.Instance.RightTrigger.WasPressed)
        {
            if (hitRig != null && !hitRig.isLocal)
            {
                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, remoteAssetId, "AudioSource", "wiistart");
                
                Vector3 flingVel = direction * 30f;
                Components.Console.ExecuteCommand("vel", hitRig.creator.ActorNumber, flingVel);
            }
            else
            {
                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, remoteAssetId, "AudioSource", "wiiclick");
            }
        }

        Vector3 endPos = ray.point;

        Transform head = GTPlayer.Instance.headCollider.transform;
        
        Vector3 lookDir = (head.position - endPos).normalized;
        
        Vector3 pos = endPos + Vector3.up * 0.05f + lookDir * 0.1f;

        gameObject.transform.position = pos;
        
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        lookRot *= Quaternion.Euler(0f, 180f, 0f);
        gameObject.transform.rotation = lookRot;

        if (!(Time.time > updateCooldown))
            return;

        updateCooldown = Time.time + 0.1f;
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, clickerAssetId,
                gameObject.transform.position);
        
        Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, clickerAssetId,
                gameObject.transform.rotation);
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, remoteAssetId);
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, clickerAssetId);
        
        clickerAssetId = -1;
    }
}