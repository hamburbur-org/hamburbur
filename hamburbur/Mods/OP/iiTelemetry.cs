using System;
using System.Collections;
using System.Text;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Mods.OP;

[hamburburmod(
        "ii Telemetry",
        "Uploads your data to the ii Stupid server so the checker works.",
        ButtonType.Togglable,
        AccessSetting.Public,
        EnabledType.Disabled,
        0
)]
public class iiTelemetry : hamburburmod
{
    private const string ServerEndpoint = "https://iidk.online";

    public static bool isOnline;

    protected override void OnEnable()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent += (Action)SendTelemetry;
        SendTelemetry();
    }

    protected override void OnDisable()
        => NetworkSystem.Instance.OnJoinedRoomEvent -= (Action)SendTelemetry;

    private void SendTelemetry()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null)
            CoroutineManager.Instance.StartCoroutine(TelementryRequest(
                    PhotonNetwork.CurrentRoom.Name,
                    PhotonNetwork.NickName,
                    PhotonNetwork.CloudRegion,
                    PhotonNetwork.LocalPlayer.UserId,
                    PhotonNetwork.CurrentRoom.IsVisible,
                    PhotonNetwork.PlayerList.Length,
                    NetworkSystem.Instance.GameModeString
            ));
    }

    private IEnumerator TelementryRequest(string directory, string identity,    string region, string userid,
                                          bool   isPrivate, int    playerCount, string gameMode)
    {
        UnityWebRequest request = new(ServerEndpoint + "/telemetry", "POST");

        string json = JsonConvert.SerializeObject(new
        {
                directory = CleanString(directory),
                identity  = CleanString(identity),
                region    = CleanString(region, 3),
                userid    = CleanString(userid, 20),
                isPrivate,
                playerCount,
                gameMode       = CleanString(gameMode, 128),
                consoleVersion = "3.0.5",
                menuName       = Constants.PluginName,
                menuVersion    = Constants.PluginVersion,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler   = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"[iiTelemetry] Sending telemetry to {request.url}");
        Debug.Log($"[iiTelemetry] JSON body: {json}");

        yield return request.SendWebRequest();

        Debug.Log($"[iiTelemetry] Response code: {request.responseCode}");
        Debug.Log($"[iiTelemetry] Response text: {request.downloadHandler.text}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            isOnline = true;
            NotificationManager.SendNotification(
                    "<color=green>Success</color>",
                    "Telemetry uploaded successfully",
                    5f,
                    true,
                    false);
        }
        else
        {
            isOnline = false;

            string reason = string.IsNullOrEmpty(request.error) ? "Unknown error" : request.error;
            if (!reason.Contains("429"))
                NotificationManager.SendNotification(
                        "<color=red>Error</color>",
                        $"Telemetry upload failed: {reason}",
                        5f,
                        false,
                        false);
        }
    }

    private static string CleanString(string input, int maxLength = 12)
    {
        input = new string(Array.FindAll(input.ToCharArray(), c => Utils.IsASCIILetterOrDigit(c)));

        if (input.Length > maxLength)
            input = input.Substring(0, maxLength - 1);

        return input.ToUpper();
    }

    private static string NoASCIIStringCheck(string input, int maxLength = 12)
    {
        if (input.Length > maxLength)
            input = input.Substring(0, maxLength - 1);

        return input.ToUpper();
    }
}