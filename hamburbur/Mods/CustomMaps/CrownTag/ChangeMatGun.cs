using ExitGames.Client.Photon;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.CustomMaps.CrownTag;

[hamburburmod(                "Change Mat Gun", "Changes their material", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class ChangeMatGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
            PhotonNetwork.RaiseEvent(180,
                    new object[] { "changeMaterial", (double)1, (double)gunLib.ChosenRig.Creator.ActorNumber, },
                    new RaiseEventOptions { Receivers = ReceiverGroup.All, }, SendOptions.SendReliable);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}