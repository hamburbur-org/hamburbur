using System;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod("Auto Get Console Users", "Automatically detects who is using console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class AutoGetConsoleUsers : hamburburmod
{
    public static AutoGetConsoleUsers Instance { get; private set; }

    protected override void Start()
    {
        Instance = this;

        NetworkSystem.Instance.OnPlayerJoined += (Action<NetPlayer>)PingForConsole;
        NetworkSystem.Instance.OnJoinedRoomEvent += () =>
                                                    {
                                                        foreach (NetPlayer player in NetworkSystem.Instance
                                                                        .PlayerListOthers)
                                                            PingForConsole(player);
                                                    };
    }

    protected override void OnEnable()
    {
        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
            PingForConsole(player);
    }

    public void PingForConsole(NetPlayer player)
    {
        if (player.IsLocal)
            return;

        Components.Console.IndicatorDelay = Time.time + 2f;
        Components.Console.ExecuteCommand("isusing", player.ActorNumber);
    }
}