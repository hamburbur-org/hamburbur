#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using hamburbur.Components;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

namespace hamburbur.Server_Api_Communicator;

public class MenuWebsocket : Singleton<MenuWebsocket>
{
    private const float PingDelay      = 10f;
    private const float ReconnectDelay = 5f;
    private const float ConnectTimeout = 10f;

    public static Action<string>  OnMessageReceived;
    public static Action<JObject> OnJsonReceived;

    private readonly Queue<string> receivedMessagesQueue = new();
    private readonly Queue<string> statusQueue           = new();
    private          bool          isClosing;

    private Coroutine? socketCoroutine;
    private WebSocket? MainWebSocket;

    private IEnumerator Start()
    {
        socketCoroutine = StartCoroutine(WebSocketLoop());

        yield break;
    }

    private void Update()
    {
        lock (statusQueue)
        {
            while (statusQueue.Count > 0)
            {
                string message = statusQueue.Dequeue();

                Debug.Log($"[Hamburbur WebSocket] {message}");

                if (ServerStatusNotifications.IsEnabled)
                    NotificationManager.SendNotification(
                            "<color=green>Hamburbur Server</color>",
                            message,
                            5f,
                            false,
                            false);
            }
        }

        lock (receivedMessagesQueue)
        {
            while (receivedMessagesQueue.Count > 0)
            {
                string message = receivedMessagesQueue.Dequeue();
                ParseAndReceiveMessage(message);
            }
        }
    }

    private void OnDestroy()
    {
        isClosing = true;

        if (socketCoroutine != null)
            StopCoroutine(socketCoroutine);

        CloseSocket();
    }

    private IEnumerator WebSocketLoop()
    {
        WaitForSeconds pingWait      = new(PingDelay);
        WaitForSeconds reconnectWait = new(ReconnectDelay);

        while (!isClosing)
        {
            if (MainWebSocket is not { IsAlive: true, })
            {
                Connect();

                float timeout = Time.time + ConnectTimeout;

                while (!isClosing                           &&
                       MainWebSocket is { IsAlive: false, } &&
                       Time.time < timeout)
                    yield return null;

                if (MainWebSocket is not { IsAlive: true, })
                {
                    CloseSocket();

                    yield return reconnectWait;

                    continue;
                }
            }

            bool pingFailed = false;

            try
            {
                MainWebSocket.Send("ping");
            }
            catch (Exception e)
            {
                QueueStatus($"Ping failed: {e.Message}");
                CloseSocket();
                pingFailed = true;
            }

            if (pingFailed)
            {
                yield return reconnectWait;

                continue;
            }

            yield return pingWait;
        }
    }

    private void Connect()
    {
        CloseSocket();

        string uri = $"{Constants.WebSocketUri}?modname={Uri.EscapeDataString(Constants.PluginName)}";

        MainWebSocket = new WebSocket(uri);

        MainWebSocket.OnOpen += (_, _) => { QueueStatus("Connected"); };

        MainWebSocket.OnClose += (_, e) =>
                                 {
                                     QueueStatus($"Disconnected, reconnecting... Reason for disconnect \"{e.Reason}\"");
                                 };

        MainWebSocket.OnError += (_, e) => { QueueStatus($"Error: {e.Message}"); };

        MainWebSocket.OnMessage += (_, e) =>
                                   {
                                       if (e.Data == "pong")
                                           return;

                                       lock (receivedMessagesQueue)
                                       {
                                           receivedMessagesQueue.Enqueue(e.Data);
                                       }
                                   };

        try
        {
            MainWebSocket.ConnectAsync();
        }
        catch (Exception e)
        {
            QueueStatus($"Failed to connect: {e.Message}");
            CloseSocket();
        }
    }

    private void ParseAndReceiveMessage(string message)
    {
        OnMessageReceived?.Invoke(message);

        try
        {
            JObject jObject = JObject.Parse(message);
            OnJsonReceived?.Invoke(jObject);

            string type = jObject[nameof(type)]?.ToString();

            switch (type)
            {
                case nameof(message):
                {
                    string serverMessage = jObject[nameof(message)]?.ToString();

                    if (!string.IsNullOrEmpty(serverMessage) && ServerStatusNotifications.IsEnabled)
                        NotificationManager.SendNotification(
                                "<color=green>Hamburbur Server</color>",
                                serverMessage,
                                5f,
                                false,
                                false);

                    break;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private void QueueStatus(string message)
    {
        lock (statusQueue)
        {
            statusQueue.Enqueue(message);
        }
    }

    private void CloseSocket()
    {
        if (MainWebSocket == null)
            return;

        try
        {
            MainWebSocket.CloseAsync();
        }
        catch
        {
            // ignored
        }

        MainWebSocket = null;
    }
}