using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Server_API;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Cheats Detector",
        "Notifies you of when someone with a known cheat joins your code and what cheat(s) they are using",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class CheatsDetector : hamburburmod
{
    private readonly Dictionary<string, string> cheats = [];
    private          string                     resurgenceProperty;

    private IEnumerator GetResurgenceProperty()
    {
        UnityWebRequest request =
                UnityWebRequest.Get(
                        "https://raw.githubusercontent.com/Industry-Gtag/fddddddddd/refs/heads/main/CustomProperty.txt");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("nooooooo they chaanged it T-T");

            yield break;
        }

        resurgenceProperty         = request.downloadHandler.text;
        cheats[resurgenceProperty] = "Resurgence";
    }

    protected override void Start() => CoroutineManager.Instance.StartCoroutine(GetResurgenceProperty());

    protected override void OnEnable()
    {
        HamburburData.OnDataReloaded += FetchCheats;

        if (HamburburData.DataLoaded)
            FetchCheats(HamburburData.Data);

        RigUtils.OnRigLoaded += CheckForCheats;
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded         -= CheckForCheats;
        HamburburData.OnDataReloaded -= FetchCheats;
    }

    private void CheckForCheats(VRRig rig)
    {
        Hashtable properties = rig.OwningNetPlayer().GetPlayerRef().CustomProperties;
        string cheatsFormatted = cheats.Where(kvp => properties.ContainsKey(kvp.Key)).Select(kvp => kvp.Value)
                                       .Join(", ").TrimEnd(',').Trim();

        if (!cheatsFormatted.IsNullOrEmpty())
            NotificationManager.SendNotification(
                    "<color=red>Cheat Detector</color>",
                    $"Player {rig.OwningNetPlayer().SanitizedNickName} detected cheating with {cheatsFormatted}",
                    5f,
                    true,
                    false);
    }

    private void FetchCheats(JObject data)
    {
        cheats.Clear();

        foreach ((string propertyKey, string cheatName) in data["knownCheats"].ToObject<Dictionary<string, string>>())
            cheats[propertyKey] = cheatName;

        cheats[resurgenceProperty] = "Resurgence";
    }
}