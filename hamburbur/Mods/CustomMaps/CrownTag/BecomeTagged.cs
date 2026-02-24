using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.CustomMaps.CrownTag;

[hamburburmod("Become Tagged", "Become Tagged", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class BecomeTagged : hamburburmod
{
    protected override void Update()
    {
        foreach (Player player in PhotonNetwork.PlayerListOthers)
            PhotonNetwork.RaiseEvent(180, new object[] { "tagAttempt", (double)player.ActorNumber, },
                    new RaiseEventOptions { Receivers = ReceiverGroup.All, }, SendOptions.SendReliable);
    }
}