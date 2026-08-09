using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Safety Settings", "Go to the safety settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SafetySettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Safety Settings");
}
