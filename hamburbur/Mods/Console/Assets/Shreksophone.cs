using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Shreksophone", "Shreksophone", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class Shreksophone : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "shrek",
                assetId);

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(-76f, 1.7f, -80f));
        
        Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId, Quaternion.Euler(0f, 40f, 0f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5f);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}