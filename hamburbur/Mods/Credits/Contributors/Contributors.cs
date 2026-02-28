using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Credits.Contributors;

[hamburburmod("Contributors", "Opens all of the hamburbur contributors. Ty to them for all the help!", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Contributors : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Contributors");
}