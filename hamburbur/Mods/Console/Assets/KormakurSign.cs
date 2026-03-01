using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Kormakur Sign", "A sign that lets you tell people who kormakur is if they have console",
        ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class KormakurSign : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "KormakurSign",
                assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
                new Vector3(0.29f, -0.2f, -0.1272f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId,
                Quaternion.Euler(355f, 275f, 265f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}