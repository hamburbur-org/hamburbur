using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using hamburbur.Managers;
using hamburbur.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using WebSocketSharp;

namespace hamburbur.Server_API;

public class TrackerManager : MonoBehaviour
{
    private static JObject cachedData;

    public static    Action<JToken> OnRoomDataReceived;
    private readonly Queue<string>  receivedMessages = new();
    private readonly WebSocket      trackerWebSocket = new("wss://hamburbur.org/tracker");

    private void Start()
    {
        Debug.Log("Starting tracker websocket flow");

        trackerWebSocket.OnMessage += (sender, messageEventArgs) =>
                                      {
                                          lock (receivedMessages)
                                          {
                                              receivedMessages.Enqueue(messageEventArgs.Data);
                                          }

                                          Debug.Log("Received message from tracker websocket: " + messageEventArgs.Data);
                                      };

        trackerWebSocket.OnClose += (sender, closeEventArgs) =>
                                    {
                                        trackerWebSocket.ConnectAsync();
                                        Debug.Log("Tracker websocket closed... reconnecting");
                                    };

        trackerWebSocket.OnOpen += (sender, args) => Debug.Log("Tracker websocket Connected");

        trackerWebSocket.ConnectAsync();

        RigUtils.OnRigCosmeticsLoaded += rig =>
                                         {
                                             NetPlayer player = rig.creator;

                                             if (rig == null || player.GetPlayerRef() == PhotonNetwork.LocalPlayer ||
                                                 HamburburData.Admins.ContainsKey(player.UserId))
                                                 return;

                                             Dictionary<string, Dictionary<string, string>> data = new()
                                             {
                                                     [player.UserId] = new Dictionary<string, string>
                                                     {
                                                             {
                                                                     "nickname",
                                                                     Tools.Utils.CleanString(player.NickName)
                                                             },
                                                             {
                                                                     "cosmetics",
                                                                     rig._playerOwnedCosmetics.Concat()
                                                             },
                                                             {
                                                                     "color",
                                                                     $"{Math.Round(rig.playerColor.r * 255)} {Math.Round(rig.playerColor.g * 255)} {Math.Round(rig.playerColor.b * 255)}"
                                                             },
                                                             {
                                                                     "platform",
                                                                     rig.IsOnSteam() ? "STEAM" : "QUEST"
                                                             },
                                                     },
                                             };

                                             StartCoroutine(SendPlayerDataSync(data,
                                                     PhotonNetwork.CurrentRoom.Name,
                                                     PhotonNetwork.CloudRegion));
                                         };
    }

    private void Update()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame && PhotonNetwork.InRoom)
        {
            VRRig     rig    = VRRig.LocalRig;
            NetPlayer player = rig.creator;

            if (rig == null)
                return;

            Dictionary<string, Dictionary<string, string>> data = new()
            {
                    [player.UserId] = new Dictionary<string, string>
                    {
                            {
                                    "nickname",
                                    Tools.Utils.CleanString(player.NickName)
                            },
                            {
                                    "cosmetics",
                                    rig._playerOwnedCosmetics.Concat()
                            },
                            {
                                    "color",
                                    $"{Math.Round(rig.playerColor.r * 255)} {Math.Round(rig.playerColor.g * 255)} {Math.Round(rig.playerColor.b * 255)}"
                            },
                            {
                                    "platform",
                                    rig.IsOnSteam() ? "STEAM" : "QUEST"
                            },
                    },
            };

            StartCoroutine(SendPlayerDataSync(data,
                    PhotonNetwork.CurrentRoom.Name,
                    PhotonNetwork.CloudRegion));
        }

        lock (receivedMessages)
        {
            while (receivedMessages.Count > 0)
                ParseAndReceiveMessage(receivedMessages.Dequeue());
        }
    }

    private void ParseAndReceiveMessage(string data)
    {
        JObject trackingData = JObject.Parse(data);
        
        bool   isUserKnown = trackingData["isUserKnown"]?.ToObject<bool>() ?? false;
        string username    = trackingData["username"]?.ToString()          ?? "Someone";
        bool   hasCosmetic = !string.IsNullOrEmpty(trackingData["specialCosmetic"]?.ToString());
        string cosmetic    = trackingData["specialCosmetic"]?.ToString()    ?? "";
        string room        = trackingData["roomCode"]?.ToString()           ?? "unknown";
        int    players     = trackingData["playersInRoom"]?.ToObject<int>() ?? 0;
        string inGameName  = trackingData["inGameName"]?.ToString()         ?? "unknown";
        string gameMode    = trackingData["gameModeString"]?.ToString()     ?? "unknown";

        NotificationManager.SendNotification("<color=green>Tracker</color>",
                $"{(isUserKnown ? username : "Someone")} {(hasCosmetic ? $"with {cosmetic}" : "")} found in {(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == room ? "your code" : $"code {room}")} with {players} players. Their in game name is {inGameName} and the gamemode string is {gameMode}",
                10f, true, false);

        OnRoomDataReceived?.Invoke(trackingData);
    }

    public static IEnumerator SendPlayerDataSync(Dictionary<string, Dictionary<string, string>> data, string directory,
                                                 string                                         region)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = Tools.Utils.CleanString(directory),
                region    = Tools.Utils.CleanString(region, 3),
                data,
                playersCount = PhotonNetwork.PlayerList.Length,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new(HamburburData.HamburburUrl + "/syncdata", "POST");
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
    }
}