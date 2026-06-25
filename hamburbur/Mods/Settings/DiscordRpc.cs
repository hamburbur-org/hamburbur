using System;
using hamburbur.GUI;
using hamburbur.Managers.DiscordRPC;
using hamburbur.Managers.DiscordRPC.Logging;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Categories;
using Photon.Pun;
using DiscordRpcAssets = hamburbur.Managers.DiscordRPC.Assets;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Discord Rich Presence", "Makes it so your discord activity says your using hamburbur",
        ButtonType.Togglable, AccessSetting.Public,    EnabledType.Enabled, 0)]
public class DiscordRpc : hamburburmod
{
    private static DiscordRpcClient discord;
    private static DateTime?        startTime;
    private static DateTime?        endTime;
    private        Action           onJoinedRoomEvent;

    private Action<NetPlayer> onPlayerJoined;
    private Action<NetPlayer> onPlayerLeft;
    private Action            onReturnedToSinglePlayer;

    protected override void OnEnable()
    {
        discord = new DiscordRpcClient("1476272482821607594")
        {
                Logger = new DiscordDebug(),
        };

        discord.Initialize();

        onPlayerJoined           = _ => UpdatePresence();
        onPlayerLeft             = _ => UpdatePresence();
        onReturnedToSinglePlayer = UpdatePresence;
        onJoinedRoomEvent        = UpdatePresence;

        if (NetworkSystem.Instance != null)
        {
            NetworkSystem.Instance.OnPlayerJoined           += onPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft             += onPlayerLeft;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += onReturnedToSinglePlayer;
            NetworkSystem.Instance.OnJoinedRoomEvent        += onJoinedRoomEvent;
        }

        UpdatePresence();
    }

    protected override void OnDisable()
    {
        if (NetworkSystem.Instance != null)
        {
            if (onPlayerJoined != null)
                NetworkSystem.Instance.OnPlayerJoined -= onPlayerJoined;

            if (onPlayerLeft != null)
                NetworkSystem.Instance.OnPlayerLeft -= onPlayerLeft;

            if (onReturnedToSinglePlayer != null)
                NetworkSystem.Instance.OnReturnedToSinglePlayer -= onReturnedToSinglePlayer;

            if (onJoinedRoomEvent != null)
                NetworkSystem.Instance.OnJoinedRoomEvent -= onJoinedRoomEvent;
        }

        discord?.ClearPresence();
        discord?.Dispose();
        discord = null;
    }

    private static void UpdatePresence()
    {
        if (discord == null || NetworkSystem.Instance == null)
            return;

        bool inRoom = NetworkSystem.Instance.InRoom;

        string roomName = string.IsNullOrEmpty(NetworkSystem.Instance.RoomName)
                                  ? "NaN"
                                  : NetworkSystem.Instance.RoomName;

        int enabledMods = Buttons.GetEnabledMods().Length;

        string gameType = "unknown";

        if (GorillaGameManager.instance != null)
            gameType = GorillaGameManager.instance.GameType().ToString().ToLower();

        int currentPlayers = PhotonNetwork.PlayerList?.Length      ?? 0;
        int maxPlayers     = PhotonNetwork.CurrentRoom?.MaxPlayers ?? 0;

        discord.SetPresence(new RichPresence
        {
                Details = $"Using Hamburbur. Enabled Mods: {enabledMods}. " + (inRoom
                                                                                       ? $"Playing {gameType}"
                                                                                       : "Playing alone"),

                State = inRoom
                                ? $"Room: {roomName} ({currentPlayers}/{maxPlayers})"
                                : "Not in a room",

                Assets = new DiscordRpcAssets
                {
                        LargeImageKey  = nameof(hamburbur),
                        LargeImageText = "hamburbur Menu",
                        SmallImageKey  = inRoom ? "online" : "offline",
                        SmallImageText = inRoom ? "Online" : "Offline",
                },

                Timestamps = inRoom
                                     ? new Timestamps
                                     {
                                             Start = startTime ?? endTime ?? DateTime.UtcNow,
                                     }
                                     : null,

                Buttons =
                [
                        new Button
                        {
                                Label = "Discord Server",
                                Url   = JoinDiscord.DiscordUrl,
                        },
                        new Button
                        {
                                Label = "Download",
                                Url   = "https://github.com/hamburbur-org/hamburbur/releases/latest/",
                        },
                ],
        });
    }
}