using System;
using System.Collections;
using hamburbur.Components;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Server_API;

public class HamburburData : Singleton<HamburburData>
{
    public static Action<JObject> OnDataReloaded;

    public static JObject Data       { get; private set; }
    public static bool    DataLoaded { get; private set; }

    private IEnumerator Start()
    {
        while (true)
        {
            UnityWebRequest hamburburWebRequest = UnityWebRequest.Get(Constants.HamburburDataUrl);

            yield return hamburburWebRequest.SendWebRequest();

            if (hamburburWebRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = hamburburWebRequest.downloadHandler.text;

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
}