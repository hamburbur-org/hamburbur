using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.OP;

[hamburburmod(
        "Fuck Off Group Joining",
        "Makes it so those little bitches can't kick you by forcing you to group join",
        ButtonType.Togglable,
        AccessSetting.Public,
        EnabledType.Disabled,
        0
)]
public class FuckOffGroupJoining : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(RoomSystem), nameof(RoomSystem.SearchForShuttle))]
public static class RoomSystemSearchForShuttlePatch
{
    private static bool Prefix(object[] shuffleData, PhotonMessageInfoWrapped info)
        => !FuckOffGroupJoining.IsEnabled;
}