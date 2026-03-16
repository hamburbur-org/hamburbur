using hamburbur.Mod_Backend;
using hamburbur.Patches;

namespace hamburbur.Mods.Misc;

[hamburburmod("VIM Spoof", "Spoofs that you have VIM locally, but some stuff such as high quality mic is networked",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class VIMSpoof : hamburburmod
{
    protected override void OnEnable()  => SubscriptionPatches.enabled = true;
    protected override void OnDisable() => SubscriptionPatches.enabled = false;
}