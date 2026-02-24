using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BepInEx;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Macros;
using hamburbur.Server_API;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Managers;

public class FileManager : Singleton<FileManager>
{
    private const string RootSoundUrl =
            "https://files.hamburbur.org/Maniacs_of_Noise.mp3";

    private const   string RootSoundFileName = "Maniacs of Noise.mp3";
    public          string SoundsFolder;
    public          string MacrosFolder;
    public          string EventLoggerFolder;
    public readonly string RootHamburburFolder = Path.Combine(Paths.GameRootPath, "hamburbur");

    public List<string> AnsweredPolls = [];

    private bool    hasLoadedSavedData;
    private float   lastTime;
    public  JObject SaveData;

    protected override void Awake()
    {
        base.Awake();
        SoundsFolder      = Path.Combine(RootHamburburFolder, "Sounds");
        MacrosFolder      = Path.Combine(RootHamburburFolder, "Macros");
        EventLoggerFolder = Path.Combine(RootHamburburFolder, "Logged Events");
    }

    private void Start()
    {
        if (!Directory.Exists(RootHamburburFolder))
            Directory.CreateDirectory(RootHamburburFolder);

        if (!Directory.Exists(SoundsFolder))
        {
            Directory.CreateDirectory(SoundsFolder);
            DownloadRootSound();
        }

        if (!Directory.Exists(MacrosFolder))
            Directory.CreateDirectory(MacrosFolder);

        if (!Directory.Exists(EventLoggerFolder))
            Directory.CreateDirectory(EventLoggerFolder);
    }

    private void Update()
    {
        if (Time.time - lastTime < 5f || !hasLoadedSavedData)
            return;

        lastTime = Time.time;

        UpdatePreferences();
    }

    public void LoadSaveData()
    {
        if (!File.Exists(Path.Combine(RootHamburburFolder, "HamburburSaveData.json")))
        {
            NotificationManager.SendNotification("<color=purple>File Manager</color>",
                    "Couldn't find a save data file, creating one", 5f, true, false);

            UpdatePreferences();
        }
        else
        {
            try
            {
                SaveData = JObject.Parse(File.ReadAllText(Path.Combine(RootHamburburFolder, "HamburburSaveData.json")));

#region Load Mod Data

                ButtonHandler.SavedModInfo = SaveData["savedModData"].ToObject<Dictionary<string, ModSaveInfo>>();

                foreach ((string _, (Type, hamburburmod)[] categoryContent) in Buttons.Categories)
                    foreach ((Type _, hamburburmod modComp) in categoryContent)
                    {
                        if (!ButtonHandler.SavedModInfo.TryGetValue(modComp.PreferencesKey,
                                    out ModSaveInfo modSaveInfo))
                            continue;

                        modComp.LoadSavedData(modSaveInfo);
                    }

#endregion

#region Check Current Poll

                HamburburData.OnDataReloaded += data =>
                                                {
                                                    JToken currentPollData = data["pollData"];
                                                    string currentPollName = currentPollData["name"].ToObject<string>();

                                                    if (AnsweredPolls.Contains(currentPollName))
                                                        return;

                                                    ButtonHandler.Instance.Prompt(new PromptData(PromptType.AcceptAndDeny, currentPollName, () => SendVoteWrapper(true, currentPollName), () => SendVoteWrapper(false, currentPollName), currentPollData["optionA"].ToObject<string>(), currentPollData["optionB"].ToObject<string>()));
                                                    NotificationManager.SendNotification("<color=purple>File Manager</color>", $"You have not voted for the poll {currentPollName}, open your menu to do so...", 5f, true, false);
                                                };
                AnsweredPolls   = SaveData["answeredPolls"].ToObject<List<string>>();
                JToken   currentPollData = HamburburData.Data["pollData"];
                string   currentPollName = currentPollData["name"].ToObject<string>();
                if (!AnsweredPolls.Contains(currentPollName))
                {
                    ButtonHandler.Instance.Prompt(new PromptData(PromptType.AcceptAndDeny, currentPollName, () => SendVoteWrapper(true, currentPollName), () => SendVoteWrapper(false, currentPollName), currentPollData["optionA"].ToObject<string>(), currentPollData["optionB"].ToObject<string>()));
                    NotificationManager.SendNotification("<color=purple>File Manager</color>", $"You have not voted for the poll {currentPollName}, open your menu to do so...", 5f, true, false);
                }

#endregion

                hasLoadedSavedData = true;
            }
            catch (Exception e)
            {
                NotificationManager.SendNotification("<color=purple>File Manager</color>", "Failed to load saved data",
                        5f,
                        true, false);

                Debug.Log(e);

                UpdatePreferences();
            }
        }

        MacroManager.LoadAllMacros();
        
        return;

        void SendVoteWrapper(bool voteForA, string currentPollName)
        {
            AnsweredPolls.Add(currentPollName);
            StartCoroutine(SendVote(voteForA));
        }
    }

    public void UpdatePreferences()
    {
        SaveData = new JObject
        {
                ["savedModData"] = GetModSaveDataJson(),
                ["answeredPolls"] = JToken.FromObject(AnsweredPolls),
        };

        File.WriteAllText(Path.Combine(RootHamburburFolder, "HamburburSaveData.json"), SaveData.ToString());
    }

    private IEnumerator SendVote(bool voteForA)
    {
        UnityWebRequest webRequest = new("https://hamburbur.org/polls/vote", "POST");
        string json = new JObject()
        {
                ["userId"] = NetworkSystem.Instance.LocalPlayer.UserId,
                ["voteForA"] = voteForA,
        }.ToString();
        byte[] body = Encoding.UTF8.GetBytes(json);

        webRequest.uploadHandler   = new UploadHandlerRaw(body);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result != UnityWebRequest.Result.Success)
            Debug.LogError("[FileManager] Failed to send vote: " + webRequest.error);
    }

    private JToken GetModSaveDataJson()
    {
        ButtonHandler.SavedModInfo = new Dictionary<string, ModSaveInfo>();

        foreach ((string _, (Type, hamburburmod)[] categoryContent) in Buttons.Categories)
            foreach ((Type _, hamburburmod modComp) in categoryContent)
                ButtonHandler.SavedModInfo[modComp.PreferencesKey] = new ModSaveInfo
                {
                        Enabled          = modComp.Enabled,
                        IncrementalValue = modComp.IncrementalValue,
                };

        if (SaveData == null)
            return JToken.FromObject(ButtonHandler.SavedModInfo);

        Dictionary<string, ModSaveInfo> oldSavedData =
                SaveData["savedModData"].ToObject<Dictionary<string, ModSaveInfo>>();

        foreach ((string preferenceKey, ModSaveInfo modSaveInfo) in oldSavedData)
            ButtonHandler.SavedModInfo.TryAdd(preferenceKey, modSaveInfo);

        return JToken.FromObject(ButtonHandler.SavedModInfo);
    }

    private void DownloadRootSound()
    {
        try
        {
            string filePath = Path.Combine(SoundsFolder, RootSoundFileName);

            WebClient client = new();
            client.DownloadFile(RootSoundUrl, filePath);

            Debug.Log($"[FileManager] Downloaded default sound to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FileManager] Failed to download default sound: {ex.Message}");
        }
    }

    public string CreateEventLoggerFile()
    {
        string fileDir = Path.Combine(EventLoggerFolder, $"{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.txt");
        File.Create(fileDir).Dispose();

        return fileDir;
    }

    public List<string> GetSoundFiles()
    {
        List<string> files = [];

        if (!Directory.Exists(SoundsFolder))
            return files;

        string[] supportedExtensions = ["*.wav", "*.ogg", "*.mp3",];

        foreach (string ext in supportedExtensions)
            files.AddRange(Directory.GetFiles(SoundsFolder, ext));

        return files;
    }
}