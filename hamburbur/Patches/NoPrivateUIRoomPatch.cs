using HarmonyLib;
using UnityEngine;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(PrivateUIRoom), nameof(PrivateUIRoom.StartOverlay))]
public static class NoPrivateUIRoomPatch
{
    private static void Postfix()
    {
        PrivateUIRoom.StopOverlay();
        PrivateUIRoom.StopForcedOverlay();
        GameObject.Find("Miscellaneous Scripts/PrivateUIRoom_HandRays").SetActive(false);
    }
}