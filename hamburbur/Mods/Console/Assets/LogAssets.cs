using System.Collections.Generic;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Log Assets", "Log Assets", ButtonType.Fixed, AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class LogAssets : hamburburmod
{
    protected override void Pressed()
    {
        foreach (KeyValuePair<string, AssetBundle> asset in Components.Console.AssetBundlePool)
            Debug.Log("[Console Asset] " + asset.Key + ":" + asset.Value);
    }
}