using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace hamburbur.Tools;

public class GorillaFriendsUtils : MonoBehaviour
{
    public enum eRecentlyPlayed : byte
    {
        Never  = 0,
        Before = 1,
        Now    = 2,
    }
    
    internal static string s_clrFriend;
    internal static string s_clrVerified;
    internal static string s_clrPlayedRecently;

    internal static byte  moreTimeIfWeLagging      = 5;
    internal static int   howMuchSecondsIsRecently = 259200;
    public static   Color m_clrFriend         { get; internal set; } = new(0.8f, 0.5f, 0.9f, 1.0f);
    public static   Color m_clrVerified       { get; internal set; } = new(0.5f, 1.0f, 0.5f, 1.0f);
    public static   Color m_clrPlayedRecently { get; internal set; } = new(1.0f, 0.67f, 0.67f, 1.0f);
    
    internal static HashSet<string> m_listVerifiedUserIds = [];
    internal static HashSet<string> m_listCurrentSessionFriends = [];
    internal static HashSet<string> m_listCurrentSessionRecentlyChecked = [];
    internal static Dictionary<string, (eRecentlyPlayed recentlyPlayed, float value)> m_listRecentPlayCache = [];

    private void Awake()
    {
        ConfigFile cfg = new(Path.Combine(Paths.ConfigPath, "GorillaFriends.cfg"), true);
        moreTimeIfWeLagging = cfg.Bind("Timings", "MoreTimeOnLag", (byte)5,
                "This is a little settings for us in case our game froze for a second or more").Value;

        howMuchSecondsIsRecently = cfg.Bind("Timings", "RecentlySeconds", 259200, "How much is \"recently\"?").Value;
        if (howMuchSecondsIsRecently < moreTimeIfWeLagging) howMuchSecondsIsRecently = moreTimeIfWeLagging;
        m_clrPlayedRecently = cfg.Bind("Colors", "RecentlyPlayedWith", m_clrPlayedRecently,
                "Color of \"Recently played with ...\"").Value;

        m_clrFriend = cfg.Bind("Colors", "Friend", m_clrFriend, "Color of FRIEND!").Value;

        byte[] clrizer = [(byte)(m_clrFriend.r * 255), (byte)(m_clrFriend.g * 255), (byte)(m_clrFriend.b * 255),];
        s_clrFriend = "<color=#" + ByteArrayToHexCode(clrizer) + ">";

        clrizer[0]    = (byte)(m_clrVerified.r * 255);
        clrizer[1]    = (byte)(m_clrVerified.g * 255);
        clrizer[2]    = (byte)(m_clrVerified.b * 255);
        s_clrVerified = "<color=#" + ByteArrayToHexCode(clrizer) + ">";

        clrizer[0]          = (byte)(m_clrPlayedRecently.r * 255);
        clrizer[1]          = (byte)(m_clrPlayedRecently.g * 255);
        clrizer[2]          = (byte)(m_clrPlayedRecently.b * 255);
        s_clrPlayedRecently = "<color=#" + ByteArrayToHexCode(clrizer) + ">";
    }

    private static string ByteArrayToHexCode(byte[] arr)
    {
        StringBuilder hex = new(arr.Length * 2);
        foreach (byte b in arr)
            hex.AppendFormat("{0:X2}", b);

        return hex.ToString();
    }

    public static bool IsVerified(string userId) => m_listVerifiedUserIds.Contains(userId);

    public static bool IsFriend(string userId) => PlayerPrefs.GetInt(userId + "_friend", 0) != 0;

    public static bool IsInFriendList(string userId) => m_listCurrentSessionFriends.Contains(userId);

    public static void AddFriend(string userId)
    {
        PlayerPrefs.SetInt(string.Concat(userId, "_friend"), 1);
        PlayerPrefs.Save();

        if (!NetworkSystem.Instance.InRoom) 
            return;

        NetPlayer player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);

        if (player == null || player.IsNull || !m_listCurrentSessionFriends.Add(userId)) 
            return;

        if (VRRigCache.Instance.TryGetVrrig(player.ActorNumber, out RigContainer playerRig)) 
            playerRig.Rig.UpdateName();
    }

    public static void RemoveFriend(string userId)
    {
        PlayerPrefs.DeleteKey(string.Concat(userId, "_friend"));
        PlayerPrefs.Save();

        if (!NetworkSystem.Instance.InRoom) 
            return;

        NetPlayer player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);

        if (player == null || player.IsNull || !m_listCurrentSessionFriends.Remove(userId))
            return;

        if (VRRigCache.Instance.TryGetVrrig(player.ActorNumber, out RigContainer playerRig)) 
            playerRig.Rig.UpdateName();
    }

    public static bool NeedToCheckRecently(string userId) => !m_listCurrentSessionRecentlyChecked.Contains(userId);

    public static (eRecentlyPlayed recentlyPlayed, float value) HasPlayedWithUsRecently(string userId)
    {
        if (m_listRecentPlayCache.TryGetValue(userId, out (eRecentlyPlayed recentlyPlayed, float value) cache))
            return cache;

        string key = string.Concat("pd_", userId);

        if (!long.TryParse(PlayerPrefs.GetString(key, "0"), NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out long lastPlayedTime) || lastPlayedTime == 0) return (eRecentlyPlayed.Never, 0);

        long currentTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

        if (lastPlayedTime > currentTime - moreTimeIfWeLagging && lastPlayedTime <= currentTime)
            return (eRecentlyPlayed.Now, 1);

        return lastPlayedTime + howMuchSecondsIsRecently > currentTime
                       ? (eRecentlyPlayed.Before,
                          1f - Mathf.InverseLerp(lastPlayedTime, lastPlayedTime + howMuchSecondsIsRecently,
                                  currentTime))
                       : (eRecentlyPlayed.Never, 0);
    }
}