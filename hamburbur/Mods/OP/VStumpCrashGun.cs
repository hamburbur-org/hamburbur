using ExitGames.Client.Photon;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.OP;

[hamburburmod("VStump Crash Gun", "Crashes whoever your hand desires", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class VStumpCrashGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private bool wasShooting;

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null)
        {
            wasShooting = false;

            return;
        }

        if (wasShooting)
            return;

        wasShooting = true;

        for (int i = 0; i < 11; i++)
            PhotonNetwork.RaiseEvent(180,
                    new object[]
                            { Constants.HamburburUrl, true, },
                    new RaiseEventOptions { TargetActors = [gunLib.ChosenRig.creator.ActorNumber,], },
                    SendOptions.SendUnreliable);
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();
        wasShooting = false;
    }
}