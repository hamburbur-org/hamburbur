using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Map Loader", "Go to the Map Loader category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class MapLoader : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Map Loader");
}