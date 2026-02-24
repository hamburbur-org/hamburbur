using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Scoreboard", "Go to the scoreboard category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Scoreboard : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Scoreboard");
}