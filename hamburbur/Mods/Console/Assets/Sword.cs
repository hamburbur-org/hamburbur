using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(nameof(Sword), "Summons the sword asset",
    ButtonType.Togglable,
    AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Sword : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", nameof(Sword),
            assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
            new Vector3(0.1f, 0.1f, 0.2f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId,
            Quaternion.Euler(0f,  90f, 90f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 0.1f);
    }

    protected override void OnDisable() =>
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}