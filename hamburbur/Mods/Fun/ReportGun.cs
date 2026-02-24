using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Fun;

[hamburburmod("Report Gun", "Report dat bitchass ho!", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class ReportGun : hamburburmod
{
    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    private readonly List<string> susReasons =
    [
            "missing player ids",
            "too many players",
            "room host force changed",
            "too many rpc calls!",
            "empty rig",
            "too far tag",
            "invalid RPC stuff",
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
            "too many rpc calls! " + GetRandomRpc(),
    ];

    private float lastTime;

    protected override void Start()
    {
        gunLib.Start();
    }
    
    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (!isShooting || gunLib.ChosenRig == null || Time.time - lastTime < 3f)
            return;

        lastTime = Time.time;

        NotificationManager.SendNotification("<color=red>Anti-Cheat</color>",
                $"MonkeAgent reported {gunLib.ChosenRig.OwningNetPlayer().SanitizedNickName} for: {susReasons[Random.Range(0, susReasons.Count)]}",
                5f, true, true);
    }

    private static string GetRandomRpc()
    {
        List<string> rpcList = PhotonNetwork.photonServerSettings.RpcList;

        return rpcList[Random.Range(0, rpcList.Count)];
    }

    protected override void OnDisable() => gunLib.OnDisable();
}