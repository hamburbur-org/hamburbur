using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.CustomMaps;

[hamburburmod("Chimp Combat", "Go to the chimp combat map mods category", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class CC : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Chimp Combat");
}