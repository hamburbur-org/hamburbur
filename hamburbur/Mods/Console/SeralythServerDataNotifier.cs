using System.Collections;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Server_API;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Mods.Console;

[hamburburmod("Seralyth Server Data Notifier", "Checks official console server data status", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class SeralythServerDataNotifier : hamburburmod
{
    private       bool?  lastUp;
    private       bool   isRunning;
    private Coroutine checkRoutine;

    protected override void OnEnable()
    {
        /*if (!HamburburData.SeralythAdmins.ContainsKey(PhotonNetwork.LocalPlayer.UserId) && HamburburData.shouldUseSeralythData)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You're not a seralyth console admin!", 5f, true, true);
            Toggle(ButtonState.Normal, false);
            return;
        }*/

        if (isRunning)
            return;

        isRunning    = true;
        checkRoutine = CoroutineManager.Instance.StartCoroutine(CheckRoutine());
    }

    protected override void OnDisable()
    {
        isRunning = false;
        
        if (checkRoutine != null)
            CoroutineManager.Instance.StopCoroutine(checkRoutine);
        
        lastUp    = !lastUp;
    }

    private IEnumerator CheckRoutine()
    {
        yield return CheckStatus();

        while (isRunning)
        {
            yield return new WaitForSeconds(30f);
            yield return CheckStatus();
        }
    }

    private IEnumerator CheckStatus()
    {
        using UnityWebRequest req = UnityWebRequest.Get(HamburburData.SeralythUrl + "/serverdata");
        yield return req.SendWebRequest();

        bool   isUp        = req.result == UnityWebRequest.Result.Success;
        string failureInfo = isUp ? "" : req.error;

        if (lastUp == isUp)
            yield break;

        lastUp = isUp;
        SendNotification(isUp, failureInfo);
    }

    private void SendNotification(bool isUp, string failureInfo)
    {
        if (isUp)
            NotificationManager.SendNotification("<color=green>Server Data</color>", "Seralyth Console Server Data is online", 5f, true, true);
        else
            NotificationManager.SendNotification("<color=red>Server Data</color>", $"Seralyth Console Server Data is offline, {failureInfo}", 5f, true, true);
    }
}