using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Block Mods", "Open the Monke Blocks fun mods", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class BlockMods : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(BlockMods));
}