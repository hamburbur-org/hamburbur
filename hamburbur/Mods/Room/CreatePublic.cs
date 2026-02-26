using System;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts;
using hamburbur.Mod_Backend;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace hamburbur.Mods.Room;

[hamburburmod(                "Create Public",      "Creates a public room so you have master", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class CreatePublic : hamburburmod
{
    protected override void Pressed()
    {
        string         roomName     = PhotonNetworkController.Instance.RandomRoomName();
        const bool     IsPublic     = true;
        const JoinType RoomJoinType = JoinType.Solo;

        RoomConfig roomConfig = new()
        {
                createIfMissing = true,
                isJoinable      = true,
                isPublic        = IsPublic,
                MaxPlayers = RoomSystem.GetRoomSizeForCreate(
                        (PhotonNetworkController.Instance.currentJoinTrigger ??
                         GorillaComputer.instance.GetJoinTriggerForZone("forest")).zone,
                        Enum.Parse<GameModeType>(GorillaComputer.instance.currentGameMode.Value, true), !IsPublic,
                        SubscriptionManager.IsLocalSubscribed()),
                CustomProps = new Hashtable
                {
                        {
                                "gameMode",
                                PhotonNetworkController.Instance.currentJoinTrigger.GetFullDesiredGameModeString() ??
                                "forest|DEFAULT|Casual"
                        },
                        { "platform", PhotonNetworkController.Instance.platformTag },
                        { "queueName", GorillaComputer.instance.currentQueue },
                },
        };

        PhotonNetworkController.Instance.currentJoinType = RoomJoinType;

        NetworkSystem.Instance.ConnectToRoom(roomName, roomConfig);
    }
}