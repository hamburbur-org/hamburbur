using hamburbur.Mod_Backend;
using hamburbur.Mods.OP;
using UnityEngine;

namespace hamburbur.Mods.Visual;

[hamburburmod("Anti Lurker", "STFU about monkey monkey monkey monkey.", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class AntiLurker : hamburburmod
{
    protected override void OnEnable() => HalloweenInteractables.Lurker.gameObject.SetActive(false);

    protected override void OnDisable() => HalloweenInteractables.Lurker.gameObject.SetActive(true);
}