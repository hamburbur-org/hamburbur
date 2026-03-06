using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Anti Report", "Does something when someone potentially tries to report you", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class AntiReport : hamburburmod
{
    public static float Range          = 0.5f;
    public static float NotifyCooldown = 2.5f;

    private readonly Dictionary<VRRig, float> lastNotifyTime = [];

    protected override void Update()
    {
        if (!NetworkSystem.Instance.InRoom) return;

        foreach (VRRig rig in GorillaScoreboardTotalUpdater.allScoreboardLines
                                                           .Where(line => line.linePlayer ==
                                                                          NetworkSystem.Instance.LocalPlayer)
                                                           .Select(line => line.reportButton.gameObject.transform)
                                                           .SelectMany(myReportButton =>
                                                                               from rig in VRRigCache.m_activeRigs
                                                                               where !rig.isLocal
                                                                               let leftHandDistance =
                                                                                       Vector3.Distance(
                                                                                               rig.leftHandTransform
                                                                                                      .position,
                                                                                               myReportButton.position)
                                                                               let rightHandDistance =
                                                                                       Vector3.Distance(
                                                                                               rig.rightHandTransform
                                                                                                      .position,
                                                                                               myReportButton.position)
                                                                               where leftHandDistance < Range ||
                                                                                   rightHandDistance  < Range
                                                                               select rig))
        {
            if (AntiReportType.Current == AntiReportType.AntiReportTypes.Notify)
            {
                float now = Time.time;

                if (lastNotifyTime.TryGetValue(rig, out float lastTime))
                    if (now - lastTime < NotifyCooldown)
                        continue;

                lastNotifyTime[rig] = now;
            }

            string cachedRigName = rig.OwningNetPlayer().SanitizedNickName;
            string actionText    = "";

            AntiReportType.AntiReportTypes currentType = AntiReportType.Current;

            switch (currentType)
            {
                case AntiReportType.AntiReportTypes.Reconnect:
                {
                    string codeName = NetworkSystem.Instance.RoomName;
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                    CoroutineManager.Instance.StartCoroutine(JoinRoom(codeName, 2f));
                    actionText = $"Attempting to reconnect to {codeName}";

                    break;
                }

                case AntiReportType.AntiReportTypes.JoinRandom:
                {
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                    JoinRandom();
                    actionText = "Joining a random public.";

                    break;
                }

                case AntiReportType.AntiReportTypes.Notify:
                {
                    //do nothing
                    break;
                }

                case AntiReportType.AntiReportTypes.Disconnect:
                default:
                {
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                    actionText = "You have been disconnected.";

                    break;
                }
            }

            NotificationManager.SendNotification(
                    "<color=red>Safety</color>",
                    $"Player {cachedRigName} tried to report you. " + actionText,
                    5f,
                    true,
                    true);
        }
    }

    private IEnumerator JoinRoom(string roomName, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        PhotonNetworkController.Instance.AttemptToAutoJoinSpecificRoom(roomName, JoinType.Solo);
    }

    private async void JoinRandom()
    {
        try
        {
            if (NetworkSystem.Instance.InRoom)
                await NetworkSystem.Instance.ReturnToSinglePlayer();

            else
                Debug.Log("Not connected to a room.");

            string networkZone = PhotonNetworkController.Instance.currentJoinTrigger == null
                                         ? "forest"
                                         : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;

            PhotonNetworkController.Instance.AttemptToJoinPublicRoom(
                    GorillaComputer.instance.GetJoinTriggerForZone(networkZone));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}