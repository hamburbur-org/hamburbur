using System.Collections.Generic;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Realtime;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Cleanup Assets", "Destroys all current console assets", ButtonType.Fixed, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class CleanupAssets : hamburburmod
{
    protected override void Pressed()
    {
        foreach (KeyValuePair<int, Components.Console.ConsoleAsset> kvp in Components.Console.ConsoleAssets)
        {
            if (NetworkSystem.Instance.InRoom)
                Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, kvp.Key);

            if (kvp.Value.assetObject != null)
                kvp.Value.assetObject.Obliterate();
        }
    }
}