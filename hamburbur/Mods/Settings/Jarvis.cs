using System.Collections;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Jarvis", "Use the built in voice assistant Jarvis", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Enabled, 0)]
public class Jarvis : hamburburmod
{
    protected override void OnEnable()
    {
        CoroutineManager.Instance.StartCoroutine(AddJarvis());
    }

    private IEnumerator AddJarvis()
    {
        yield return new WaitForEndOfFrame();
        Plugin.Instance.ComponentHolder.AddComponent<VoiceControls>();
    }

    protected override void OnDisable()
    {
        VoiceControls.Instance.Obliterate();
    }
}