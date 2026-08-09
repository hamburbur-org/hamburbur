using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(Forest), "Go to Forest Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Forest : hamburburmod
{
    protected override void Pressed()
    {
        Tools.Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.forest), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-76f, 5f, -80f));
    }
}