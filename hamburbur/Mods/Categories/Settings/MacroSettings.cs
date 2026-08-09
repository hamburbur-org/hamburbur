using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Macro Settings", "Go to the macro settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MacroSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Macro Settings");
}
