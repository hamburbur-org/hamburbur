using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Axe", "Summons the axe asset",
    ButtonType.Togglable,
    AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Axe : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "Axe",
            assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
            new Vector3(0.05f, 0.03f, 0f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId,
            Quaternion.Euler(0f, 0f, 90f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5);
    }

    protected override void OnDisable() =>
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}