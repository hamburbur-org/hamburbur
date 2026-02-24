using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Tag Lag Detector", "Sends a notification when there is most likely tag lag", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagLagDetector : hamburburmod
{
    private const  int   PingThreshold    = 800;
    private const  float HighPingDuration = 2f;
    private const  float LowPingDuration  = 3f;
    private static bool  lastTagLag;

    private float highPingStart;
    private float lowPingStart;

    protected override void Update()
    {
        if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient ||
            NetworkSystem.Instance.RoomPlayerCount <= 1)
        {
            ResetLag();

            return;
        }

        VRRig masterRig = PhotonNetwork.MasterClient.Rig();

        if (masterRig == null) return;

        int   ping = masterRig.Ping();
        float now  = Time.time;

        if (ping > PingThreshold)
        {
            if (highPingStart == 0f) highPingStart = now;
            lowPingStart = 0f;

            if (lastTagLag || !(now - highPingStart >= HighPingDuration))
                return;

            lastTagLag = true;
            NotificationManager.SendNotification(
                    "<color=orange>Comp</color>",
                    "There is currently tag lag.",
                    3f,
                    false,
                    true);
        }
        else
        {
            if (lowPingStart == 0f) lowPingStart = now;
            highPingStart = 0f;

            if (!lastTagLag || !(now - lowPingStart >= LowPingDuration))
                return;

            lastTagLag = false;
            NotificationManager.SendNotification(
                    "<color=orange>Comp</color>",
                    "There is no longer tag lag.",
                    3f,
                    false,
                    true);
        }
    }

    private void ResetLag()
    {
        if (lastTagLag)
            NotificationManager.SendNotification(
                    "<color=orange>Comp</color>",
                    "There is no longer tag lag.",
                    3f,
                    false,
                    true);

        lastTagLag    = false;
        highPingStart = 0f;
        lowPingStart  = 0f;
    }
}