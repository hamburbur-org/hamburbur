using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Primary Menu Button", "Use Primary instead of Secondary to open and close the menu",
        ButtonType.Togglable, AccessSetting.Public,  EnabledType.Disabled, 0)]
public class PrimaryMenuButton : hamburburmod
{
    public static bool IsEnabled;

    public override string ModName => $"Menu Button: {(IsEnabled ? "Primary" : "Secondary")}";

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}