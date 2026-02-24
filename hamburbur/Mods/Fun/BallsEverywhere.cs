using System.Linq;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod("Snowballs Everywhere", "Grab snowballs everywhere", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class BallsEverywhere : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(SnowballMaker), nameof(SnowballMaker.PostTick))]
public static class BallsEverywherePatch
{
    private static bool Prefix(SnowballMaker __instance)
    {
        if (!BallsEverywhere.IsEnabled)
            return true;

        if (__instance.snowballs == null || __instance.snowballs.Length == 0 || !EquipmentInteractor.hasInstance ||
            !GorillaTagger.hasInstance   || GorillaTagger.Instance?.offlineVRRig == null)
            return false;

        GTPlayer player = GTPlayer.Instance;

        if (player == null) return false;

        EquipmentInteractor eq = EquipmentInteractor.instance;
        bool isGrabbing = __instance.isLeftHand ? eq.isLeftGrabbing : eq.isRightGrabbing;
        bool holding = __instance.isLeftHand ? eq.leftHandHeldEquipment != null : eq.rightHandHeldEquipment != null;

        if (!isGrabbing || holding) return false;

        SnowballThrowable active = __instance.snowballs.FirstOrDefault(t => t.gameObject.activeSelf);

        bool otherControllerInvalid = __instance.isLeftHand
                                              ? !ConnectedControllerHandler.Instance.RightValid
                                              : !ConnectedControllerHandler.Instance.LeftValid;

        if (active is GrowingSnowballThrowable grow &&
            (!GrowingSnowballThrowable.twoHandedSnowballGrowing || otherControllerInvalid))
        {
            grow.IncreaseSize(1);
            GorillaTagger.Instance.StartVibration(__instance.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 8f,
                    GorillaTagger.Instance.tapHapticDuration * 0.5f);

            __instance.requiresFreshMaterialContact = true;

            return false;
        }

        Transform handTransform = __instance.handTransform ??
                                  (__instance.isLeftHand
                                           ? GorillaTagger.Instance.offlineVRRig.myBodyDockPositions.leftHandTransform
                                           : GorillaTagger.Instance.offlineVRRig.myBodyDockPositions
                                                          .rightHandTransform);

        foreach (SnowballThrowable snowball in __instance.snowballs)
        {
            snowball.SetSnowballActiveLocal(true);
            snowball.velocityEstimator = __instance.velocityEstimator;

            snowball.transform.position = handTransform.position;
            snowball.transform.rotation = handTransform.rotation;

            GorillaTagger.Instance.StartVibration(__instance.isLeftHand,
                    GorillaTagger.Instance.tapHapticStrength * 0.5f, GorillaTagger.Instance.tapHapticDuration * 0.5f);

            __instance.requiresFreshMaterialContact = true;

            break;
        }

        return false;
    }
}