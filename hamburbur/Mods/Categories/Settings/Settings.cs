using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Settings", "Go to the settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Settings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Settings");
}