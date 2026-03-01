using System;
using System.Linq;
using GorillaNetworking;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod(                                                                                 "Networked CosmetX",
        "Forcefully networks your worn cosmetics even if you don't own them to console users", ButtonType.Togglable,
        AccessSetting.AdminOnly,                                                               EnabledType.Disabled, 0)]
public class NetworkedCosmetX : hamburburmod
{
    private bool Enabled;

    private int[] oldCosmetics;
    private int[] oldTryOn;

    protected override void Start()
    {
        NetworkSystem.Instance.OnPlayerJoined += (Action<NetPlayer>)OnPlayerJoinSpoof;
    }

    protected override void Update()
    {
        if (!NetworkSystem.Instance.InRoom)
            return;

        if (oldCosmetics == CosmeticsController.instance.currentWornSet.ToPackedIDArray())
            return;

        oldCosmetics = CosmeticsController.instance.currentWornSet.ToPackedIDArray();
        string concat = CosmeticsController.instance.currentWornSet.ToDisplayNameArray()
                                           .Aggregate("", (current, cosmetic) => current + cosmetic);

        if (string.IsNullOrEmpty(concat))
            return;

        Components.Console.ExecuteCommand("cosmetic", ReceiverGroup.Others, concat);
        GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others,
                CosmeticsController.instance.currentWornSet.ToPackedIDArray(),
                CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
    }

    protected override void OnDisable() => Enabled = false;

    protected override void OnEnable()
    {
        Enabled = true;

        if (!NetworkSystem.Instance.InRoom)
            return;

        oldCosmetics = CosmeticsController.instance.currentWornSet.ToPackedIDArray();
        string concat = CosmeticsController.instance.currentWornSet.ToDisplayNameArray()
                                           .Aggregate("", (current, cosmetic) => current + cosmetic);

        if (string.IsNullOrEmpty(concat))
            return;

        Components.Console.ExecuteCommand("cosmetic", ReceiverGroup.Others, concat);
        GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others,
                CosmeticsController.instance.currentWornSet.ToPackedIDArray(),
                CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
    }

    private void OnPlayerJoinSpoof(NetPlayer player)
    {
        if (!Enabled)
            return;

        string concat = CosmeticsController.instance.currentWornSet.ToDisplayNameArray()
                                           .Aggregate("", (current, cosmetic) => current + cosmetic);

        if (string.IsNullOrEmpty(concat))
            return;

        Components.Console.ExecuteCommand("cosmetic", player.ActorNumber, concat);
        GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others,
                CosmeticsController.instance.currentWornSet.ToPackedIDArray(),
                CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
    }
}