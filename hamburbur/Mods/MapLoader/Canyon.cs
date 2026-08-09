using System;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(Canyon), "Go to Canyon Map", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Canyon : hamburburmod
{
    protected override void Pressed()
    {
        Tools.Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.canyon), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-84f, 31f, -78f));
    }
}