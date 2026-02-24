using BepInEx;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Mods.Movement.Walker;

[hamburburmod("PC Walking", "It lets you walk on the PC (almost like WS)", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class WalkerMovement : hamburburmod
{
    private const float TargetGroundDistanceStanding  = 0.8f;
    private const float TargetGroundDistanceCrouching = 0.6f;
    private const float SpringStrength                = 50f;
    private const float Damping                       = 5f;
    private       float pitch;

    private float yaw;

    protected override void Update()
    {
        Vector3 movement      = Vector3.zero;
        float   movementSpeed = 500f * Time.deltaTime;
        if (UnityInput.Current.GetKey(KeyCode.LeftShift))
            movementSpeed *= 3f;

        if (UnityInput.Current.GetKey(KeyCode.W)) movement += VRRig.LocalRig.bodyRenderer.transform.forward;
        if (UnityInput.Current.GetKey(KeyCode.S)) movement -= VRRig.LocalRig.bodyRenderer.transform.forward;
        if (UnityInput.Current.GetKey(KeyCode.A)) movement -= VRRig.LocalRig.bodyRenderer.transform.right;
        if (UnityInput.Current.GetKey(KeyCode.D)) movement += VRRig.LocalRig.bodyRenderer.transform.right;

        movement.Normalize();
        movement *= movementSpeed;

        Rigidbody rb = GorillaTagger.Instance.rigidbody;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta  = Mouse.current.delta.ReadValue();
            float   sensitivity = 6f;

            yaw   += mouseDelta.x * sensitivity * Time.deltaTime;
            pitch -= mouseDelta.y * sensitivity * Time.deltaTime;
            pitch =  Mathf.Clamp(pitch, -89f, 89f);

            GorillaTagger.Instance.headCollider.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            Cursor.lockState                                       = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    protected override void FixedUpdate()
    {
        if (!IsPlayerGrounded(out float groundDistance))
            return;

        float currentGroundDistance = UnityInput.Current.GetKey(KeyCode.LeftControl)
                                              ? TargetGroundDistanceCrouching
                                              : TargetGroundDistanceStanding;

        currentGroundDistance += Mathf.Sin(Time.time * 10f) * 0.01f;

        Rigidbody rb         = GorillaTagger.Instance.rigidbody;
        float     difference = groundDistance - currentGroundDistance;

        float verticalVelocity = Vector3.Dot(rb.linearVelocity, Vector3.up);
        float forceAmount      = -difference * SpringStrength - verticalVelocity * Damping;

        if (UnityInput.Current.GetKey(KeyCode.Space))
            forceAmount = 100f;

        rb.AddForce(Vector3.up * forceAmount, ForceMode.Acceleration);
    }

    protected override void OnEnable()
    {
        Vector3 euler = GorillaTagger.Instance.headCollider.transform.eulerAngles;
        yaw   = euler.y;
        pitch = euler.x;

        HandAnimator leftHandAnimator = GTPlayer.Instance.leftHand.controllerTransform.AddComponent<HandAnimator>();
        leftHandAnimator.Body         = GTPlayer.Instance.bodyCollider.transform;
        leftHandAnimator.TerrainLayer = GTPlayer.Instance.locomotionEnabledLayers;
        leftHandAnimator.FootSpacing  = -0.3f;
        leftHandAnimator.IsLeftHand   = true;

        HandAnimator rightHandAnimator = GTPlayer.Instance.rightHand.controllerTransform.AddComponent<HandAnimator>();
        rightHandAnimator.Body         = GTPlayer.Instance.bodyCollider.transform;
        rightHandAnimator.TerrainLayer = GTPlayer.Instance.locomotionEnabledLayers;
        rightHandAnimator.FootSpacing  = 0.3f;
        rightHandAnimator.IsLeftHand   = false;

        leftHandAnimator.OtherHandAnimator  = rightHandAnimator;
        rightHandAnimator.OtherHandAnimator = leftHandAnimator;
    }

    protected override void OnDisable()
    {
        GTPlayer.Instance.leftHand.controllerTransform.GetComponent<HandAnimator>().Obliterate();
        GTPlayer.Instance.rightHand.controllerTransform.GetComponent<HandAnimator>().Obliterate();
    }

    private bool IsPlayerGrounded(out float groundDistance)
    {
        bool isGrounded = Physics.Raycast(GTPlayer.Instance.bodyCollider.transform.position, Vector3.down,
                out RaycastHit hit, 1f, GTPlayer.Instance.locomotionEnabledLayers);

        groundDistance = isGrounded ? hit.distance : 0f;

        return isGrounded;
    }
}