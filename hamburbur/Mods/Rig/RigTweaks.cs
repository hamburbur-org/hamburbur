using System;
using System.Linq;
using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Rig Tweaks", "Rig go weeeeeeeee", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RigTweaks : hamburburmod
{
    public static bool       IsEnabled;
    public static Quaternion RigRotation = Quaternion.identity;

    protected override void Update()
    {
        if (!IsEnabled)
            return;

        float speed = 90f;

        if (UnityInput.Current.GetKey(KeyCode.UpArrow))
            RigRotation *= Quaternion.Euler(speed * Time.deltaTime, 0, 0);

        if (UnityInput.Current.GetKey(KeyCode.DownArrow))
            RigRotation *= Quaternion.Euler(-speed * Time.deltaTime, 0, 0);

        if (UnityInput.Current.GetKey(KeyCode.LeftArrow))
            RigRotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);

        if (UnityInput.Current.GetKey(KeyCode.RightArrow))
            RigRotation *= Quaternion.Euler(0, -speed * Time.deltaTime, 0);

        if (UnityInput.Current.GetKey(KeyCode.RightControl))
            RigRotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime);

        if (UnityInput.Current.GetKey(KeyCode.RightShift))
            RigRotation *= Quaternion.Euler(0, 0, -speed * Time.deltaTime);

        if (UnityInput.Current.GetKeyDown(KeyCode.R))
            RigRotation = Quaternion.identity;
    }

    protected override void OnEnable()
    {
        IsEnabled = true;

        GTPlayerTransform.UseNetRotation = true;
    }

    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class RigTweaks_LateUpdatePatch
{
    private static void Postfix(VRRig __instance)
    {
        try
        {
            if (!RigTweaks.IsEnabled)
                return;

            if (!__instance.isLocal)
                return;

            __instance.transform.rotation =
                    GorillaTagger.Instance.headCollider.transform.rotation * RigTweaks.RigRotation;

            __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
            __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
            __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);

            __instance.head.rigTarget.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LateUpdate postfix error: {e}");
        }
    }
}