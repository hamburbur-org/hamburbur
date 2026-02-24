using System;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod(                "ZlothY's RecRoomRig", "Better and worse RRR", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class ZlothYRecRoomRig : hamburburmod
{
    private static float smoothedYaw;

    private readonly float  behindFloat = 0.18f;
    public override  Type[] IncompatibleMods => [typeof(RecRoomRig),];

    protected override void Update()
    {
        if (!Tools.Utils.InVR || GTPlayer.Instance == null || GTPlayer.Instance.headCollider == null)
            return;

        float bodyYRot = CalculateBodyYRotation(Time.deltaTime);

        Transform headCol = GTPlayer.Instance.headCollider.transform;
        Vector3   ogEuler = headCol.transform.eulerAngles;

        float clampedPitch = Mathf.Clamp(ogEuler.x > 180f ? ogEuler.x - 360f : ogEuler.x, -90f, 90f);
        float clampedRoll  = Mathf.Clamp(ogEuler.z > 180f ? ogEuler.z - 360f : ogEuler.z, -90f, 90f);

        headCol.rotation = Quaternion.Euler(clampedPitch, bodyYRot, clampedRoll);
    }

    private float CalculateBodyYRotation(float deltaTime)
    {
        Transform camera    = GorillaTagger.Instance.mainCamera.transform;
        Transform leftHand  = GTPlayer.Instance.leftHand.controllerTransform;
        Transform rightHand = GTPlayer.Instance.rightHand.controllerTransform;

        Vector3 headForward   = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
        Vector3 handCenter    = (leftHand.position + rightHand.position) / 2f;
        Vector3 handDirection = Vector3.ProjectOnPlane(handCenter - camera.position, Vector3.up).normalized;

        bool leftBehind  = Vector3.Dot(leftHand.position  - camera.position, camera.forward) < 0;
        bool rightBehind = Vector3.Dot(rightHand.position - camera.position, camera.forward) < 0;

        float factor = leftBehind || rightBehind ? behindFloat : 1f;

        Vector3 blendedDir;

        if (leftBehind && rightBehind)
            blendedDir = headForward;
        else
            blendedDir = Vector3.Lerp(headForward, handDirection, factor).normalized;

        float targetYaw = Mathf.Atan2(blendedDir.x, blendedDir.z) * Mathf.Rad2Deg;

        smoothedYaw = SmoothYaw(smoothedYaw, targetYaw, deltaTime, 0.1f);

        return smoothedYaw;
    }

    private static float SmoothYaw(float previousYaw, float targetYaw, float deltaTime, float halfLifeSeconds)
    {
        float delta = Mathf.DeltaAngle(previousYaw, targetYaw);
        float decay = Mathf.Pow(0.5f, deltaTime / Mathf.Max(0.0001f, halfLifeSeconds));

        return previousYaw + delta * (1f - decay);
    }
}