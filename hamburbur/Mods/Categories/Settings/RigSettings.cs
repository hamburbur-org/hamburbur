using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Rig Settings", "Go to the rig settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class RigSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Rig Settings");
}
