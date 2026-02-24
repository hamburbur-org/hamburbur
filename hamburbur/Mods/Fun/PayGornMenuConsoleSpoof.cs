using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.Fun;

[hamburburmod("Pay Gorn Menu Console Spoof", "Spoof the Pay Gorn Menu for Console admins", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class PayGornMenuConsoleSpoof : hamburburmod
{
    protected override void OnEnable()  => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    protected override void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;

    private void OnEventReceived(EventData eventData)
    {
        if (eventData.Code != 68)
            return;

        object[] data;

        if (eventData.Parameters.TryGetValue(ParameterCode.Data, out object rawData) && rawData is object[] dataArray)
            data = dataArray;
        else
            return;

        string command = (string)data[0];

        if (command == "isusing")
            PhotonNetwork.RaiseEvent(68, new object[] { "confirmusing", "69.420", "<size=200%>pay gorn menu</size>", },
                    new RaiseEventOptions { TargetActors = [eventData.Sender,], }, SendOptions.SendReliable);
    }
}