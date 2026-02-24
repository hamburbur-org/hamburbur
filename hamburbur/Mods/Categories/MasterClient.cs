using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Master Client", "Go to the master client category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MasterClient : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Master Client");
}