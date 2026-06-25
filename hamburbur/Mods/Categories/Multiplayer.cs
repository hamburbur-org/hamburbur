using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(nameof(Multiplayer), "Go to the multiplayer category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Multiplayer : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(Multiplayer));
}