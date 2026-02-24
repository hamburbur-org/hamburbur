using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Jarvis Speak", "Makes Jarvis speak the notifications being sent", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class JarvisSpeak : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}