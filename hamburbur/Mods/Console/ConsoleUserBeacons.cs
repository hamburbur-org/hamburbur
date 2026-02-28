using System;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod("Console User Beacons", "Shows beacons on console users above and below their bodies",
        ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class ConsoleUserBeacons : hamburburmod
{
    public static bool IsEnabled;

    public static readonly  List<string>                                                 UserTags      = [];
    private static readonly Dictionary<string, (GameObject upLine, GameObject downLine)> ActiveBeacons = new();

    public override Type[] Dependencies => [typeof(AutoGetConsoleUsers),];

    protected override void Update()
    {
        foreach (string playerId in UserTags.ToList())
        {
            VRRig rig = playerId.Rig();
            if (rig == null)
            {
                if (ActiveBeacons.TryGetValue(playerId, out (GameObject upLine, GameObject downLine) old))
                {
                    old.upLine.Obliterate();
                    old.downLine.Obliterate();
                }

                ActiveBeacons.Remove(playerId);
                UserTags.Remove(playerId);

                continue;
            }

            if (!ActiveBeacons.TryGetValue(playerId, out (GameObject upLine, GameObject downLine) beaconPair))
            {
                GameObject   upLineObj = new("BeaconUp");
                LineRenderer upLine    = upLineObj.AddComponent<LineRenderer>();
                SetupLine(upLine);

                GameObject   downLineObj = new("BeaconDown");
                LineRenderer downLine    = downLineObj.AddComponent<LineRenderer>();
                SetupLine(downLine);

                beaconPair              = (upLineObj, downLineObj);
                ActiveBeacons[playerId] = beaconPair;
            }

            LineRenderer upLr   = beaconPair.upLine.GetComponent<LineRenderer>();
            LineRenderer downLr = beaconPair.downLine.GetComponent<LineRenderer>();

            Vector3 headPos = rig.headMesh.transform.position + new Vector3(0, rig.scaleFactor * 0.4f, 0);
            Vector3 footPos =
                    rig.transform.position -
                    new Vector3(0, rig.scaleFactor * 0.5f, 0);

            upLr.SetPosition(0, headPos);
            upLr.SetPosition(1, headPos + Vector3.up * 1000f);

            downLr.SetPosition(0, footPos);
            downLr.SetPosition(1, footPos + Vector3.down * 1000f);
        }
    }

    protected override void OnEnable()
    {
        IsEnabled = true;

        if (NetworkSystem.Instance.InRoom)
            foreach (Player player in PhotonNetwork.PlayerListOthers)
                AutoGetConsoleUsers.Instance.PingForConsole(player);
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        foreach ((GameObject upLine, GameObject downLine) pair in ActiveBeacons.Values)
        {
            pair.upLine.Obliterate();
            pair.downLine.Obliterate();
        }

        ActiveBeacons.Clear();
    }

    private void SetupLine(LineRenderer lr)
    {
        lr.startWidth     = 0.1f;
        lr.endWidth       = 0.1f;
        lr.material       = new Material(Shader.Find("GUI/Text Shader"));
        lr.material.color = Plugin.Instance.MainColour;
        lr.positionCount  = 2;
    }
}