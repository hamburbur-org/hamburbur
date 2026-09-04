using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using hamburbur.Tools;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Server_Api_Communicator;

public class TrackerManager : MonoBehaviour
{
    private void Start() =>
            RigUtils.OnRigCosmeticsLoaded += rig =>
                                             {
                                                 NetPlayer player = rig.creator;

                                                 if (rig == null || player.IsLocal ||
                                                     HamburburOrgData.AllAdmins.ContainsKey(player.UserId))
                                                     return;

                                                 Plugin.Instance.StartCoroutine(SendRigData(rig, player));
                                             };

    private static IEnumerator SendRigData(VRRig rig, NetPlayer player)
    {
        Task<string> creationDateTask = rig.GetCreationDate();

        yield return new WaitUntil(() => creationDateTask.IsCompleted);

        string userCreationDate = creationDateTask.Status == TaskStatus.RanToCompletion
                                          ? creationDateTask.Result
                                          : null;

        Dictionary<string, object> customProperties = player.GetCustomProperties();

        Dictionary<string, Dictionary<string, object>> data = new()
        {
                [player.UserId] = new Dictionary<string, object>
                {
                        {
                                "userId",
                                player.UserId
                        },
                        {
                                "userName",
                                player.SanitizedNickName
                        },
                        {
                                "userCreationDate",
                                userCreationDate
                        },
                        {
                                "rawCosmeticString",
                                rig._playerOwnedCosmetics.Concat()
                        },
                        {
                                "customProperties",
                                customProperties
                        },
                        {
                                "roomCode",
                                Tools.Utils.CleanString(PhotonNetwork.CurrentRoom.Name, 12, ['@',])
                        },
                        {
                                "playersInCode",
                                PhotonNetwork.PlayerList.Length
                        },
                        {
                                "gameMode",
                                NetworkSystem.Instance.GameModeString
                        },
                        {
                                "trackedTime",
                                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        },
                },
        };

        yield return SendPlayerDataSync(
                data,
                PhotonNetwork.CurrentRoom.Name,
                PhotonNetwork.CloudRegion,
                NetworkSystem.Instance.GameModeString);
    }

    private static IEnumerator SendPlayerDataSync(
            Dictionary<string, Dictionary<string, object>> data,
            string                                         directory,
            string                                         region,
            string                                         gameMode)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = Tools.Utils.CleanString(directory, 12, ['@',]),
                region    = Tools.Utils.CleanString(region,    3),
                gameMode  = Tools.Utils.CleanString(gameMode,  128, [';',]),
                data,
                playersCount = PhotonNetwork.PlayerList.Length,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new(Constants.HamburburUrl + "/syncdata", "POST");
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
    }
}