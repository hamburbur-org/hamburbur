using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace hamburbur.Mods.CustomMaps.GorillaMystery;

public enum MysteryTagRole
{
    Innocent,
    Sheriff,
    Murderer,
}

public static class MysteryTagState
{
    private static readonly HashSet<int> DeadPlayers = [];

    private static bool   initialized;
    private static string currentRoomName;
    private static bool   statusRequested;

    public static int? MurdererActorNumber { get; private set; }
    public static int? SheriffActorNumber  { get; private set; }
    public static bool GameActive          { get; private set; }
    public static bool VotingActive        { get; private set; }

    public static void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized                                  =  true;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
        PrepareForCurrentRoom();
    }

    public static MysteryTagRole GetRole(int actorNumber)
    {
        PrepareForCurrentRoom();

        if (MurdererActorNumber == actorNumber)
            return MysteryTagRole.Murderer;

        if (SheriffActorNumber == actorNumber)
            return MysteryTagRole.Sheriff;

        return MysteryTagRole.Innocent;
    }

    public static bool IsAlive(int actorNumber)
    {
        PrepareForCurrentRoom();

        return !DeadPlayers.Contains(actorNumber);
    }

    public static void PollGameStatus()
    {
        EnsureInitialized();
        PrepareForCurrentRoom();

        if (statusRequested || !PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
            return;

        statusRequested = true;
        MysteryTagNetwork.Send(
                MysteryTagEvents.WhatGameStatus,
                (double)PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private static void PrepareForCurrentRoom()
    {
        string roomName = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom?.Name : null;

        if (roomName == currentRoomName)
            return;

        currentRoomName = roomName;
        statusRequested = false;
        ResetRoundState();
        VotingActive = false;
    }

    private static void OnEventReceived(EventData eventData)
    {
        if (eventData.Code != MysteryTagEvents.EventCode || eventData.CustomData is not object[] content ||
            content.Length == 0                          || content[0] is not string eventName)
            return;

        PrepareForCurrentRoom();

        switch (eventName)
        {
            case MysteryTagEvents.StartGame when IsMasterSender(eventData.Sender):
                ResetRoundState();
                GameActive   = true;
                VotingActive = false;

                break;

            case MysteryTagEvents.EndGame when IsMasterSender(eventData.Sender):
                ResetRoundState();
                VotingActive = false;

                break;

            case MysteryTagEvents.ChooseMurder when IsMasterSender(eventData.Sender) &&
                                                    TryGetInt(content, 1, out int murderer):
                MurdererActorNumber = murderer;
                DeadPlayers.Remove(murderer);

                break;

            case MysteryTagEvents.ChooseSheriff when TryGetInt(content, 1, out int sheriff) &&
                                                     IsAuthorizedSheriffChoice(eventData.Sender, sheriff):
                SheriffActorNumber = sheriff;
                DeadPlayers.Remove(sheriff);

                break;

            case MysteryTagEvents.ChangeWeaponState when TryGetInt(content, 1, out int weaponType):
                InferRoleFromWeaponEvent(eventData.Sender, weaponType);

                break;

            case MysteryTagEvents.PlayerDeath when TryGetInt(content, 1, out int knifeVictim):
            case MysteryTagEvents.SheriffHit when TryGetInt(content,  1, out knifeVictim):
                DeadPlayers.Add(knifeVictim);
                if (SheriffActorNumber == knifeVictim)
                    SheriffActorNumber = null;

                break;

            case MysteryTagEvents.DropGun:
                SheriffActorNumber = null;

                break;

            case MysteryTagEvents.SheriffShot:
                if (SheriffActorNumber == null)
                    SheriffActorNumber = eventData.Sender;

                break;

            case MysteryTagEvents.PlayerVote:
            case MysteryTagEvents.StartVoting:
                if (!GameActive)
                    VotingActive = true;

                break;

            case MysteryTagEvents.GameStatus when IsMasterSender(eventData.Sender):
                ApplyGameStatus(content);

                break;
        }
    }

    private static void ApplyGameStatus(object[] content)
    {
        if (!TryGetInt(content, 1, out int gameStatus) || !TryGetInt(content, 2, out int votingStatus))
            return;

        if (content.Length            > 4     && TryGetInt(content, 4, out int target) &&
            PhotonNetwork.LocalPlayer != null && target != PhotonNetwork.LocalPlayer.ActorNumber)
            return;

        GameActive   = gameStatus == 1;
        VotingActive = !GameActive && votingStatus == 1;
    }

    private static bool IsMasterSender(int sender) =>
            PhotonNetwork.MasterClient == null || PhotonNetwork.MasterClient.ActorNumber == sender;

    private static bool IsAuthorizedSheriffChoice(int sender, int sheriff) =>
            IsMasterSender(sender) || SheriffActorNumber == null && sender == sheriff;

    private static void InferRoleFromWeaponEvent(int sender, int weaponType)
    {
        if (!GameActive)
            return;

        if (weaponType == 1 && MurdererActorNumber == null)
            MurdererActorNumber = sender;
        else if (weaponType == 2 && SheriffActorNumber == null)
            SheriffActorNumber = sender;
    }

    private static bool TryGetInt(object[] content, int index, out int value)
    {
        value = 0;

        if (index < 0 || index >= content.Length || content[index] == null)
            return false;

        try
        {
            value = Convert.ToInt32(content[index]);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void ResetRoundState()
    {
        MurdererActorNumber = null;
        SheriffActorNumber  = null;
        GameActive          = false;
        DeadPlayers.Clear();
    }
}