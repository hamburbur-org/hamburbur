using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.CustomMaps;

[hamburburmod("Gorilla Mystery", "Go to the Gorilla Mystery map mods category", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GM : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Gorilla Mystery");
}
