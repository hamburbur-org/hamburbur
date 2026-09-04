using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Multiplayer Settings", "Go to the multiplayer settings category", ButtonType.Category,
        AccessSetting.Public, EnabledType.Disabled,   0)]
public class MultiplayerSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Multiplayer Settings");
}