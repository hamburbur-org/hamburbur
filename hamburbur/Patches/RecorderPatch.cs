using System;
using hamburbur.Managers;
using HarmonyLib;
using Photon.Voice;
using Photon.Voice.Unity;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(Recorder))]
public static class RecorderPatch
{
    [HarmonyPatch(nameof(Recorder.SourceType), MethodType.Getter)]
    private static bool Prefix(ref Recorder.InputSourceType __result)
    {
        __result = Recorder.InputSourceType.Factory;

        return false;
    }

    [HarmonyPatch(nameof(Recorder.InputFactory), MethodType.Getter)]
    private static bool Prefix(ref Func<IAudioDesc> __result)
    {
        __result = () => VoiceManager.Get();

        return false;
    }

    [HarmonyPatch(nameof(Recorder.CreateLocalVoiceAudioAndSource))]
    private static bool Prefix(Recorder __instance)
    {
        __instance.SourceType   = Recorder.InputSourceType.Factory;
        __instance.InputFactory = () => VoiceManager.Get();

        return true;
    }
}