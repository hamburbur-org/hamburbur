using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Dynamic Menu Sounds", "Replaces the menu sound such as open with alternatives ones",
        ButtonType.Togglable, AccessSetting.Public,  EnabledType.Disabled, 0)]
public class DynamicMenuSounds : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}