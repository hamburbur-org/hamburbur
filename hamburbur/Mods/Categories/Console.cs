using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Console", "Go to the console category", ButtonType.Category, AccessSetting.AdminOnly,
        EnabledType.Disabled, 0)]
public class Console : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Console");
}