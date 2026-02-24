using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Rig", "Go to the rig category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Rig : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Rig");
}