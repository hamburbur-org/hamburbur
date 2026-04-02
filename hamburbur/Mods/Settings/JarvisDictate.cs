using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Jarvis Dictate", "Makes Jarvis speak some of your notifications", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class JarvisDictate : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}