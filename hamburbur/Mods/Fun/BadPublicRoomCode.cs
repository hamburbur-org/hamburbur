using System;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts;
using hamburbur.Mod_Backend;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Fun;

[hamburburmod(                "Bad Public Room Code", "Creates a public room with a not so nice work", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,   0)]
public class BadPublicRoomCode : hamburburmod
{
    private readonly string[] badBoyWords =
    [
            "FUCK",
            "SHIT",
            "COCK",
            "CRAP",
            "GOON",
            "ARSE",
            "CUNT",
            "DICK",
            "SLUT",
            "TWAT",
            "FAGG",
            "PISS",
            "KYSS",
            "FUKU",
            "OILY",
    ];

    protected override void Pressed()
    {
        string         roomName     = badBoyWords[Random.Range(0, badBoyWords.Length)];
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