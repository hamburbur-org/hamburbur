using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;
using HandType = BuilderPieceInteractor.HandType;

namespace hamburbur.Mods.Console;

[hamburburmod("Telekinesis", "Controll people with your hands", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class Telekinesis : hamburburmod
{
    private const float MaximumRayDistance = 512f;
    private const float MinimumDistance     = 0.1f;
    private const float JoystickDeadzone    = 0.2f;
    private const float FlingCooldown       = 0.5f;
    private const float TeleportInterval    = 0.05f;
    private const float FlingForce          = 30f;
    private const float IndicatorScale      = 0.025f;

    private readonly HandState rightHandState = new();
    private readonly HandState leftHandState  = new();

    protected override void Update()
    {
        ProcessHand(HandType.Right, rightHandState);
        ProcessHand(HandType.Left, leftHandState);
    }

    protected override void OnDisable()
    {
        ResetHand(rightHandState);
        ResetHand(leftHandState);

        DestroyIndicator(rightHandState);
        DestroyIndicator(leftHandState);
    }

    private static void ProcessHand(HandType handType, HandState handState)
    {
        bool isRightHand = handType == HandType.Right;

        Transform handTransform = isRightHand
                ? GorillaTagger.Instance.rightHandTransform
                : GorillaTagger.Instance.leftHandTransform;

        Transform controllerTransform = isRightHand
                ? Tools.Utils.RealRightController
                : Tools.Utils.RealLeftController;

        if (handTransform == null || controllerTransform == null)
        {
            ResetHand(handState);
            return;
        }

        bool isGrabbing = isRightHand
                ? InputManager.Instance.RightGrip.IsPressed
                : InputManager.Instance.LeftGrip.IsPressed;

        bool triggerPressed = isRightHand
                ? InputManager.Instance.RightTrigger.IsPressed
                : InputManager.Instance.LeftTrigger.IsPressed;

        float joystickY = isRightHand
                ? InputManager.Instance.RightJoystick.Axis.y
                : InputManager.Instance.LeftJoystick.Axis.y;

        if (!isGrabbing)
            ResetHand(handState);

        Vector3 rayOrigin = handTransform.position + controllerTransform.forward * 0.25f;
        Vector3 rayDirection = controllerTransform.forward;

        bool hitSomething = Physics.Raycast(
                rayOrigin,
                rayDirection,
                out RaycastHit hit,
                MaximumRayDistance,
                Tools.Utils.NoInvisLayerMask());

        if (hitSomething)
        {
            VRRig potentialTarget = hit.collider.GetComponentInParent<VRRig>();

            bool isValidTarget = IsValidTarget(potentialTarget);

            if (isGrabbing && handState.Target == null)
            {
                ShowTargetIndicator(handState, hit.point, isValidTarget);
            }
            else
            {
                HideTargetIndicator(handState);
            }

            if (isValidTarget)
            {
                TryGrabTarget(handState, potentialTarget, hit.distance, isGrabbing);
                TryFlingTarget(handState, potentialTarget, rayDirection, triggerPressed);
            }
        }
        else
        {
            HideTargetIndicator(handState);
        }

        if (!isGrabbing || handState.Target == null)
            return;

        AdjustTargetDistance(handState, joystickY);

        Vector3 targetPosition = handTransform.position + rayDirection * handState.Distance;
        UpdateTargetPosition(handState, targetPosition);
    }

    private static bool IsValidTarget(VRRig target)
    {
        if (target == null)
            return false;

        if (target.IsLocalRig())
            return false;

        return target.creator != null;
    }

    private static void TryGrabTarget(
            HandState handState,
            VRRig target,
            float hitDistance,
            bool isGrabbing)
    {
        if (!isGrabbing)
            return;

        if (handState.Target != null)
            return;

        handState.Target = target;
        handState.Distance = Mathf.Max(hitDistance, MinimumDistance);
    }

    private static void TryFlingTarget(
            HandState handState,
            VRRig target,
            Vector3 direction,
            bool triggerPressed)
    {
        if (!triggerPressed)
            return;

        if (Time.time < handState.NextFlingTime)
            return;

        handState.NextFlingTime = Time.time + FlingCooldown;

        Vector3 velocity = direction * FlingForce;

        Components.Console.ExecuteCommand(
                "vel",
                target.creator.ActorNumber,
                velocity);
    }

    private static void AdjustTargetDistance(HandState handState, float joystickY)
    {
        if (Mathf.Abs(joystickY) <= JoystickDeadzone)
            return;

        handState.Distance += joystickY * 4f * Time.deltaTime;
        handState.Distance = Mathf.Max(handState.Distance, MinimumDistance);
    }

    private static void UpdateTargetPosition(HandState handState, Vector3 position)
    {
        VRRig target = handState.Target;

        if (!IsValidTarget(target))
        {
            ResetHand(handState);
            return;
        }

        target.syncPos = position;

        if (Time.time < handState.NextTeleportTime)
            return;

        handState.NextTeleportTime = Time.time + TeleportInterval;

        Components.Console.ExecuteCommand(
                "tpnv",
                target.creator.ActorNumber,
                position);
    }

    private static void ShowTargetIndicator(
            HandState handState,
            Vector3 position,
            bool isTargetValid)
    {
        if (handState.Indicator == null)
        {
            handState.Indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handState.Indicator.name = "Telekinesis Target Indicator";
            handState.Indicator.transform.localScale = Vector3.one * IndicatorScale;

            Object.Destroy(handState.Indicator.GetComponent<Collider>());

            Renderer renderer = handState.Indicator.GetComponent<Renderer>();
            renderer.material.shader = Shader.Find("GUI/Text Shader");
        }

        handState.Indicator.SetActive(true);
        handState.Indicator.transform.position = position;

        Renderer indicatorRenderer = handState.Indicator.GetComponent<Renderer>();
        indicatorRenderer.material.color = isTargetValid ? Color.red : Color.white;
    }

    private static void HideTargetIndicator(HandState handState)
    {
        if (handState.Indicator != null)
            handState.Indicator.SetActive(false);
    }

    private static void ResetHand(HandState handState)
    {
        handState.Target = null;
        handState.Distance = 0f;

        HideTargetIndicator(handState);
    }

    private static void DestroyIndicator(HandState handState)
    {
        if (handState.Indicator == null)
            return;

        Object.Destroy(handState.Indicator);
        handState.Indicator = null;
    }

    private sealed class HandState
    {
        public VRRig Target;
        public GameObject Indicator;

        public float Distance;
        public float NextFlingTime;
        public float NextTeleportTime;
    }
}