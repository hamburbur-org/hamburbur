using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Console Assets", "Go to the console category", ButtonType.Category, AccessSetting.AdminOnly,
        EnabledType.Disabled, 0)]
public class ConsoleAssets : hamburburmod
{
    protected override void Pressed()
    {
        ButtonHandler.Instance.SetCategory("Console Assets");
    }
}