using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Console;

[hamburburmod("Dont Destroy Other Console Instances", "Makes it so it doesnt destroy seralyths console.", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class DontDestroyOtherConsoleInstances : hamburburmod
{
    public static bool IsEnabled;
    
    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}