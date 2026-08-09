using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Menu Title Theme Name", "Makes the title of the menu, the name of the current theme", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class MenuTitleThemeName : hamburburmod
{
    public static bool IsEnabled;
    
    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}