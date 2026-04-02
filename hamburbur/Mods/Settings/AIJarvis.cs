using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("AI Jarvis", "Use Pollinations AI when talking with Jarvis", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class AIJarvis : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}