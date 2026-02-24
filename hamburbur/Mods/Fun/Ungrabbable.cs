using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.Fun;

[hamburburmod("Ungrabbable", "Makes you ungrabbable", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Ungrabbable : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(TakeMyHand_HandLink), nameof(TakeMyHand_HandLink.CanBeGrabbed))]
public static class UngrabbablePatch
{
    private static bool Prefix(ref bool __result)
    {
        if (!Ungrabbable.IsEnabled)
            return true;

        __result = false;

        return false;
    }
}