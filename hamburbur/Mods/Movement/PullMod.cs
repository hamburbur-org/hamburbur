using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Pull Mod", "A super sigma pull mod", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class PullMod : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
public static class PullModPatch
{
    private static bool wasLeftHandColliding;
    private static bool wasRightHandColliding;

    private static void Postfix(GTPlayer __instance)
    {
        if (!PullMod.IsEnabled)
            return;

        bool isLeftHandColliding  = __instance.leftHand.wasColliding;
        bool isRightHandColliding = __instance.rightHand.wasColliding;

        if (InputManager.Instance.RightGrip.IsPressed)
            if (isLeftHandColliding && !wasLeftHandColliding || isRightHandColliding && !wasRightHandColliding)
            {
                Vector3 velocity = GorillaTagger.Instance.rigidbody.linearVelocity;
                velocity.x *= ChangePullStrength.Instance.IncrementalValue * 0.1f;
                velocity.y =  0f;
                velocity.z *= ChangePullStrength.Instance.IncrementalValue * 0.1f;

                Vector3 startPos1 = __instance.transform.position;
                Vector3 startPos2 = __instance.leftHand.controllerTransform.position;
                Vector3 startPos3 = __instance.rightHand.controllerTransform.position;

                int  iterations = 0;
                bool shouldPull = true;

                while (Physics.Raycast(startPos1, velocity.normalized, velocity.magnitude,
                               GTPlayer.Instance.locomotionEnabledLayers) ||
                       Physics.Raycast(startPos2, velocity.normalized, velocity.magnitude,
                               GTPlayer.Instance.locomotionEnabledLayers) ||
                       Physics.Raycast(startPos3, velocity.normalized, velocity.magnitude,
                               GTPlayer.Instance.locomotionEnabledLayers))
                {
                    startPos1.y += 0.1f * ChangePullStrength.Instance.IncrementalValue * 0.1f;
                    startPos2.y += 0.1f * ChangePullStrength.Instance.IncrementalValue * 0.1f;
                    startPos3.y += 0.1f * ChangePullStrength.Instance.IncrementalValue * 0.1f;
                    iterations++;

                    if (iterations > 15)
                    {
                        shouldPull = false;

                        break;
                    }
                }

                if (shouldPull)
                {
                    Vector3 currentPos = __instance.transform.position;
                    currentPos                    += velocity;
                    currentPos.y                  =  startPos1.y;
                    __instance.transform.position =  currentPos;
                }
            }

        wasLeftHandColliding  = isLeftHandColliding;
        wasRightHandColliding = isRightHandColliding;
    }
}