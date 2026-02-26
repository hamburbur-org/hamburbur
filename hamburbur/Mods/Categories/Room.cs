using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Room", "Go to the room category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Room : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Room");
}