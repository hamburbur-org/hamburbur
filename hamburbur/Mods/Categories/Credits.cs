using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(nameof(Credits), "Go to the credits category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Credits : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(Credits));
}