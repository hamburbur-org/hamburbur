using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(Space), "Go to Space Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Space : hamburburmod
{
    protected override void Pressed()
    {
        Tools.Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.spaceMap), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-542f, 15.2f, 4f));
    }
}