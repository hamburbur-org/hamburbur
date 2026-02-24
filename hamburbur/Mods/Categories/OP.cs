using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("OP", "Go to the OP category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled, 0)]
public class OP : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("OP");
}