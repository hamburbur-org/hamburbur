using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(City), "Go to City Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class City : hamburburmod
{
    protected override void Pressed()
    {
        Tools.Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.city), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-58f, 17.2f, -103f));
    }
}