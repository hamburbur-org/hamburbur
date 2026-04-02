using System;
using GorillaNetworking;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Room;

[hamburburmod("Join random", "Makes you join a random public room", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinRandom : hamburburmod
{
    protected override void Pressed() => JoinRandomRoom();

    private async void JoinRandomRoom()
    {
        try
        {
            if (NetworkSystem.Instance.InRoom)
                await NetworkSystem.Instance.ReturnToSinglePlayer();

            else
                Debug.Log("Not connected to a room.");

            string networkZone = PhotonNetworkController.Instance.currentJoinTrigger == null ||
                                 PhotonNetworkController.Instance.currentJoinTrigger.name.ToLower().Contains("private")
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