using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Legacy Settings", "Go to the legacy settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class LegacySettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Legacy Settings");
}