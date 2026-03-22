using System;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Television", "Spawns in a tv", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled,
        0)]
public class Television : hamburburmod
{
    private         int    assetId;
    private         int    sofaAssetId;
    public override Type[] Dependencies => [typeof(DarkFade),];

    protected override void OnEnable()
    {
        assetId     = Components.Console.GetFreeAssetID();
        sofaAssetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets",
                "TV", assetId);

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets",
                "sofa", sofaAssetId);

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(-57.1f, 5.6f, -37f));

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, sofaAssetId,
                new Vector3(-51.8f, 4.2f, -37.4f));

        Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId,
                Quaternion.Euler(270f, 0f, 0f));

        Components.Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, sofaAssetId,
                Quaternion.Euler(270f, 270f, 0f));

        Components.Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "VideoPlayer",
                VideoPlayerType.Instance.CurrentUrl);
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, sofaAssetId);
    }
}