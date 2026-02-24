using HarmonyLib;
using UnityEngine.Video;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(VODPlayer))]
public class GTvVodPlayerPatches
{
    private const string ForcedVideoURL = "https://files.hamburbur.org/hamburbur-screensaver.mp4";

    [HarmonyPostfix]
    [HarmonyPatch("OnEnable")]
    public static void OnEnable_Postfix(VODPlayer __instance)
    {
        VODPlayer.state = VODPlayer.State.RUNNING;

        __instance.StartVideoPlayback(ForcedVideoURL);
    }

    [HarmonyPrefix]
    [HarmonyPatch("Player_loopPointReached")]
    public static bool PlayerLoopPointReachedPrefix(VODPlayer __instance, VideoPlayer source)
    {
        __instance.StartVideoPlayback(ForcedVideoURL);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("IGorillaSliceableSimple.SliceUpdate")]
    public static bool SliceUpdatePrefix(VODPlayer __instance)
    {
        if (VODPlayer.state != VODPlayer.State.RUNNING)
            VODPlayer.state = VODPlayer.State.RUNNING;

        if (__instance.player != null && __instance.player.isPlaying)
            __instance.PositionAudio();
        else if (!__instance.playerBusy)
            __instance.StartVideoPlayback(ForcedVideoURL);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("StartPlayback")]
    public static bool StartPlaybackPrefix(VODPlayer __instance, ref VODPlayer.VODStream str, ref double time)
    {
        __instance.StartVideoPlayback(ForcedVideoURL);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlayPreviouStream")]
    public static bool PlayPreviouStreamPrefix(VODPlayer __instance)
    {
        __instance.StartVideoPlayback(ForcedVideoURL);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("NextStream")]
    public static bool NextStreamPrefix(ref VODPlayer.VODNextStream __result)
    {
        __result = null;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("StartImagePlayback")]
    public static bool StartImagePlaybackPrefix(VODPlayer __instance)
    {
        __instance.StartVideoPlayback(ForcedVideoURL);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("StartVideoPlayback")]
    public static void StartVideoPlaybackPrefix(VODPlayer __instance)
    {
        if (__instance.player != null)
            __instance.player.aspectRatio = VideoAspectRatio.Stretch;
    }
}