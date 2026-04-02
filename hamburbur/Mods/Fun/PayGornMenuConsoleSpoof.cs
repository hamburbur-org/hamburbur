using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

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
            PhotonNetwork.RaiseEvent(68, new object[] { "confirmusing", "69.67", $"<size=300%>{GetRainbowText("pay gorn menu")}</size>", },
                    new RaiseEventOptions { TargetActors = [eventData.Sender,], }, SendOptions.SendReliable);
    }
    
    private string GetRainbowText(string input)
    {
        string result = "";
        for (int i = 0; i < input.Length; i++)
        {
            float  hue   = (float)i / input.Length;
            Color  color = Color.HSVToRGB(hue, 1f, 1f);
            string hex   = ColorUtility.ToHtmlStringRGB(color);
            result += $"<color=#{hex}>{input[i]}</color>";
        }

        return result;
    }
}