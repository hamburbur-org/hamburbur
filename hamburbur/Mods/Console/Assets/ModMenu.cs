using hamburbur.Mod_Backend;
using Photon.Realtime;
using Vector3 = System.Numerics.Vector3;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Mod Menu", "Spawns the mod menu asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class ModMenu : hamburburmod
{
    private static int allocatedId = -1;

    protected override void OnEnable()
    {
        allocatedId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "clickbaitmenu‎", "Mod Menu",
                allocatedId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedId, 1);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, allocatedId,
                new Vector3(-0.09f, 0.125f, 0f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, allocatedId,
                new Vector3(0f, 110f, 80f));
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedId);
        allocatedId = -1;
    }
}