using System;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Misc;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod("Hoverboard Gun", "Lets you spawn hoverboards everywhere", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class HoverboardGun : hamburburmod
{
    private readonly GunLib gunLib = new();

    private float lastTime;

    private         bool   wasShooting;
    public override Type[] Dependencies => [typeof(HoverboardsAnywhere),];

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (!isShooting || wasShooting || !(Time.time - lastTime > 0.5f))
            return;

        lastTime = Time.time;

        FreeHoverboardManager.instance.SendDropBoardRPC(gunLib.Hit.point, Quaternion.identity, Vector3.zero,
                new Vector3(0f, 10f, 0f), Plugin.Instance.MainColour);

        Tools.Utils.RPCProtection();
    }

    protected override void OnDisable() => gunLib.OnDisable();
}