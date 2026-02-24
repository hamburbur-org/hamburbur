using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Mods.OP;

[hamburburmod(
        "ii Stupid Check",
        "Shoot someone with the gun and it'll tell you if they have used ii Stupid in the last 30 days.",
        ButtonType.Togglable,
        AccessSetting.Public,
        EnabledType.Disabled,
        0
)]
public class iiStupidCheck : hamburburmod
{
    private readonly GunLib      gunLib       = new() { ShouldFollow = true, };
    private readonly List<VRRig> StupidPeople = [];
    private          bool        wasShooting;

    public override Type[] Dependencies => [typeof(iiTelemetry),];

    protected override void Start()
    {
        gunLib.Start();

        RigUtils.OnRigUnloaded += rig =>
                                  {
                                      if (rig == null)
                                          return;

                                      if (StupidPeople.Contains(rig))
                                          StupidPeople.Remove(rig);
                                  };
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (isShooting && gunLib.ChosenRig != null)
        {
            if (!wasShooting)
            {
                wasShooting = true;
                CheckPlayer(gunLib.ChosenRig);
            }
        }
        else
        {
            wasShooting = false;
        }
    }

    protected override void OnDisable() => gunLib.OnDisable();

    private void CheckPlayer(VRRig rigToCheck)
    {
        if (!iiTelemetry.isOnline)
        {
            NotificationManager.SendNotification(
                    "<color=red>Error</color>",
                    "iiDk's server is offline or inaccessible at this moment",
                    5f,
                    false,
                    false);

            return;
        }

        string userId = rigToCheck.Creator.UserId;

        if (string.IsNullOrEmpty(userId)) return;

        if (StupidPeople.Contains(rigToCheck))
        {
            NotificationManager.SendNotification("<color=orange>Stupid Person</color>",
                    $"Player <color=#{ColorUtility.ToHtmlStringRGB(rigToCheck.playerColor)}>{rigToCheck.Creator.NickName}</color> <color=green>is using</color> <color=#FFAA50>ii Stupid</color>",
                    5f, true, false);

            return;
        }

        CheckIfPlayerIsStupid(userId, (hasMenu, error) =>
                                      {
                                          if (hasMenu)
                                          {
                                              NotificationManager.SendNotification(
                                                      "<color=orange>Stupid Person</color>",
                                                      $"Player <color=#{ColorUtility.ToHtmlStringRGB(rigToCheck.playerColor)}>{rigToCheck.Creator.NickName}</color> <color=green>is using</color> <color=#FFAA50>ii Stupid</color>",
                                                      5f,
                                                      true,
                                                      false);

                                              if (!StupidPeople.Contains(rigToCheck))
                                                  StupidPeople.Add(rigToCheck);
                                          }
                                          else
                                          {
                                              if (error.Contains("already") || error.Contains("friend"))
                                              {
                                                  NotificationManager.SendNotification(
                                                          "<color=orange>Stupid Person</color>",
                                                          $"Player <color=#{ColorUtility.ToHtmlStringRGB(rigToCheck.playerColor)}>{rigToCheck.Creator.NickName}</color> <color=green>is using</color> <color=#FFAA50>ii Stupid</color>",
                                                          5f,
                                                          true,
                                                          false);

                                                  StupidPeople.Add(rigToCheck);
                                              }
                                              else if (error.ToLower().Contains("too many requests"))
                                              {
                                                  NotificationManager.SendNotification(
                                                          "<color=red>Error</color>",
                                                          "<color=red>Woah, slow down there. You're checking too fast!</color>",
                                                          5f,
                                                          false,
                                                          false);
                                              }
                                              else
                                              {
                                                  NotificationManager.SendNotification(
                                                          "<color=orange>Stupid Person</color>",
                                                          $"Player <color=#{ColorUtility.ToHtmlStringRGB(rigToCheck.playerColor)}>{rigToCheck.Creator.NickName}</color> <color=red>doesn't have</color> <color=#FFAA50>ii Stupid</color>",
                                                          5f,
                                                          false,
                                                          false);
                                              }
                                          }
                                      });
    }

    private void CheckIfPlayerIsStupid(string userId, Action<bool, string> callback) =>
            CoroutineManager.Instance.StartCoroutine(ExecuteAction(userId, "frienduser", () => callback(true, null),
                    error => callback(false,                                                                  error)));

    private IEnumerator ExecuteAction(string uid, string action, Action success, Action<string> failure)
    {
        UnityWebRequest request = new($"https://iidk.online/{action}", "POST");

        string json = JsonConvert.SerializeObject(new { uid, });
        byte[] raw  = Encoding.UTF8.GetBytes(json);

        request.uploadHandler   = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            success.Invoke();
        }
        else
        {
            string reason = string.IsNullOrEmpty(request.error) ? "Unknown error" : request.error;

            try
            {
                string responseText = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    Dictionary<string, object> responseJson =
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(responseText);

                    if (responseJson != null && responseJson.TryGetValue("error", out object value))
                        reason = value.ToString();
                }
            }
            catch { }

            failure.Invoke(reason);
        }
    }
}