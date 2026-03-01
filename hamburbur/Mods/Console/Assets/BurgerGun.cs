using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Burger Gun", "A gun that lets you spawn in burgers if they have console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class BurgerGun : hamburburmod
{
    public static List<int> BurgerIds = [];

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
        Components.Console.ExecuteCommand("asset-spawn",    ReceiverGroup.All, "consolehamburburassets", "burger", newId);
        
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, newId,
                gunLib.Hit.point + new Vector3(0f, 1f, 0f));

        BurgerIds.Add(newId);
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();

        foreach (int id in BurgerIds)
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, id);

        BurgerIds.Clear();
    }
}