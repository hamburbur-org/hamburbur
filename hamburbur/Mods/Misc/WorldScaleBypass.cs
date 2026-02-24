using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;
using UnityEngine.XR;

namespace hamburbur.Mods.Misc;

[hamburburmod("World Scale Bypass", "You can bypass the world scale checks", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class WorldScaleBypass : hamburburmod
{
    private GameObject leftHandCube;

    private Vector3    leftHandPosition;
    private GameObject rightHandCube;
    private Vector3    rightHandPosition;

    protected override void LateUpdate()
    {
        Vector3 leftLocalPos  = ControllerInputPoller.DevicePosition(XRNode.LeftHand);
        Vector3 rightLocalPos = ControllerInputPoller.DevicePosition(XRNode.RightHand);

        Vector3 leftWorldPos  = GTPlayer.Instance.transform.TransformPoint(leftLocalPos);
        Vector3 rightWorldPos = GTPlayer.Instance.transform.TransformPoint(rightLocalPos);

        UpdateHandVisuals(leftWorldPos, rightWorldPos);

        Transform leftHandTransform  = GTPlayer.Instance.leftHand.controllerTransform;
        Transform rightHandTransform = GTPlayer.Instance.rightHand.controllerTransform;

        float   handsDistance   = Vector3.Distance(leftWorldPos, rightWorldPos);
        Vector3 handPositionAvg = (leftWorldPos + rightWorldPos) / 2;
        Vector3 topOfHead       = GTPlayer.Instance.transform.TransformPoint(-0.15f, 0.1f, 0f);

        float leftHandDistance  = Vector3.Distance(leftWorldPos,  topOfHead);
        float rightHandDistance = Vector3.Distance(rightWorldPos, topOfHead);
        float handDistanceAvg   = (leftHandDistance + rightHandDistance) / 2;

        Vector3 targetLeftHandPos  = leftWorldPos;
        Vector3 targetRightHandPos = rightWorldPos;

        if (handsDistance < 0.2f && handDistanceAvg < 0.4f && handPositionAvg.y >= topOfHead.y)
        {
            targetLeftHandPos  = topOfHead;
            targetRightHandPos = topOfHead;
        }

        const float lerpSpeed = 5f;
        leftHandPosition  = Vector3.Lerp(leftHandPosition,  targetLeftHandPos,  Time.deltaTime * lerpSpeed);
        rightHandPosition = Vector3.Lerp(rightHandPosition, targetRightHandPos, Time.deltaTime * lerpSpeed);

        leftHandTransform.position  = leftHandPosition;
        rightHandTransform.position = rightHandPosition;
    }

    protected override void OnEnable()  => SpawnHandVisuals();
    protected override void OnDisable() => DestroyHandVisuals();

    private void DestroyHandVisuals()
    {
        leftHandCube.Obliterate();
        rightHandCube.Obliterate();
    }

    private void SpawnHandVisuals()
    {
        Transform rigTransform = GTPlayer.Instance.transform;

        leftHandCube  = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightHandCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        leftHandCube.transform.SetParent(rigTransform, true);
        rightHandCube.transform.SetParent(rigTransform, true);

        leftHandCube.transform.localScale  = Vector3.one * 0.05f;
        rightHandCube.transform.localScale = Vector3.one * 0.05f;

        leftHandCube.GetComponent<Renderer>().material.color  = Color.green;
        rightHandCube.GetComponent<Renderer>().material.color = Color.green;

        leftHandCube.GetComponent<Renderer>().material.shader  = Shader.Find("GUI/Text Shader");
        rightHandCube.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

        leftHandCube.GetComponent<Collider>().Obliterate();
        rightHandCube.GetComponent<Collider>().Obliterate();
    }

    private void UpdateHandVisuals(Vector3 leftPos, Vector3 rightPos)
    {
        Transform rigTransform = GTPlayer.Instance.transform;

        if (leftHandCube != null)
            leftHandCube.transform.localPosition = rigTransform.InverseTransformPoint(leftPos);

        if (rightHandCube != null)
            rightHandCube.transform.localPosition = rigTransform.InverseTransformPoint(rightPos);
    }
}