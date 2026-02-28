using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Kormakur Gun", "A gun that lets you spawn in kormakurs if they have console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class KormakurGun : hamburburmod
{
    private static readonly List<int> KormakurIds = [];

    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private float spawnDelay;

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || Time.time < spawnDelay)
            return;

        spawnDelay = Time.time + 0.1f;
        int newId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "effects", "starfall_rise", newId);
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, newId,
                gunLib.Hit.point + new Vector3(0f, 1f, 0f));

        Components.Console.ExecuteCommand("asset-settexture", ReceiverGroup.All, newId, "Cube.002",
                "https://raw-images.zlothy.uk/thisiswhokormakurreallyis.png");

        KormakurIds.Add(newId);
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();

        foreach (int id in KormakurIds)
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, id);

        KormakurIds.Clear();
    }
}