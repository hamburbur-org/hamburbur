using System;
using System.Collections.Generic;
using GorillaLocomotion;
using hamburbur.Components;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Tools;

public class RigUtils : Singleton<RigUtils>
{
    public static List<VRRig>                LoadedRigs    = [];
    public static Dictionary<VRRig, Vector3> RigPositions  = [];
    public static Dictionary<VRRig, Vector3> RigVelocities = [];

    public static Action<VRRig> OnRigLoaded;
    public static Action<VRRig> OnRigUnloaded;
    public static Action<VRRig> OnRigCosmeticsLoaded;
    public static Action<VRRig> OnMatIndexChange;

    public static Action<VRRig, Color> OnRigColourChanged;

    public static bool       IsRigEnabled = true;
    public static Vector3    RigPosition;
    public static Quaternion RigRotation;

    private GameObject rBall, lBall;

    private void Start()
    {
        rBall = CreateBall(Utils.RealRightController);
        lBall = CreateBall(Utils.RealLeftController);
    }

    private void Update()
    {
        if (UseRigManager.IsEnabled)
        {
            VRRig.LocalRig.enabled = IsRigEnabled;

            if (!IsRigEnabled)
            {
                VRRig.LocalRig.transform.position = RigPosition;
                VRRig.LocalRig.transform.rotation = RigRotation;
            }
        }

        bool showControllerBalls = !IsRigEnabled;

        rBall.SetActive(showControllerBalls);
        lBall.SetActive(showControllerBalls);

        foreach (VRRig rig in LoadedRigs)
        {
            if (rig == null)
                continue;

            if (!RigPositions.TryGetValue(rig, out Vector3 position))
                position = rig.transform.position;

            RigPositions[rig]  = rig.transform.position;
            RigVelocities[rig] = (rig.transform.position - position) / Time.deltaTime;
        }
    }

    public static void ToggleRig(bool toggled) => ToggleRig(toggled, VRRig.LocalRig.transform.position);

    public static void ToggleRig(bool toggled, Vector3 rigPosition) =>
            ToggleRig(toggled, rigPosition, VRRig.LocalRig.transform.rotation);

    private static void ToggleRig(bool toggled, Vector3 rigPosition, Quaternion rigRotation)
    {
        if (!UseRigManager.IsEnabled)
        {
            VRRig.LocalRig.enabled            = toggled;
            VRRig.LocalRig.transform.position = rigPosition;
            VRRig.LocalRig.transform.rotation = rigRotation;
        }

        IsRigEnabled = toggled;
        RigPosition  = rigPosition;
        RigRotation  = rigRotation;
    }

    public static void EnableLowGravity()  => Utils.OnFixedUpdate += LowGravityFixed;
    public static void DisableLowGravity() => Utils.OnFixedUpdate -= LowGravityFixed;

    private static void LowGravityFixed() =>
            GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity / 2f * GorillaTagger.Instance.rigidbody.mass);

    public static void EnableZeroGravity()  => Utils.OnFixedUpdate += ZeroGravityFixed;
    public static void DisableZeroGravity() => Utils.OnFixedUpdate -= ZeroGravityFixed;

    private static void ZeroGravityFixed() =>
            GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity * GorillaTagger.Instance.rigidbody.mass);

    public static void EnableHighGravity()  => Utils.OnFixedUpdate += HighGravityFixed;
    public static void DisableHighGravity() => Utils.OnFixedUpdate -= HighGravityFixed;

    private static void HighGravityFixed() =>
            GorillaTagger.Instance.rigidbody.AddForce(Physics.gravity * GorillaTagger.Instance.rigidbody.mass);

    public static void EnableReverseGravity()
    {
        Utils.OnFixedUpdate                             += ReverseGravityFixed;
        GTPlayer.Instance.turnParent.transform.rotation =  Quaternion.Euler(180f, 0f, 0f);
    }

    public static void DisableReverseGravity()
    {
        Utils.OnFixedUpdate -= ReverseGravityFixed;
        FixRigRotations();
    }

    private static void ReverseGravityFixed() =>
            GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity * (2f * GorillaTagger.Instance.rigidbody.mass));

    public static void FixRigRotations()
    {
        Quaternion localRot = GTPlayer.Instance.turnParent.transform.rotation;

        if (localRot == Quaternion.Euler(0f, 0f, 0f))
            return;

        GTPlayer.Instance.turnParent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private static GameObject CreateBall(Transform parent)
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        if (ball.TryGetComponent(out Renderer rend))
        {
            rend.material.shader = Shaders.UberShader;
            rend.material.color  = Plugin.Instance.MainColour;
        }

        if (ball.TryGetComponent(out SphereCollider coll))
            coll.Obliterate();

        ball.transform.SetParent(parent);
        ball.transform.localPosition = Vector3.zero;
        ball.transform.localRotation = Quaternion.identity;
        ball.transform.localScale    = Vector3.one * 0.1f;

        return ball;
    }
}