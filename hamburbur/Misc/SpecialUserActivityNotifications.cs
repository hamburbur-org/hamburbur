using System.Collections.Generic;
using System.Linq;
using hamburbur.Components;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using hamburbur.Server_Api_Communicator;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;
using Photon.Pun;

namespace hamburbur.Misc;

public class SpecialUserActivityNotifications : Singleton<SpecialUserActivityNotifications>
{
    private void Start() => RigUtils.OnRigCosmeticsLoaded += OnRigCosmeticsLoaded;

    private void OnRigCosmeticsLoaded(VRRig rig)
    {
        if (rig.IsLocalRig() || rig._playerOwnedCosmetics == null || !SpecialUserNotification.IsEnabled)
            return;

        string specialCosmetics = HamburburOrgData.Data["specialCosmetics"]
                                                 ?.ToObject<Dictionary<string, string>>()
                                                  .Where(cosmeticData => rig.HasCosmetic(cosmeticData.Key))
                                                  .Aggregate("",
                                                           (current, cosmeticData) =>
                                                                   current + cosmeticData.Value + ", ");

        specialCosmetics = specialCosmetics.TrimEnd(',', ' ');
        specialCosmetics = specialCosmetics.Trim();

        string userName = HamburburOrgData.Data["knownPeople"]
                                         ?.ToObject<Dictionary<string, string>>()
                                          .GetValueOrDefault(rig.creator.UserId, "");

        if (string.IsNullOrEmpty(specialCosmetics) && string.IsNullOrWhiteSpace(userName))
            return;

        JObject trackingData = new()
        {
                { "isUserKnown", !string.IsNullOrWhiteSpace(userName) },
                { "username", userName },
                { "hasSpecialCosmetic", !string.IsNullOrWhiteSpace(specialCosmetics) },
                { "specialCosmetic", specialCosmetics },
                { "roomCode", PhotonNetwork.CurrentRoom.Name },
                { "playersInRoom", PhotonNetwork.CurrentRoom.PlayerCount },
                { "inGameName", rig.creator.NickName },
                { "gameModeString", NetworkSystem.Instance.GameModeString },
                { "userId", rig.creator.UserId },
        };

        NotificationManager.SendNotification("<color=green>Special User</color>",
                $"{(trackingData["isUserKnown"].ToObject<bool>() ? trackingData["username"].ToObject<string>() : "Someone")} {(trackingData["hasSpecialCosmetic"].ToObject<bool>() ? $"with {trackingData["specialCosmetic"].ToObject<string>()}" : "")} found in {(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == trackingData["roomCode"].ToObject<string>() ? "your code" : $"code {trackingData["roomCode"].ToObject<string>()}")} with {trackingData["playersInRoom"].ToObject<int>()} players. Their in game name is {trackingData["inGameName"].ToObject<string>()} and the gamemode string is {trackingData["gameModeString"].ToObject<string>()}",
                10f, true, true);
    }
}