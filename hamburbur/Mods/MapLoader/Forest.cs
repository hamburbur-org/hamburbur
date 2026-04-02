using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod("Forest", "Go to Forest Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Forest : hamburburmod
{
    protected override void Pressed()
    {
        ZoneManagement.SetActiveZone(GTZone.forest);
        Tools.Utils.TeleportPlayer(new Vector3(-76f, 5f, -80f));
    }
}