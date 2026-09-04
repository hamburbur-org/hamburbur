using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(Mountains), "Go to Mountains Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Mountains : hamburburmod
{
    protected override void Pressed()
    {
        Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.mountain), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-15f, 20f, -111f));
    }
}