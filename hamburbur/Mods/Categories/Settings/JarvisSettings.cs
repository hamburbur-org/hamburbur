using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Jarvis Settings", "Go to the jarvis settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JarvisSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Jarvis Settings");
}