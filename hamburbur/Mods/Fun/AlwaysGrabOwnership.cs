using hamburbur.Mod_Backend;
using HarmonyLib;

namespace hamburbur.Mods.Fun;

[hamburburmod(                "Always Grab Ownership", "Always gives you ownership of the grabs when grabbing people",
        ButtonType.Togglable, AccessSetting.Public,    EnabledType.Enabled, 0)]
public class AlwaysGrabOwnership : hamburburmod
{
    public static      bool IsEnabled;
    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(TakeMyHand_HandLink), nameof(TakeMyHand_HandLink.Write))]
public static class AlwaysGrabOwnershipPatch
{
    private static bool Prefix(
            TakeMyHand_HandLink __instance,
            out bool            isGroundedHand,
            out bool            isGroundedButt,
            out int             grabbedPlayerActorNumber,
            out bool            grabbedHandIsLeft)
    {
        if (!AlwaysGrabOwnership.IsEnabled)
        {
            isGroundedHand = __instance.isGroundedHand;
            isGroundedButt = __instance.isGroundedButt;

            if (__instance.grabbedPlayer != null)
            {
                grabbedPlayerActorNumber = __instance.grabbedPlayer.ActorNumber;
                grabbedHandIsLeft        = __instance.grabbedHandIsLeft;
            }
            else
            {
                grabbedPlayerActorNumber = 0;
                grabbedHandIsLeft        = false;
            }

            return true;
        }

        isGroundedHand = true;
        isGroundedButt = false;

        if (__instance.grabbedPlayer != null)
        {
            grabbedPlayerActorNumber = __instance.grabbedPlayer.ActorNumber;
            grabbedHandIsLeft        = __instance.grabbedHandIsLeft;
        }
        else
        {
            grabbedPlayerActorNumber = 0;
            grabbedHandIsLeft        = false;
        }

        return false;
    }
}