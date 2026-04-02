using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Remember Last Category", "Makes it so it remembers what page you were last on in each category", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RememberLastCategory : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}