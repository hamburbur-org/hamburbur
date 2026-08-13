using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.CustomMaps;

[hamburburmod("Meccha Gorilla", "Open the Meccha Gorilla custom map mods", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MGorilla : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Meccha Gorilla");
}
