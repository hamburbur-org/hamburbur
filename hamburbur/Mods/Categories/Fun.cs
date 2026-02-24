using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Fun", "Go to the fun category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Fun : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Fun");
}