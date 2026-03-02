using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod(            "Notify Presence", "Announces to all console users that an admin is in the lobby",
        ButtonType.Fixed, AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class NotifyPresence : hamburburmod
{
    protected override void Pressed()
    {
        Components.Console.ExecuteCommand("notify", ReceiverGroup.All,
            $"Admin {PhotonNetwork.LocalPlayer.NickName} is in the lobby!");
    }
}