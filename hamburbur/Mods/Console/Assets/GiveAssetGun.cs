using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Give Asset Gun", "A gun that lets you give a player the selected asset", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class GiveAssetGun : hamburburmod
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
                ChangeAsset.Assets[ChangeAsset.Instance.IncrementalValue].file,
                ChangeAsset.Assets[ChangeAsset.Instance.IncrementalValue].prefabName, newId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, newId, 2,
                gunLib.ChosenRig.OwningNetPlayer.ActorNumber);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, newId,
                ChangeAsset.Assets[ChangeAsset.Instance.IncrementalValue].position);

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, newId,
                ChangeAsset.Assets[ChangeAsset.Instance.IncrementalValue].rotation);

        Components.Console.ExecuteCommand("asset-setlocalscale", ReceiverGroup.All, newId,
                ChangeAsset.Assets[ChangeAsset.Instance.IncrementalValue].scale);

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