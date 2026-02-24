using Newtonsoft.Json.Linq;
using UnityEngine;

namespace hamburbur.Tools;

public struct RigTransform
{
    private RigTransform(Vector3    headPosition, Quaternion headRotation, Vector3 rigPosition, Quaternion rigRotation,
                         Vector3    leftHandPosition, Quaternion leftHandRotation, Vector3 rightHandPosition,
                         Quaternion rightHandRotation, Vector3 velocity)
    {
        HeadPosition = headPosition;
        HeadRotation = headRotation;

        RigPosition = rigPosition;
        RigRotation = rigRotation;

        LeftHandPosition = leftHandPosition;
        LeftHandRotation = leftHandRotation;

        RightHandPosition = rightHandPosition;
        RightHandRotation = rightHandRotation;

        Velocity = velocity;
    }

    public Vector3    HeadPosition;
    public Quaternion HeadRotation;

    public Vector3    RigPosition;
    public Quaternion RigRotation;

    public Vector3    LeftHandPosition;
    public Quaternion LeftHandRotation;

    public Vector3    RightHandPosition;
    public Quaternion RightHandRotation;

    public Vector3 Velocity;

    public static RigTransform GetRigPosition(VRRig rig) => new(rig.head.rigTarget.position,
            rig.head.rigTarget.rotation,
            rig.transform.position, rig.transform.rotation, rig.leftHand.rigTarget.position,
            rig.leftHand.rigTarget.rotation, rig.rightHand.rigTarget.position, rig.rightHand.rigTarget.rotation,
            rig.Velocity());

    public JObject ToJObject() => new()
    {
            ["headPosition"] = JObjectExtensions.FromVector3(HeadPosition),
            ["headRotation"] = JObjectExtensions.FromQuaternion(HeadRotation),

            ["rigPosition"] = JObjectExtensions.FromVector3(RigPosition),
            ["rigRotation"] = JObjectExtensions.FromQuaternion(RigRotation),

            ["leftHandPosition"] = JObjectExtensions.FromVector3(LeftHandPosition),
            ["leftHandRotation"] = JObjectExtensions.FromQuaternion(LeftHandRotation),

            ["rightHandPosition"] = JObjectExtensions.FromVector3(RightHandPosition),
            ["rightHandRotation"] = JObjectExtensions.FromQuaternion(RightHandRotation),

            ["velocity"] = JObjectExtensions.FromVector3(Velocity),
    };

    public static RigTransform FromJObject(JObject jObject) => new(
            JObjectExtensions.ToVector3((JObject)jObject["headPosition"]),
            JObjectExtensions.ToQuaternion((JObject)jObject["headRotation"]),
            JObjectExtensions.ToVector3((JObject)jObject["rigPosition"]),
            JObjectExtensions.ToQuaternion((JObject)jObject["rigRotation"]),
            JObjectExtensions.ToVector3((JObject)jObject["leftHandPosition"]),
            JObjectExtensions.ToQuaternion((JObject)jObject["leftHandRotation"]),
            JObjectExtensions.ToVector3((JObject)jObject["rightHandPosition"]),
            JObjectExtensions.ToQuaternion((JObject)jObject["rightHandRotation"]),
            JObjectExtensions.ToVector3((JObject)jObject["velocity"]));
}