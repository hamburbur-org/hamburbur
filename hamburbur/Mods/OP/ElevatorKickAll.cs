using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.OP;

[hamburburmod("Elevator Kick All", "Elevator Kick All", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class ElevatorKickAll : hamburburmod
{
    protected override void Pressed()
    {
        GRElevatorManager elevatorManager = GRElevatorManager._instance;

        if (elevatorManager == null || elevatorManager.photonView == null) return;

        if (Tools.Utils.IsMasterClient)
        {
            Hashtable rpcHash = new()
            {
                    { 0, elevatorManager.photonView.ViewID },
                    { 2, PhotonNetwork.ServerTimestamp - 750 },
                    { 3, "RemoteActivateTeleport" },
                    {
                            4,
                            new object[]
                            {
                                    (int)elevatorManager.currentLocation, 2,
                                    GRElevatorManager.LowestActorNumberInElevator(),
                            }
                    },
            };

            if (elevatorManager.photonView.Prefix > 0)
                rpcHash[1] = (short)elevatorManager.photonView.Prefix;

            if (PhotonNetwork.PhotonServerSettings.RpcList.Contains("RemoteActivateTeleport"))
                rpcHash[5] = (byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf("RemoteActivateTeleport");

            PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(
                    200, rpcHash, new RaiseEventOptions { Receivers = ReceiverGroup.Others, },
                    new SendOptions
                            { Reliability = true, DeliveryMode = DeliveryMode.ReliableUnsequenced, Encrypt = false, }
            );
        }
        else
        {
            elevatorManager.SendRPC(
                    "RemoteElevatorButtonPress",
                    RpcTarget.MasterClient,
                    3,
                    (int)elevatorManager.currentLocation
            );
        }
    }
}