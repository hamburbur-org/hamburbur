using System.Collections;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Dark Fade Fog", "Adds fog to the map.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class DarkFade : hamburburmod
{
    private float     currentFogOpacity;
    private Coroutine fadeFogCoroutine;

    protected override void Update()
    {
        if (fadeFogCoroutine != null)
            CoroutineManager.Instance.StopCoroutine(fadeFogCoroutine);

        fadeFogCoroutine = CoroutineManager.Instance.StartCoroutine(FadeFog(0.9f));
    }

    protected override void OnDisable()
    {
        if (fadeFogCoroutine != null)
        {
            CoroutineManager.Instance.StopCoroutine(fadeFogCoroutine);
            fadeFogCoroutine = null;
        }

        Components.Console.ExecuteCommand("setfog",   ReceiverGroup.All, 0f, 0f, 0f, 0f, 0f, float.MaxValue, 0f);
        Components.Console.ExecuteCommand("resetfog", ReceiverGroup.All);
        Tools.Utils.RPCProtection();
    }

    private IEnumerator FadeFog(float targetOpacity)
    {
        const float Duration     = 2f;
        float       elapsedTime  = 0f;
        float       startOpacity = currentFogOpacity;

        while (elapsedTime < Duration)
        {
            elapsedTime       += Time.deltaTime;
            currentFogOpacity =  Mathf.Lerp(startOpacity, targetOpacity, elapsedTime / Duration);

            Components.Console.ExecuteCommand("setfog", ReceiverGroup.All, 0f, 0f, 0f, currentFogOpacity, 0f,
                    float.MaxValue, 0f);

            yield return null;
        }

        currentFogOpacity = targetOpacity;
        Components.Console.ExecuteCommand("setfog", ReceiverGroup.All, 0f, 0f, 0f, targetOpacity, 0f,
                float.MaxValue, 0f);
    }
}