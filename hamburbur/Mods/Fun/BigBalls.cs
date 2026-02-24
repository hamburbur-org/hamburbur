using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.Fun;

[hamburburmod("Big Snowballs", "Makes it so the snowballs are always big", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class BigBalls : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(GrowingSnowballThrowable), nameof(GrowingSnowballThrowable.OnEnable))]
public static class BigBallsPatch
{
    private static void Postfix(GrowingSnowballThrowable __instance)
    {
        if (BigBalls.IsEnabled)
            __instance.IncreaseSize(5);
    }
}