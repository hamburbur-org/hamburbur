using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod("No Admin Indicator", "Removes the admin indicator to play sneakily", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class NoAdminIndicator : hamburburmod
{
    private int lastPlayerCount = -1;

    protected override void Update()
    {
        if (!PhotonNetwork.InRoom)
        {
            lastPlayerCount = -1;

            return;
        }

        if (PhotonNetwork.PlayerList.Length == lastPlayerCount)
            return;

        Components.Console.ExecuteCommand("nocone", ReceiverGroup.All, true);
        lastPlayerCount = PhotonNetwork.PlayerList.Length;
    }

    protected override void OnEnable()
    {
        Components.Console.ExecuteCommand("nocone", ReceiverGroup.All, true);
        lastPlayerCount = -1;
    }

    protected override void OnDisable() => Components.Console.ExecuteCommand("nocone", ReceiverGroup.All, false);
}