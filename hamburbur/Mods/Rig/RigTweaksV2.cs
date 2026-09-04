using System;
using BepInEx;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Rig Tweaks V2", "Rig go weeeeeeeee", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RigTweaksV2 : hamburburmod
{
    public static bool       IsEnabled;
    public static Quaternion RigRotation = Quaternion.identity;

    protected override void Update()
    {
        if (!IsEnabled)
            return;

        float speed = 90f;

        if (UnityInput.Current.GetKey(KeyCode.UpArrow) || InputManager.Instance.LeftJoystick.Axis.y > 0.5f)
            RigRotation *= Quaternion.Euler(speed * Time.deltaTime, 0, 0);

        if (UnityInput.Current.GetKey(KeyCode.DownArrow) || InputManager.Instance.LeftJoystick.Axis.y < -0.5f)
            RigRotation *= Quaternion.Euler(-speed * Time.deltaTime, 0, 0);

        if (UnityInput.Current.GetKey(KeyCode.LeftArrow) || InputManager.Instance.LeftJoystick.Axis.x < -0.5f)
            RigRotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);

        if (UnityInput.Current.GetKey(KeyCode.RightArrow) || InputManager.Instance.LeftJoystick.Axis.x > 0.5f)
            RigRotation *= Quaternion.Euler(0, -speed * Time.deltaTime, 0);

        if (UnityInput.Current.GetKey(KeyCode.RightControl) || InputManager.Instance.RightJoystick.Axis.x > 0.5f)
            RigRotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime);

        if (UnityInput.Current.GetKey(KeyCode.RightShift) || InputManager.Instance.RightJoystick.Axis.x < -0.5f)
            RigRotation *= Quaternion.Euler(0, 0, -speed * Time.deltaTime);

        if (UnityInput.Current.GetKeyDown(KeyCode.R))
            RigRotation = Quaternion.identity;
    }

    protected override void OnEnable() => IsEnabled = true;

    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class RigTweaksV2_LateUpdatePatch
{
    private static void Postfix(VRRig __instance)
    {
        try
        {
            if (!RigTweaksV2.IsEnabled)
                return;

            if (!__instance.IsLocalRig())
                return;

            __instance.transform.rotation =
                    GorillaTagger.Instance.headCollider.transform.rotation * RigTweaksV2.RigRotation;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LateUpdate postfix error: {e}");
        }
    }
}