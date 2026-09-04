using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Movement Settings", "Go to the movement settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MovementSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Movement Settings");
}