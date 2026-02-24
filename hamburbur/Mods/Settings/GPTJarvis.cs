using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("GPT Jarvis", "Use Chat GPT to talk with Jarvis", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GPTJarvis : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}