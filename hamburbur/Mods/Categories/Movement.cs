using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Movement", "Go to the movement category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Movement : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Movement");
}