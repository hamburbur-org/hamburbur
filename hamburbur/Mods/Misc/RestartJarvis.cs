using System.Collections;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Force Restart Jarvis", "Kills and revives jarvis", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class RestartJarvis : hamburburmod
{
    protected override void Pressed()
    {
        VoiceControls.Instance.Obliterate();
        CoroutineManager.Instance.StartCoroutine(AddJarvis());
    }

    public static IEnumerator AddJarvis()
    {
        yield return new WaitForEndOfFrame();
        Plugin.Instance.ComponentHolder.AddComponent<VoiceControls>();
    }
}