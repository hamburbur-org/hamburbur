using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("BaggZ", "Spawns the bag asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class BaggZ : hamburburmod
{
    private static int allocatedId = -1;

    protected override void OnEnable()
    {
        allocatedId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "bag",
                allocatedId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedId, 3);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, allocatedId,
                new Vector3(0f, 0f, 0f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, allocatedId,
                Quaternion.Euler(0f, 0f, 0f));
        
        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedId,
                Vector3.One);
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedId);
        allocatedId = -1;
    }
}