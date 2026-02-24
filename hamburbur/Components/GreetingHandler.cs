using Photon.Pun;
using TMPro;
using UnityEngine;

namespace hamburbur.Components;

public class GreetingHandler : MonoBehaviour
{
    private TextMeshProUGUI greetingText;
    private TextMeshProUGUI roomInfoText;

    private void Start()
    {
        greetingText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        roomInfoText =
                transform.GetChild(1)
                         .GetComponent<
                                  TextMeshProUGUI>(); // <size=32><b>Room Info</b></size>\nCode:\nPlayers In Room:\nQueue:
    }

    private void Update()
    {
        string playerName = "BADGORILLA";
        if (!NetworkSystem.Instance.LocalPlayer.SanitizedNickName.IsNullOrEmpty())
            playerName = NetworkSystem.Instance.LocalPlayer.SanitizedNickName;

        greetingText.text = $"Hey, {playerName}!";
        if (!NetworkSystem.Instance.InRoom)
        {
            roomInfoText.text = "<size=32><b>Room Info</b></size>\nCode: -\nPlayers In Room: -\nQueue: -";
        }
        else
        {
            string roomCode      = PhotonNetwork.CurrentRoom.Name;
            byte   playersInCode = PhotonNetwork.CurrentRoom.PlayerCount;
            string queue         = GetQueueKey(NetworkSystem.Instance.GameModeString);
            roomInfoText.text =
                    $"<size=32><b>Room Info</b></size>\nCode: {roomCode}\nPlayers In Room: {playersInCode}/{PhotonNetwork.CurrentRoom.MaxPlayers}\nQueue: {queue}";
        }
    }

    private string GetQueueKey(string gamemodeString)
    {
        gamemodeString = gamemodeString.ToUpper();

        if (gamemodeString.Contains("DEFAULT")) return "Default";
        if (gamemodeString.Contains("MINIGAMES")) return "Minigames";

        return gamemodeString.Contains("COMPETITIVE") ? "Competitive" : gamemodeString;
    }
}