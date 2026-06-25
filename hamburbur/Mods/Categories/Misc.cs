using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(nameof(Misc), "Go to the misc category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Misc : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(Misc));
}