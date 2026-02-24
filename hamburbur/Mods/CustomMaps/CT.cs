using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.CustomMaps;

[hamburburmod("Crown Tag", "Go to the crown tag map mods category", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class CT : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Crown Tag");
}