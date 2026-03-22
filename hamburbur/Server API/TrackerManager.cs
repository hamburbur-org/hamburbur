using System;
using System.Collections.Generic;
using hamburbur.Managers;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using UnityEngine;
using WebSocketSharp;

namespace hamburbur.Server_API;

public class TrackerManager : MonoBehaviour
{
    private static   JObject       cachedData;
    private readonly Queue<string> receivedMessages = new();
    private readonly WebSocket     trackerWebSocket = new("wss://hamburbur.org/tracker");
    
    public static Action<JToken> OnRoomDataReceived;

    private void Start()
    {
        trackerWebSocket.OnMessage += (sender, messageEventArgs) =>
                                      {
                                          lock (receivedMessages)
                                          {
                                              receivedMessages.Enqueue(messageEventArgs.Data);
                                          }
                                      };

        trackerWebSocket.OnClose += (sender, closeEventArgs) => trackerWebSocket.ConnectAsync();
        trackerWebSocket.ConnectAsync();
    }

    private void Update()
    {
        lock (receivedMessages)
        {
            while (receivedMessages.Count > 0)
                ParseAndReceiveMessage(receivedMessages.Dequeue());
        }
    }

    private void ParseAndReceiveMessage(string data)
    {
        JObject trackingData = JObject.Parse(data);
        
        NotificationManager.SendNotification("<color=green>Tracker</color>",
                $"{(trackingData["isUserKnown"].ToObject<bool>() ? trackingData["username"].ToObject<string>() : "Someone")} {(trackingData["hasSpecialCosmetic"].ToObject<bool>() ? $"with {trackingData["specialCosmetic"].ToObject<string>()}" : "")} found in {(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == trackingData["roomCode"].ToObject<string>() ? "your code" : $"code {trackingData["roomCode"].ToObject<string>()}")} with {trackingData["playersInRoom"].ToObject<int>()} players. Their in game name is {trackingData["inGameName"].ToObject<string>()} and the gamemode string is {trackingData["gameModeString"].ToObject<string>()}",
                10f, true, false);
        
        OnRoomDataReceived?.Invoke(trackingData);
    }
}