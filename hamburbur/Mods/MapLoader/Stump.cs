using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.MapLoader;

[hamburburmod(nameof(Stump), "Go to Stump", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Stump : hamburburmod
{
    protected override void Pressed()
    {
        Extensions.RecursiveInvoke(() => ZoneManagement.SetActiveZone(GTZone.forestWithCity), 3);
        Tools.Utils.TeleportPlayer(new Vector3(-64f, 13f, -105f));
    }
}