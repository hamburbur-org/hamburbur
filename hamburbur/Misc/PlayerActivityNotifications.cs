using System;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Misc;

public class PlayerActivityNotifications : MonoBehaviour
{
    private string roomCode;

    private void Start()
    {
        NetworkSystem.Instance.OnPlayerJoined += (Action<NetPlayer>)OnPlayerJoined;
        NetworkSystem.Instance.OnPlayerLeft   += (Action<NetPlayer>)OnPlayerLeft;

        NetworkSystem.Instance.OnJoinedRoomEvent        += (Action)OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action)OnLeftRoom;
    }

    private void OnPlayerJoined(NetPlayer player)
    {
        if (player.IsLocal || !RoomNotifications.Instance.Enabled)
            return;

        string userId = player.UserId;

        if (GorillaFriends.Main.IsFriend(userId))
            NotificationManager.SendNotification("<color=#1b0d4f>GorillaFriends</color>",
                    $"<color=#{ColorUtility.ToHtmlStringRGB(GorillaFriends.Main.m_clrFriend)}>Friend</color> {player.NickName} has joined your code",
                    8f, true, false);
        else if (GorillaFriends.Main.IsVerified(userId))
            NotificationManager.SendNotification(
                    "<color=#1b0d4f>GorillaFriends</color>",
                    $"<color=#{ColorUtility.ToHtmlStringRGB(GorillaFriends.Main.m_clrVerified)}>Verified</color> player {player.NickName} has joined your code",
                    8f,
                    true,
                    false);
        else if (GorillaFriends.Main.HasPlayedWithUsRecently(userId).recentlyPlayed !=
                 GorillaFriends.Main.eRecentlyPlayed.Never)
            NotificationManager.SendNotification(
                    "<color=#1b0d4f>GorillaFriends</color>",
                    $"<color=#{ColorUtility.ToHtmlStringRGB(GorillaFriends.Main.m_clrPlayedRecently)}>Recently played with</color> player {player.NickName} has joined your code",
                    8f,
                    false,
                    false);
        else
            NotificationManager.SendNotification(
                    "<color=yellow>Room Activity</color>",
                    $"{player.NickName} has joined your code",
                    8f,
                    false,
                    false);
    }

    private void OnPlayerLeft(NetPlayer player)
    {
        if (player.IsLocal || !RoomNotifications.Instance.Enabled)
            return;

        string userId = player.UserId;

        if (GorillaFriends.Main.IsFriend(userId))
            NotificationManager.SendNotification(
                    "<color=#1b0d4f>GorillaFriends</color>",
                    $"<color=#{ColorUtility.ToHtmlStringRGB(GorillaFriends.Main.m_clrFriend)}>Friend</color> {player.NickName} has left your code",
                    8f,
                    true,
                    false);

        else if (GorillaFriends.Main.IsVerified(userId))
            NotificationManager.SendNotification(
                    "<color=#1b0d4f>GorillaFriends</color>",
                    $"<color=#{ColorUtility.ToHtmlStringRGB(GorillaFriends.Main.m_clrVerified)}>Verified</color> player {player.NickName} has left your code",
                    8f,
                    true,
                    false);

        else
            NotificationManager.SendNotification(
                    "<color=yellow>Room Activity</color>",
                    $"{player.NickName} has left your code",
                    8f,
                    false,
                    false);
    }

    private void OnJoinedRoom()
    {
        if (!RoomNotifications.Instance.Enabled)
            return;

        roomCode = NetworkSystem.Instance.RoomName;
        NotificationManager.SendNotification(
                "<color=yellow>Room Activity</color>",
                $"You have joined the {(Tools.Utils.IsModdedRoom ? "modded " : "")}room {roomCode}",
                8f,
                false,
                false);
    }

    private void OnLeftRoom()
    {
        if (!RoomNotifications.Instance.Enabled)
            return;

        NotificationManager.SendNotification(
                "<color=yellow>Room Activity</color>",
                $"You have left the room {roomCode}",
                8f,
                false,
                false);
    }
}