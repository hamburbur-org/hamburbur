using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Visual Settings", "Go to the visual settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class VisualSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Visual Settings");
}
