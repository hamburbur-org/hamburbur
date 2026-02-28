using System.Collections;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(                "Battle Arena", "Creates a battle arena", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.Disabled, 0)]
public class BattleArena : hamburburmod
{
    private int assetId;

    private Vector3   cachedStartPosition;
    private Coroutine platfRoutine;

    protected override void OnEnable()
    {
        cachedStartPosition = GorillaTagger.Instance.bodyCollider.transform.position;

        platfRoutine = CoroutineManager.Instance.StartCoroutine(PlatFRoutine());

        Components.Console.ExecuteCommand("tpsmooth", ReceiverGroup.All, new Vector3(504.92f, 51f, 500.87f), 2f);

        assetId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "VideoPlayer", assetId);
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(486f, 53f, 500f));

        Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId,
                Quaternion.Euler(0f, 90f, 0f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId,
                new Vector3(0.6f, 0.6f, 0.6f));

        /* wut
                Received event 68 from -
                [
                asset-setrotation,
                582010513,
                (0.00000, -0.09734, 0.00000, -0.99525)
                ]

               Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All,assetId, new Vector3(0.00000, -0.09734, 0.00000, -0.99525));
         */

        Components.Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "Video",
                "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/Playboi%20Cart%20-%20Sky.mp4");

        Components.Console.ExecuteCommand("notify", ReceiverGroup.All, "♪ Arena opened — Playboi Carti: Sky ♪");
    }

    protected override void OnDisable()
    {
        CoroutineManager.Instance.StopCoroutine(platfRoutine);
        platfRoutine = null;

        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);

        Components.Console.ExecuteCommand("tpsmooth", ReceiverGroup.All, cachedStartPosition, 2f);
    }

    private IEnumerator PlatFRoutine()
    {
        while (true)
        {
            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 49.5f, 500f),
                    new Vector3(30f, 0.5f, 30f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 49.78f, 500f),
                    new Vector3(20f, 0.06f, 20f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 53f, 515f),
                    new Vector3(30f, 6f, 1.2f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 53f, 485f),
                    new Vector3(30f, 6f, 1.2f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(515f, 53f, 500f),
                    new Vector3(1.2f, 6f, 30f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(485f, 53f, 500f),
                    new Vector3(1.2f, 6f, 30f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(514f, 54.5f, 514f),
                    new Vector3(2f, 9f, 2f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(486f, 54.5f, 514f),
                    new Vector3(2f, 9f, 2f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(514f, 54.5f, 486f),
                    new Vector3(2f, 9f, 2f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(486f, 54.5f, 486f),
                    new Vector3(2f, 9f, 2f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 56.3f, 515f),
                    new Vector3(32f, 0.9f, 1.8f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 56.3f, 485f),
                    new Vector3(32f, 0.9f, 1.8f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(515f, 56.3f, 500f),
                    new Vector3(1.8f, 0.9f, 32f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(485f, 56.3f, 500f),
                    new Vector3(1.8f, 0.9f, 32f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(511f, 53f, 511f),
                    new Vector3(0.25f, 3.5f, 0.25f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(511f, 55f, 511f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 45f, 0f), 1f, 0.45f, 0.05f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(489f, 53f, 511f),
                    new Vector3(0.25f, 3.5f, 0.25f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(489f, 55f, 511f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 45f, 0f), 1f, 0.45f, 0.05f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(511f, 53f, 489f),
                    new Vector3(0.25f, 3.5f, 0.25f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(511f, 55f, 489f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 45f, 0f), 1f, 0.45f, 0.05f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(489f, 53f, 489f),
                    new Vector3(0.25f, 3.5f, 0.25f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(489f, 55f, 489f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 45f, 0f), 1f, 0.45f, 0.05f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 51.5f, 511f),
                    new Vector3(20f, 1f, 3f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 53f, 512f),
                    new Vector3(20f, 1f, 2f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 51.5f, 489f),
                    new Vector3(20f, 1f, 3f), Vector3.zero, 0.1694782f, 0.1504984f, 0.3584906f, 1f, 3600f);

            Components.Console.ExecuteCommand("platf", ReceiverGroup.All, new Vector3(500f, 53f, 488f),
                    new Vector3(20f, 1f, 2f), Vector3.zero, 0.3f, 0.26f, 0.22f, 1f, 3600f);

            yield return new WaitForSeconds(10);
        }
    }
}