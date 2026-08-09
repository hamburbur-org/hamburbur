using hamburbur.Managers;
using hamburbur.Mods.Misc;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(LckWallCameraSpawner), nameof(LckWallCameraSpawner.SpawnCamera))]
public static class LivCameraSpawnPatch
{
    private const float NotificationCooldown = 3f;

    private static float nextNotificationTime;

    private static bool Prefix()
    {
        if (!FirstPerson.isEnabled)
            return true;

        // ReSharper disable once InvertIf
        if (Time.unscaledTime >= nextNotificationTime)
        {
            NotificationManager.SendNotification(
                    "<color=grey>Info</color>",
                    "You cannot use LIV whilst the First Person mod is enabled!",
                    3f,
                    true,
                    true
            );

            nextNotificationTime = Time.unscaledTime + NotificationCooldown;
        }

        return false;
    }
}