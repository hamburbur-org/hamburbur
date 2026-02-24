using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Macros", "Go to the macros category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Macros : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Macros");
}