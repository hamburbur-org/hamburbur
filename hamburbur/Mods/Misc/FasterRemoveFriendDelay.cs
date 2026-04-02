using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace hamburbur.Mods.Misc;

[hamburburmod("Faster Remove Friend Delay", "Makes the remove friend delay at the friend station alot faster",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FasterRemoveFriendDelay : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()
    {
        IsEnabled = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToAllFriendDisplays();
    }

    protected override void OnDisable()
    {
        IsEnabled = false;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        ApplyToAllFriendDisplays(3f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) =>
            ApplyToAllFriendDisplays();

    // ReSharper disable Unity.PerformanceAnalysis
    private static void ApplyToAllFriendDisplays(float delay = 0.3f)
    {
        foreach (FriendDisplay display in Object.FindObjectsOfType<FriendDisplay>())
            Apply(display, delay);
    }

    public static void Apply(FriendDisplay display, float delay = 0.3f)
    {
        if (display == null || display._friendCardButtons == null)
            return;

        foreach (GorillaPressableDelayButton btn in display._friendCardButtons)
            if (btn != null)
                btn.delayTime = delay;
    }
}

[HarmonyPatch(typeof(FriendDisplay))]
public class DelayPatches
{
    [HarmonyPostfix]
    [HarmonyPatch("InitFriendCards")]
    private static void InitPatch(FriendDisplay __instance)
    {
        if (FasterRemoveFriendDelay.IsEnabled)
            FasterRemoveFriendDelay.Apply(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ToggleRemoveFriendMode")]
    private static void TogglePatch(FriendDisplay __instance)
    {
        if (FasterRemoveFriendDelay.IsEnabled)
            FasterRemoveFriendDelay.Apply(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("TriggerEntered")]
    private static void EnterPatch(FriendDisplay __instance)
    {
        if (FasterRemoveFriendDelay.IsEnabled)
            FasterRemoveFriendDelay.Apply(__instance);
    }
}