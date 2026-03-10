using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using Console = hamburbur.Components.Console;

namespace hamburbur.Server_API;

public class HamburburData : Singleton<HamburburData>
{
    public static Action<JObject> OnDataReloaded;

    public static readonly Dictionary<string, string> Admins               = [];
    public static readonly List<string>               HamburburSuperAdmins = [];
    
    public static readonly Dictionary<string, string> SeralythAdmins               = [];
    public static readonly List<string>               SeralythSuperAdmins = [];

    private static Action<bool> onPlayerConfirmedToBeAdmin;
    private static bool         hasSubscribedToAddingAdminMods;
    private static bool         hasSubscribedToAddingSuperAdminMods;
    private static bool         givenAdminMods;

    private       bool    hasLoadedConsole;
    public static JObject Data       { get; private set; }
    public static bool    DataLoaded { get; private set; }

    public static bool IsLocalAdmin      { get; private set; }
    public static bool IsLocalSuperAdmin { get; private set; }

    private IEnumerator Start()
    {
        while (true)
        {
            UnityWebRequest hamburburWebRequest = UnityWebRequest.Get(Constants.HamburburDataUrl);
            UnityWebRequest stupidWebRequest    = UnityWebRequest.Get("https://menu.seralyth.software/serverdata");


            yield return hamburburWebRequest.SendWebRequest();
            yield return stupidWebRequest.SendWebRequest();

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
                        // ignored
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
                    JObject seryalythData         = null;
                    
                    if (stupidWebRequest.result != UnityWebRequest.Result.Success)
                        shouldUseSeralythData = false;
                    
                    if (shouldUseSeralythData)
                        try
                        {
                            seryalythData = JObject.Parse(stupidWebRequest.downloadHandler.text);
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
                        foreach (JToken seralythAdminPair in (JArray)seryalythData["admins"]!)
                        {
                            string seralythAdminUserId = seralythAdminPair["user-id"]!.ToString();
                            string seralythAdminName   = seralythAdminPair["name"]!.ToString();
                            
                            Admins[seralythAdminUserId] = seralythAdminName;
                            SeralythAdmins[seralythAdminUserId] = seralythAdminName;
                        }
                        
                        SeralythSuperAdmins.AddRange(((JArray)seryalythData["super-admins"]!).Select(token => token.ToString()));
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
        if (givenAdminMods || PhotonNetwork.LocalPlayer.UserId.IsNullOrEmpty() ||
            !Admins.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string playerName))
            return;

        IsLocalSuperAdmin = HamburburSuperAdmins.Contains(playerName);

        IsLocalAdmin   = true;
        givenAdminMods = true;
        StartCoroutine(LoadAdminModsRoutine(playerName, IsLocalSuperAdmin));
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