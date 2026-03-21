using System;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Astronomical", "Fortnite astronomical event", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Astronomical : hamburburmod
{
    public override  Type[] Dependencies => [typeof(DarkFade),];
    
    private int assetId;
    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();
        
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets",
                "Astroworld_Planet", assetId);

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(-64.2f, 15f, -65.46f));
        
        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 10f);
        
        Components.Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "VideoPlayer",
                VideoPlayerType.Instance.CurrentUrl);
    }

    protected override void OnDisable() => Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}