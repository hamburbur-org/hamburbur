using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using hamburbur.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Server_Api_Communicator;

public class TrackerManager : MonoBehaviour
{
    private void Start()
    {
        RigUtils.OnRigCosmeticsLoaded += rig =>
                                         {
                                             NetPlayer player = rig.creator;

                                             if (rig == null || player.GetPlayerRef() == PhotonNetwork.LocalPlayer ||
                                                 HamburburOrgData.AllAdmins.ContainsKey(player.UserId))
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

    private static IEnumerator SendPlayerDataSync(Dictionary<string, Dictionary<string, string>> data, string directory,
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

        UnityWebRequest request = new(HamburburOrgData.HamburburUrl + "/syncdata", "POST");
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
    }
}