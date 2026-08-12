using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.Misc;

[hamburburmod("Disable Quit Box", "Disables the quit box at the bottom of the map", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DisableQuitBox : hamburburmod
{
    public static bool IsEnabled;
    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(GorillaQuitBox), nameof(GorillaQuitBox.OnBoxTriggered))]
public static class QuitBoxPatch
{
    private static bool Prefix() => !DisableQuitBox.IsEnabled;
}