using System.Collections.Generic;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Disable Forest Colliders", "Removes the fuckass colliders in forest.", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class DisableForestColliders : hamburburmod
{
    public static bool WindDisabled;

    private readonly List<GameObject> forestColliders = [];

    protected override void OnEnable()
    {
        WindDisabled = true;

        Transform forestCollisions = GameObject
                                    .Find(
                                             "Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly")
                                    .transform;

        if (forestCollisions == null)
            return;

        for (int i = 2; i < 4; i++)
        {
            GameObject c = forestCollisions.transform.GetChild(i).gameObject;
            c.SetActive(false);
            forestColliders.Add(c);
        }
    }

    protected override void OnDisable()
    {
        WindDisabled = false;

        foreach (GameObject c in forestColliders)
            c.SetActive(true);

        forestColliders.Clear();
    }
}

[HarmonyPatch(typeof(ForceVolume), "SliceUpdate")]
public static class WindPatch
{
    private static bool Prefix(ForceVolume __instance)
    {
        bool enabled = !DisableForestColliders.WindDisabled;

        if (__instance.audioSource != null)
            __instance.audioSource.enabled = enabled;

        if (__instance.volume != null)
            __instance.volume.enabled = enabled;

        return enabled;
    }
}