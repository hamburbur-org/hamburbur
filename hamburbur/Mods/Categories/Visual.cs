using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(nameof(Visual), "Go to the visual category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Visual : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(Visual));
}