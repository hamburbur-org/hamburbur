using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Rat Gun", "A gun that lets you rat people", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class RatGun : hamburburmod
{
    private static readonly List<int> AssetIds = [];

    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private float spawnDelay;

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || Time.time < spawnDelay || gunLib.ChosenRig == null)
            return;

        spawnDelay = Time.time + 0.1f;

        int newId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All,
            "consolehamburburassets",
            "rat",
            newId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All,
            newId,
            0,
            gunLib.ChosenRig.creator.ActorNumber);

        Vector3 forwardOffset = new(0f, 0f, 0.5f);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All,
            newId,
            forwardOffset);

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All,
            newId,
            Vector3.zero);

        Components.Console.ExecuteCommand("asset-setlocalscale", ReceiverGroup.All,
            newId,
            Vector3.one);

        AssetIds.Add(newId);
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();

        foreach (int id in AssetIds)
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, id);

        AssetIds.Clear();
    }
}