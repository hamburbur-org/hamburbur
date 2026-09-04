using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace hamburbur.Mods.CustomMaps.MecchaGorilla;

public enum MecchaRole
{
    Hider,
    Seeker,
}

public static class MecchaState
{
    private static readonly Dictionary<int, MecchaRole> Roles = [];
    private static readonly HashSet<int> Dead = [];
    private static bool initialized;
    private static string roomName;

    public static int RoundState { get; private set; } = 1;
    public static int RoundMap { get; private set; }

    public static void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized = true;
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        ResetForRoom();
    }

    public static MecchaRole GetRole(int actorNumber)
    {
        PrepareRoom();
        return Roles.GetValueOrDefault(actorNumber, MecchaRole.Hider);
    }

    public static bool IsAlive(int actorNumber)
    {
        PrepareRoom();
        return !Dead.Contains(actorNumber);
    }

    private static void PrepareRoom()
    {
        string current = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom?.Name : null;
        if (current == roomName)
            return;

        ResetForRoom();
    }

    private static void ResetForRoom()
    {
        roomName = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom?.Name : null;
        Roles.Clear();
        Dead.Clear();
        RoundState = 1;
        RoundMap = 0;
    }

    private static void OnEvent(EventData eventData)
    {
        if (eventData.Code != CustomMapUtils.EventCode || eventData.CustomData is not object[] content ||
            content.Length == 0                   || content[0] is not string name)
            return;

        PrepareRoom();
        switch (name)
        {
            case MecchaEvents.Role when Int(content, 1, out int player):
                Roles[player] = Int(content, 2, out int flag) && flag == 1 ? MecchaRole.Seeker : MecchaRole.Hider;
                break;
            case MecchaEvents.Seekers:
                Roles.Clear();
                for (int i = 1; i < content.Length; i++)
                    if (Int(content, i, out int seeker) && seeker > 0)
                        Roles[seeker] = MecchaRole.Seeker;
                break;
            case MecchaEvents.Kill when Int(content, 1, out int victim):
            case MecchaEvents.Leave when Int(content, 1, out victim):
                Dead.Add(victim);
                break;
            case MecchaEvents.Respawn when Int(content, 1, out int respawned):
                Dead.Remove(respawned);
                break;
            case MecchaEvents.StartRound when Int(content, 1, out int startedMap):
                RoundState = 2;
                RoundMap = startedMap;
                Dead.Clear();
                break;
            case MecchaEvents.RoundState when Int(content, 1, out int state):
                RoundState = state;
                if (Int(content, 3, out int map)) RoundMap = map;
                if (state is 1 or 2) Dead.Clear();
                break;
        }
    }

    private static bool Int(object[] content, int index, out int value)
    {
        value = 0;
        if (index < 0 || index >= content.Length || content[index] == null)
            return false;
        try { value = Convert.ToInt32(content[index]); return true; }
        catch { return false; }
    }
}
