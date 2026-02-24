using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using hamburbur.Managers;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace hamburbur.Misc;

public class AccountBanLogger : MonoBehaviour
{
    private static AccountBanLogger instance;

    private readonly string jsonFilePath =
            Path.Combine(Path.Combine(Paths.GameRootPath, "hamburbur"), "AccountBansLog.json");

    private List<BanEntry> banEntries;

    private void Awake()
    {
        if (!Plugin.Instance.IsSteam)
            return;

        instance = this;
        LoadJson();
        StartCoroutine(UnbanCheckRoutine());
    }

    public static void AddOrUpdateCurrentAccount(string reason, DateTime? unbanTime)
    {
        if (instance == null)
        {
            GameObject gameObject = new("HamburburAccountBanLogger");
            instance = gameObject.AddComponent<AccountBanLogger>();
            DontDestroyOnLoad(gameObject);
        }

        instance.InternalAddOrUpdate(reason, unbanTime);
    }

    private void InternalAddOrUpdate(string reason, DateTime? unbanTime)
    {
        try
        {
            CSteamID  steamId  = SteamUser.GetSteamID();
            string    nickname = SteamFriends.GetPersonaName();
            DateTime? utcUnban = unbanTime?.ToUniversalTime();

            BanEntry existing = banEntries.FirstOrDefault(e => e.SteamId == steamId.m_SteamID);
            if (existing != null)
            {
                existing.Nickname       = nickname;
                existing.UnbanTimestamp = utcUnban;
                existing.Reason         = reason;
            }
            else
            {
                banEntries.Add(new BanEntry
                {
                        SteamId        = steamId.m_SteamID,
                        Nickname       = nickname,
                        UnbanTimestamp = utcUnban,
                        Reason         = reason,
                });
            }

            SaveJson();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to add/update ban entry: {ex}");
        }
    }

    private IEnumerator UnbanCheckRoutine()
    {
        while (true)
        {
            CheckUnbans();

            yield return new WaitForSeconds(30f);
        }
    }

    private void CheckUnbans()
    {
        LoadJson();

        bool     changed = false;
        DateTime now     = DateTime.UtcNow;

        for (int i = banEntries.Count - 1; i >= 0; i--)
        {
            BanEntry entry = banEntries[i];

            if (entry.UnbanTimestamp == null || entry.UnbanTimestamp > now)
                continue;

            NotificationManager.SendNotification("<color=#0746c4>Ban Manager</color>",
                    $"The account {entry.Nickname} [{entry.SteamId}] has been unbanned", 10f, true, true);

            banEntries.RemoveAt(i);
            changed = true;
        }

        if (changed)
            SaveJson();
    }

    private void LoadJson()
    {
        if (File.Exists(jsonFilePath))
            try
            {
                string json = File.ReadAllText(jsonFilePath);
                banEntries = JsonConvert.DeserializeObject<List<BanEntry>>(json) ?? new List<BanEntry>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load ban JSON: {ex}");
                banEntries = [];
            }
        else
            banEntries = [];
    }

    private void SaveJson()
    {
        try
        {
            string json = JsonConvert.SerializeObject(banEntries, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save ban JSON: {ex}");
        }
    }

    [Serializable]
    private class BanEntry
    {
        public ulong     SteamId;
        public string    Nickname;
        public string    Reason;
        public DateTime? UnbanTimestamp;
    }
}