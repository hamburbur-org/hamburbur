using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.OP;

[hamburburmod(                "Elevator Door Spam", "Spam open and closes the elevator door", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ElevatorDoorSpam : hamburburmod
{
    private const float Delay = 0.2f;

    private float lastTime;

    private bool state;

    protected override void Update()
    {
        if (!(lastTime + Delay < Time.time) || !PhotonNetwork.InRoom)
            return;

        lastTime = Time.time;

        state = !state;

        GRElevatorManager.ElevatorButtonPressed(
                state ? GRElevator.ButtonType.Open : GRElevator.ButtonType.Close,
                GRElevatorManager._instance.currentLocation
        );

        Tools.Utils.RPCProtection();
    }
}