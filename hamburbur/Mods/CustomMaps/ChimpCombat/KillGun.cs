using ExitGames.Client.Photon;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.CustomMaps.ChimpCombat;

[hamburburmod("Kill Gun", "Lets you Kill anyone", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class KillGun : hamburburmod
{
    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    private bool wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (isShooting && gunLib.ChosenRig != null)
        {
            if (!wasShooting)
            {
                RaiseEventOptions options = new()
                {
                        Receivers = ReceiverGroup.All,
                };

                PhotonNetwork.RaiseEvent(180,
                        new object[]
                        {
                                "HitPlayer",
                                (double)gunLib.ChosenRig.Creator.ActorNumber, (double)99999,
                                (double)PhotonNetwork.LocalPlayer.ActorNumber,
                        }, options,
                        SendOptions.SendReliable);
            }

            wasShooting = true;
        }
        else
        {
            wasShooting = false;
        }
    }

    protected override void OnDisable() => gunLib.OnDisable();
}