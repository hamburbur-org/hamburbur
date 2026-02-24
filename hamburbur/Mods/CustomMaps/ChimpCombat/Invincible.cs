using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.CustomMaps.ChimpCombat;

[hamburburmod("Invincible", "No die die", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Invincible : hamburburmod
{
    protected override void Update()
    {
        RaiseEventOptions options = new()
        {
                Receivers = ReceiverGroup.All,
        };

        PhotonNetwork.RaiseEvent(180,
                new object[]
                {
                        "SyncInfo",
                        (double)PhotonNetwork.LocalPlayer.ActorNumber,
                        (double)1,    //if shoot
                        (double)1,    //gun 
                        (double)1,    //grenade
                        (double)1000, //health
                        (double)1,    //boost
                        (double)1,    //trait
                        (double)1,    //skin
                }, options,
                SendOptions.SendReliable);
    }
}