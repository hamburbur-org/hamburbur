using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.OP;

[hamburburmod(                "Basement Door Spam", "Spam open and closes the basement door", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class BasementDoorSpam : hamburburmod
{
    private const float Delay = 0.1f;
    private       int   currentDoorIndex;

    private GTDoor door;

    private float lastTime;

    protected override void Update()
    {
        if (!(lastTime + Delay < Time.time) || !PhotonNetwork.InRoom)
            return;

        lastTime = Time.time;

        door.photonView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GetNextDoorState());
        Tools.Utils.RPCProtection();
    }

    protected override void OnEnable()
    {
        door = Object.FindObjectOfType<GTDoor>();

        if (!PhotonNetwork.InRoom)
            return;

        door.photonView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
        Tools.Utils.RPCProtection();
    }

    private GTDoor.DoorState GetNextDoorState()
    {
        GTDoor.DoorState[] states =
        [
                GTDoor.DoorState.Closing,
                GTDoor.DoorState.HeldOpen,
        ];

        GTDoor.DoorState state = states[currentDoorIndex];

        currentDoorIndex = (currentDoorIndex + 1) % states.Length;

        return state;
    }
}