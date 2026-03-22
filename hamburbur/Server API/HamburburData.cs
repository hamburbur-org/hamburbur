using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Networking;
using Console = hamburbur.Components.Console;

namespace hamburbur.Server_API;

public class HamburburData : Singleton<HamburburData>
{
    private const string          HamburburUrl = "https://hamburbur.org";
    public static Action<JObject> OnDataReloaded;

    public static readonly Dictionary<string, string> Admins               = [];
    public static readonly List<string>               HamburburSuperAdmins = [];

    public static readonly Dictionary<string, string> SeralythAdmins      = [];
    public static readonly List<string>               SeralythSuperAdmins = [];

    private static Action<bool> onPlayerConfirmedToBeAdmin;
    private static bool         hasSubscribedToAddingAdminMods;
    private static bool         hasSubscribedToAddingSuperAdminMods;
    private static bool         givenAdminMods;

    public static          ClientWebSocket SeralythUserCountWebsocket;
    public static readonly string          SeralythServerWebsocket = "wss://menu.seralyth.software";

    private static float DataSyncDelay;
    public static  int   PlayerCount;

    private       bool    hasLoadedConsole;
    public static JObject Data       { get; private set; }
    public static bool    DataLoaded { get; private set; }

    public static bool IsLocalAdmin      { get; private set; }
    public static bool IsLocalSuperAdmin { get; private set; }

    private IEnumerator Start()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent += () => StartCoroutine(TelemetryRequest(
                                                            PhotonNetwork.CurrentRoom.Name, PhotonNetwork.NickName,
                                                            PhotonNetwork.CloudRegion, PhotonNetwork.LocalPlayer.UserId,
                                                            !PhotonNetwork.CurrentRoom.IsVisible,
                                                            PhotonNetwork.PlayerList.Length,
                                                            NetworkSystem.Instance.GameModeString));

        NetworkSystem.Instance.OnPlayerJoined += UpdatePlayerCount;
        NetworkSystem.Instance.OnPlayerLeft   += UpdatePlayerCount;

        while (true)
        {
            UnityWebRequest hamburburWebRequest = UnityWebRequest.Get(Constants.HamburburDataUrl);
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
                    Debug.LogError($"Failed to parse JSON from {Constants.HamburburDataUrl}: {e}");
                    errored = true;
                }

                if (!errored)
                {
                    bool    shouldUseSeralythData = true;
                    JObject seralythData         = null;

                    if (seralythWebRequest.result != UnityWebRequest.Result.Success)
                        shouldUseSeralythData = false;

                    if (shouldUseSeralythData)
                        try
                        {
                            seralythData = JObject.Parse(seralythWebRequest.downloadHandler.text);
                        }
                        catch
                        {
                            shouldUseSeralythData = false;
                        }

                    Admins.Clear();
                    HamburburSuperAdmins.Clear();

                    SeralythAdmins.Clear();
                    SeralythSuperAdmins.Clear();

                    foreach (JToken adminPair in (JArray)Data["admins"]!)
                    {
                        string adminUserId = adminPair["userId"]!.ToString();
                        string adminName   = adminPair["name"]!.ToString();
                        Admins[adminUserId] = adminName;
                    }

                    HamburburSuperAdmins.AddRange(((JArray)Data["superAdmins"]!).Select(token => token.ToString()));

                    if (shouldUseSeralythData)
                    {
                        foreach (JToken seralythAdminPair in (JArray)seralythData["admins"]!)
                        {
                            string seralythAdminUserId = seralythAdminPair["user-id"]!.ToString();
                            string seralythAdminName   = seralythAdminPair["name"]!.ToString();

                            Admins[seralythAdminUserId]         = seralythAdminName;
                            SeralythAdmins[seralythAdminUserId] = seralythAdminName;
                        }

                        SeralythSuperAdmins.AddRange(
                                ((JArray)seralythData["super-admins"]!).Select(token => token.ToString()));
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
                if (ServerStatusNotifications.IsEnabled)
                    NotificationManager.SendNotification(
                            "<color=red>Error</color>",
                            $"Failed to fetch necessary data from {Constants.HamburburDataUrl}, retrying in 1 minute.",
                            5f,
                            true,
                            false);

                Debug.LogError($"Failed to fetch data from {Constants.HamburburDataUrl}: {hamburburWebRequest.error}");
            }

            yield return new WaitForSeconds(60);
        }
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            if (Time.time > DataSyncDelay && PhotonNetwork.PlayerList.Length != PlayerCount)
            {
                StartCoroutine(PlayerDataSync(
                        PhotonNetwork.CurrentRoom.Name,
                        PhotonNetwork.CloudRegion
                ));

                DataSyncDelay = Time.time + 3f;
            }

            PlayerCount = PhotonNetwork.PlayerList.Length;
        }
        else
        {
            PlayerCount = -1;
        }

        if (givenAdminMods || PhotonNetwork.LocalPlayer.UserId.IsNullOrEmpty() ||
            !Admins.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string playerName))
            return;

        IsLocalSuperAdmin = HamburburSuperAdmins.Contains(playerName);
        IsLocalAdmin      = true;
        givenAdminMods    = true;
        StartCoroutine(LoadAdminModsRoutine(playerName, IsLocalSuperAdmin));
    }

    public static void UpdatePlayerCount(NetPlayer Player) =>
            PlayerCount = -1;

    public static IEnumerator TelemetryRequest(string directory, string identity,    string region, string userid,
                                               bool   isPrivate, int    playerCount, string gameMode)
    {
        UnityWebRequest request = new(HamburburUrl + "/telemetry", "POST");

        string json = JsonConvert.SerializeObject(new
        {
                directory = Tools.Utils.CleanString(directory),
                identity  = Tools.Utils.CleanString(identity),
                region    = Tools.Utils.CleanString(region, 3),
                userid    = Tools.Utils.CleanString(userid, 20),
                isPrivate,
                playerCount,
                gameMode       = Tools.Utils.CleanString(gameMode, 128),
                consoleVersion = Constants.PluginVersion,
                menuName       = Constants.PluginName,
                menuVersion    = Constants.PluginVersion,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
    }

    public static bool IsPlayerSteam(VRRig Player)
    {
        string concat           = Player._playerOwnedCosmetics.Concat();
        int    customPropsCount = Player.Creator.GetPlayerRef().CustomProperties.Count;

        return concat.Contains("S. FIRST LOGIN") || concat.Contains("FIRST LOGIN") || customPropsCount >= 2;
    }

    public static IEnumerator PlayerDataSync(string directory, string region)
    {
        DataSyncDelay = Time.time + 5f;

        yield return new WaitForSeconds(3f);

        if (!PhotonNetwork.InRoom)
            yield break;

        Dictionary<string, Dictionary<string, string>> data = new();

        foreach (Player identification in PhotonNetwork.PlayerList)
        {
            VRRig rig = identification.Rig();

            if (rig == null || identification == PhotonNetwork.LocalPlayer || Admins.ContainsKey(identification.UserId))
                continue;

            data.Add(identification.UserId,
                    new Dictionary<string, string>
                    {
                            { "nickname", Tools.Utils.CleanString(identification.NickName) },
                            { "cosmetics", rig._playerOwnedCosmetics.Concat() },
                            {
                                    "color",
                                    $"{Math.Round(rig.playerColor.r * 255)} {Math.Round(rig.playerColor.g * 255)} {Math.Round(rig.playerColor.b * 255)}"
                            },
                            { "platform", IsPlayerSteam(rig) ? "STEAM" : "QUEST" },
                    });
        }

        UnityWebRequest request = new(HamburburUrl + "/syncdata", "POST");

        string json = JsonConvert.SerializeObject(new
        {
                directory = Tools.Utils.CleanString(directory),
                region    = Tools.Utils.CleanString(region, 3),
                data,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
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
    public static bool AdminModsAccessible()
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
    public static bool SuperAdminModsAccessible()
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