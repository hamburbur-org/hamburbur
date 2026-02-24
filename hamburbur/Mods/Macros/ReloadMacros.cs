using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Macros;

[hamburburmod(                "Reload Macros", "Reloads the macro buttons", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class ReloadMacros : hamburburmod
{
    protected override void Pressed() => MacroManager.LoadAllMacros();
}