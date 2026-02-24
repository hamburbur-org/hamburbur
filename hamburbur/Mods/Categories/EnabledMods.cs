using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Enabled Mods", "Go to the enabled mods category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class EnabledMods : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Enabled Mods");
}