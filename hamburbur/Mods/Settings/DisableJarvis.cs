using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Misc;
using hamburbur.Tools;

namespace hamburbur.Mods.Settings;

[hamburburmod("Disable Jarvis", "Makes it so when you say jarvis or any of the words it wont do anything", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DisableJarvis : hamburburmod
{
    public static bool IsEnabled;
    protected override void OnEnable()
    {
        IsEnabled = true;
        
        VoiceControls.Instance.Obliterate();
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        
        CoroutineManager.Instance.StartCoroutine(RestartJarvis.AddJarvis());
    }
}