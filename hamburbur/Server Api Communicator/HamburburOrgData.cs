using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using Console = hamburbur.Components.Console;

namespace hamburbur.Server_Api_Communicator;

public class HamburburOrgData : Singleton<HamburburOrgData>
{
    public static Action<JObject> OnDataReloaded;

    private static readonly Dictionary<string, string>         allAdmins         = [];
    private static readonly ReadOnlyDictionary<string, string> allAdminsReadonly = new(allAdmins);

    private static readonly Dictionary<string, string>         hamburburAdmins         = [];
    private static readonly ReadOnlyDictionary<string, string> hamburburAdminsReadonly = new(hamburburAdmins);

    private static readonly HashSet<string> hamburburSuperAdmins = [];

    private static readonly Dictionary<string, string>         seralythAdmins         = [];
    private static readonly ReadOnlyDictionary<string, string> seralythAdminsReadonly = new(seralythAdmins);

    private static readonly HashSet<string> seralythSuperAdmins = [];

    private static Action<bool> onPlayerConfirmedToBeAdmin;
    private static bool         hasSubscribedToAddingAdminMods;
    private static bool         hasSubscribedToAddingSuperAdminMods;
    private static bool         givenAdminMods;

    public static bool ShouldUseSeralythData;

    public static          ClientWebSocket SeralythUserCountWebsocket;
    public static readonly string          SeralythServerWebsocket = "wss://menu.seralyth.software";
    
    private       bool                                hasLoadedConsole;
    public static IReadOnlyDictionary<string, string> AllAdmins            => allAdminsReadonly;
    public static IReadOnlyDictionary<string, string> HamburburAdmins      => hamburburAdminsReadonly;
    public static IReadOnlyCollection<string>         HamburburSuperAdmins => hamburburSuperAdmins;
    public static IReadOnlyDictionary<string, string> SeralythAdmins       => seralythAdminsReadonly;
    public static IReadOnlyCollection<string>         SeralythSuperAdmins  => seralythSuperAdmins;
    public static JObject                             Data                 { get; private set; }
    public static bool                                DataLoaded           { get; private set; }

    public static bool IsLocalAdmin      { get; private set; }
    public static bool IsLocalSuperAdmin { get; private set; }

    private IEnumerator Start()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent += () => StartCoroutine(TelemetryRequest(
                                                            PhotonNetwork.CurrentRoom.Name, PhotonNetwork.NickName,
                                                            PhotonNetwork.CloudRegion, PhotonNetwork.LocalPlayer.UserId,
                                                            PhotonNetwork.CurrentRoom.IsVisible,
                                                            PhotonNetwork.PlayerList.Length,
                                                            NetworkSystem.Instance.GameModeString));

        while (true)
        {
            UnityWebRequest hamburburWebRequest = UnityWebRequest.Get(Constants.HamburburUrl + "/data");
            UnityWebRequest seralythWebRequest  = UnityWebRequest.Get("https://menu.seralyth.software/serverdata");

            Task.Run(async () =>
                     {
                         SeralythUserCountWebsocket ??= new ClientWebSocket();
                         await SeralythUserCountWebsocket.ConnectAsync(
                                 new Uri($"{SeralythServerWebsocket}?mod={Constants.PluginName}"),
                                 CancellationToken.None
                         );
                     });

            yield return hamburburWebRequest.SendWebRequest();
            yield return seralythWebRequest.SendWebRequest();

            if (hamburburWebRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = hamburburWebRequest.downloadHandler.text;
                bool   errored      = false;

                try
                {
                    Data       = JObject.Parse(jsonResponse);
                    DataLoaded = true;
                    try
                    {
                        OnDataReloaded?.Invoke(Data);
                    }
                    catch
                    {
                        // Ignored
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON from {Constants.HamburburUrl}/data: {e}");
                    errored = true;
                }

                if (!errored)
                {
                    ShouldUseSeralythData = true;
                    JObject seralythData = null;

                    if (seralythWebRequest.result != UnityWebRequest.Result.Success)
                        ShouldUseSeralythData = false;

                    if (ShouldUseSeralythData)
                        try
                        {
                            seralythData = JObject.Parse(seralythWebRequest.downloadHandler.text);
                        }
                        catch
                        {
                            ShouldUseSeralythData = false;
                        }

                    allAdmins.Clear();

                    hamburburAdmins.Clear();
                    hamburburSuperAdmins.Clear();

                    seralythAdmins.Clear();
                    seralythSuperAdmins.Clear();

                    foreach (JToken adminPair in (JArray)Data["admins"]!)
                    {
                        string adminUserId = adminPair["userId"]!.ToString();
                        string adminName   = adminPair[nameof(name)]!.ToString();

                        allAdmins[adminUserId]       = adminName;
                        hamburburAdmins[adminUserId] = adminName;
                    }

                    foreach (string superAdmin in ((JArray)Data["superAdmins"]!).Select(token => token.ToString()))
                        hamburburSuperAdmins.Add(superAdmin);

                    if (ShouldUseSeralythData)
                    {
                        foreach (JToken seralythAdminPair in (JArray)seralythData["admins"]!)
                        {
                            string seralythAdminUserId = seralythAdminPair["user-id"]!.ToString();
                            string seralythAdminName   = seralythAdminPair[nameof(name)]!.ToString();

                            allAdmins[seralythAdminUserId]      = seralythAdminName;
                            seralythAdmins[seralythAdminUserId] = seralythAdminName;
                        }

                        foreach (string superAdmin in
                                 ((JArray)seralythData["super-admins"]!).Select(token => token.ToString()))
                            seralythSuperAdmins.Add(superAdmin);
                    }

                    if (!hasLoadedConsole)
                    {
                        Console.LoadConsole();
                        hasLoadedConsole = true;
                    }
                }
            }
            else
            {
                NotificationManager.SendNotification(
                        "<color=red>Error</color>",
                        $"Failed to fetch necessary data from {Constants.HamburburUrl}/data: {hamburburWebRequest.error}",
                        5f,
                        true,
                        true);

                Debug.LogError($"Failed to fetch data from {Constants.HamburburUrl}/data: {hamburburWebRequest.error}");
            }

            yield return new WaitForSeconds(120);
        }
    }

    private void Update()
    {
        if (givenAdminMods || PhotonNetwork.LocalPlayer.UserId.IsNullOrEmpty() ||
            !allAdmins.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string playerName))
            return;

        IsLocalSuperAdmin = hamburburSuperAdmins.Contains(playerName);
        IsLocalAdmin      = true;
        givenAdminMods    = true;
        StartCoroutine(LoadAdminModsRoutine(playerName, IsLocalSuperAdmin));
    }

    private static IEnumerator TelemetryRequest(string directory, string identity,    string region, string userid,
                                                bool   isPrivate, int    playerCount, string gameMode)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = Tools.Utils.CleanString(directory, 12),
                identity  = Tools.Utils.CleanString(identity, 12),
                region    = Tools.Utils.CleanString(region, 3),
                userid    = Tools.Utils.CleanString(userid, 20),
                isPrivate,
                playerCount,
                gameMode       = Tools.Utils.CleanString(gameMode, 128),
                consoleVersion = "NaN",
                menuName       = Constants.PluginName,
                menuVersion    = Constants.PluginVersion,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest hamburburRequest = new(Constants.HamburburUrl + "/telemetry", "POST");
        hamburburRequest.uploadHandler = new UploadHandlerRaw(raw);
        hamburburRequest.SetRequestHeader("Content-Type", "application/json");
        hamburburRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return hamburburRequest.SendWebRequest();

        UnityWebRequest seralythRequest = new(Constants.SeralythUrl + "/telemetry", "POST");
        seralythRequest.uploadHandler = new UploadHandlerRaw(raw);
        seralythRequest.SetRequestHeader("Content-Type", "application/json");
        seralythRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return seralythRequest.SendWebRequest();
    }

    private IEnumerator LoadAdminModsRoutine(string playerName, bool superAdmin)
    {
        while (!ButtonHandler.InaccessibleButtons.ContainsKey(AccessSetting.AdminOnly))
            yield return null;

        yield return new WaitForSeconds(3f);
        NotificationManager.SendNotification(
                "<color=purple>Console</color>",
                $"Hello {playerName}! Admin {(superAdmin ? "(and super admin!) " : "")}mods have been added.",
                5f,
                true,
                false);

        if (superAdmin)
            Console.IsBlocked = 0L;

        givenAdminMods    = true;
        IsLocalAdmin      = true;
        IsLocalSuperAdmin = superAdmin;
        onPlayerConfirmedToBeAdmin?.Invoke(superAdmin);
    }

    [AccessSettingsAllowedCheck(AccessSetting.AdminOnly)]
    public static bool CanAccessAdminModButtons()
    {
        if (!givenAdminMods)
        {
            if (hasSubscribedToAddingAdminMods)
                return false;

            onPlayerConfirmedToBeAdmin     += AddAdminMods;
            hasSubscribedToAddingAdminMods =  true;

            return false;
        }

        if (!IsLocalAdmin)
            return false;

        if (ButtonHandler.InaccessibleButtons.ContainsKey(AccessSetting.AdminOnly))
            AddAdminMods(IsLocalSuperAdmin);

        return true;

        void AddAdminMods(bool superAdmin) // superAdmin bool is disregarded in this one
        {
            foreach ((string category, Type mod) in ButtonHandler.InaccessibleButtons[AccessSetting.AdminOnly])
                ButtonHandler.AddButton(category, mod);

            ButtonHandler.InaccessibleButtons.Remove(AccessSetting.AdminOnly);
        }
    }

    [AccessSettingsAllowedCheck(AccessSetting.SuperAdminOnly)]
    public static bool CanAccessSuperAdminModButtons()
    {
        if (!givenAdminMods)
        {
            if (hasSubscribedToAddingSuperAdminMods)
                return false;

            onPlayerConfirmedToBeAdmin          += AddSuperAdminMods;
            hasSubscribedToAddingSuperAdminMods =  true;

            return false;
        }

        if (!IsLocalSuperAdmin)
            return false;

        if (ButtonHandler.InaccessibleButtons.ContainsKey(AccessSetting.SuperAdminOnly))
            AddSuperAdminMods(IsLocalSuperAdmin);

        return true;

        void AddSuperAdminMods(bool superAdmin)
        {
            if (!superAdmin)
                return;

            foreach ((string category, Type mod) in ButtonHandler.InaccessibleButtons[AccessSetting.SuperAdminOnly])
                ButtonHandler.AddButton(category, mod);

            ButtonHandler.InaccessibleButtons.Remove(AccessSetting.SuperAdminOnly);
        }
    }
}