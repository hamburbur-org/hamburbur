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
    private static float            updateTime;

    protected override void OnEnable()
    {
        NetworkSystem.Instance.OnPlayerJoined           += _ => UpdatePresence();
        NetworkSystem.Instance.OnPlayerLeft             += _ => UpdatePresence();
        NetworkSystem.Instance.OnReturnedToSinglePlayer += UpdatePresence;
        NetworkSystem.Instance.OnJoinedRoomEvent        += UpdatePresence;

        discord = new DiscordRpcClient("1476272482821607594")
        {
                Logger = new DiscordDebug(),
        };

        discord.Initialize();
    }

    protected override void OnDisable()
    {
        NetworkSystem.Instance.OnPlayerJoined           -= _ => UpdatePresence();
        NetworkSystem.Instance.OnPlayerLeft             -= _ => UpdatePresence();
        NetworkSystem.Instance.OnReturnedToSinglePlayer -= UpdatePresence;
        NetworkSystem.Instance.OnJoinedRoomEvent        -= UpdatePresence;

        discord.ClearPresence();
        discord.Dispose();
        discord = null;
    }

    private void UpdatePresence()
    {
        bool   inRoom      = NetworkSystem.Instance.InRoom;
        string roomName    = NetworkSystem.Instance.RoomName ?? "NaN";
        int    enabledMods = Buttons.GetEnabledMods().Length;

        discord.SetPresence(new RichPresence
        {
                Details = $"Using Hamburbur. Enabled Mods: {enabledMods}. " + (inRoom
                                                                                       ? $"Playing {GorillaGameManager.instance.GameType().ToString().ToLower()}"
                                                                                       : "Playing alone"),
                State = inRoom
                                ? $"Room: {roomName} ({PhotonNetwork.PlayerList.Length}/{PhotonNetwork.CurrentRoom.MaxPlayers})"
                                : "Not in a room",
                Assets = new DiscordRpcAssets
                {
                        LargeImageKey  = "hamburbur",
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