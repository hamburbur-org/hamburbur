using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod("Cherry Bomb", "Spawns the cherry bomb asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class CherryBomb : hamburburmod
{
    private int   allocatedId = -1;
    private bool  thing;
    private float timeSinceSpawn;

    protected override void Update()
    {
        if (allocatedId < 0)
        {
            allocatedId = Components.Console.GetFreeAssetID();

            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "cherrybomb", "beam",
                    allocatedId);

            Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, allocatedId,
                    GorillaTagger.Instance.bodyCollider.transform.position + new Vector3(0f, 9.5f, 0f) +
                    GorillaTagger.Instance.bodyCollider.transform.forward * -0.25f);

            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedId, "beam",
                    "cherrybomb");

            Tools.Utils.RPCProtection();

            timeSinceSpawn = Time.time + 3.66f;
        }

        if (Time.time <= timeSinceSpawn) return;

        if (!thing)
        {
            thing = true;
            Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedId, "beam",
                    "show");
        }

        Tools.Utils.TeleportPlayer(Vector3.Lerp(GorillaTagger.Instance.bodyCollider.transform.position,
                Components.Console.ConsoleAssets[allocatedId].assetObject.transform.position +
                new Vector3(0f, -2f + Mathf.Sin(Time.time * 5f) * 1.25f, 0f), 0.01f));

        GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
    }

    protected override void OnDisable()
    {
        if (allocatedId < 0)
            return;

        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedId);
        allocatedId    = -1;
        timeSinceSpawn = -1;
        thing          = false;
    }
}