using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Menu Settings", "Go to the menu settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MenuSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Menu Settings");
}