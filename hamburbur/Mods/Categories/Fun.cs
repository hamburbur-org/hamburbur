using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(nameof(Fun), "Go to the fun category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Fun : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(Fun));
}