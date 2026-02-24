using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Custom Maps", "Go to the custom maps category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class CustomMaps : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Custom Maps");
}