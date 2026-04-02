using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Credits;

[hamburburmod("<color=#3EB075>Kormakur</color>", "Main ideas and themes provider", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Kormakur : hamburburmod
{
    protected override void Pressed() => Debug.Log("No GitHub");
}