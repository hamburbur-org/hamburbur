using System;
using System.Collections.Generic;
using GorillaLocomotion;
using hamburbur.Components;
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

    private void Update()
    {
        VRRig.LocalRig.enabled = IsRigEnabled;
        if (!IsRigEnabled)
        {
            VRRig.LocalRig.transform.position = RigPosition;
            VRRig.LocalRig.transform.rotation = RigRotation;
        }

        foreach (VRRig rig in LoadedRigs)
        {
            if (!RigPositions.TryGetValue(rig, out Vector3 position))
                position = rig.transform.position;

            RigPositions[rig]  = rig.transform.position;
            RigVelocities[rig] = (rig.transform.position - position) / Time.deltaTime;
        }
    }

    public static void ToggleRig(bool toggled) => ToggleRig(toggled, VRRig.LocalRig.transform.position);

    public static void ToggleRig(bool toggled, Vector3 rigPosition) =>
            ToggleRig(toggled, rigPosition, VRRig.LocalRig.transform.rotation);

    public static void ToggleRig(bool toggled, Vector3 rigPosition, Quaternion rigRotation)
    {
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
}