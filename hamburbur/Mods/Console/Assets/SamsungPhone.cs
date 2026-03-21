using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(                   "Spawn Samsung Phone ",     "Spawns a samsung phone in your left hand", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class SamsungPhone : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "samsungphone", assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 1);
        
        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
                new Vector3(-0.075f, 0.1f, 0f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId,
                Quaternion.Euler(80f,  90f, 180f));
        
        if (BigAssets.isEnabled)
                Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 1.5f);
        else
                Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 0.3f);
        
        Components.Console.ExecuteCommand("asset-setvideo",  ReceiverGroup.All, assetId, "VideoPlayer", VideoPlayerType.Instance.CurrentUrl);

        Components.Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, assetId);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}