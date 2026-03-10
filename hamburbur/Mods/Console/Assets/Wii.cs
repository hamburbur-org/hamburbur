using System.Linq;
using System.Threading.Tasks;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Wii", "Wii twin", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Wii : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();
        
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "wiiremote",
                assetId);
        
        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2);
        
        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
                new Vector3(0.075f, 0.1f, 0.075f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId,
                Quaternion.Euler(80f,  5f, 0f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 150f);
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
    }
}