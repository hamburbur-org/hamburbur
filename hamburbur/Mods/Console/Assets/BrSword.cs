using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("BR Sword", "Spawns the sword asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class BrSword : hamburburmod
{
    private int allocatedSwordId = -1;

    protected override void OnEnable()
    {
        if (allocatedSwordId >= 0) return;

        allocatedSwordId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "brsword", "default",
                allocatedSwordId);

        if (BigAssets.isEnabled)
            Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedSwordId, Vector3.one * 5);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedSwordId, 2);
    }

    protected override void OnDisable()
    {
        if (allocatedSwordId >= 0)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedSwordId);
            allocatedSwordId = -1;
        }
    }
}