using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Gun Settings", "Go to the gun settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GunSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Gun Settings");
}