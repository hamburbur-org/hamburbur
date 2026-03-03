using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Asset Gun", "A gun that lets you spawn in the selected asset if they have console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class AssetGun : hamburburmod
{
    public static List<int> AssetIds = [];

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
        Components.Console.ExecuteCommand("asset-spawn",    ReceiverGroup.All, ChangeAsset.assets[ChangeAsset.Instance.IncrementalValue].file,
            ChangeAsset.assets[ChangeAsset.Instance.IncrementalValue].prefabName, newId);
        
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, newId,
                gunLib.Hit.point);
        
        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, newId,
            Vector3.one * ChangeAssetScale.Instance.IncrementalValue);

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