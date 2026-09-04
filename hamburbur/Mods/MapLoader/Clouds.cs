using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(Clouds), "Go to Clouds Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Clouds : hamburburmod
{
    protected override void Pressed()
    {
        Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.skyJungle), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-84f, 31f, -78f));
    }
}