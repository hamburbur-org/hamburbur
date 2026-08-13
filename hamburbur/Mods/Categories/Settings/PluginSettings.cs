using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Plugins", "View and manage loaded hamburbur plugins", ButtonType.Category,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class PluginSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Plugins");
}
