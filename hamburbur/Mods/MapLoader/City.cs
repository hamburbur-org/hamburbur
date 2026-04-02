using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod("City", "Go to City Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class City : hamburburmod
{
    protected override void Pressed()
    {
        ZoneManagement.SetActiveZone(GTZone.city);
        Tools.Utils.TeleportPlayer(new Vector3(-56f, 17f, -100f));
    }
}