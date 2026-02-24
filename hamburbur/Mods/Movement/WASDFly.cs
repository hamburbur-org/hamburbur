using BepInEx;
using hamburbur.Mod_Backend;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Mods.Movement;

[hamburburmod("WASD Fly", "Lets you fly around on PC with WASD", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class WASDFly : hamburburmod
{
    private const float MouseSensitivity = 0.08f;

    public static bool DisableMovement;

    protected override void FixedUpdate()
    {
        if (Tools.Utils.InVR)
            return;

        Transform head = GorillaTagger.Instance.headCollider.transform;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            head.Rotate(Vector3.up,    mouseDelta.x  * MouseSensitivity, Space.World);
            head.Rotate(Vector3.right, -mouseDelta.y * MouseSensitivity, Space.Self);

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Vector3 movementDirection = Vector3.zero;

        if (UnityInput.Current.GetKey(KeyCode.W)) movementDirection           += head.forward;
        if (UnityInput.Current.GetKey(KeyCode.S)) movementDirection           -= head.forward;
        if (UnityInput.Current.GetKey(KeyCode.A)) movementDirection           -= head.right;
        if (UnityInput.Current.GetKey(KeyCode.D)) movementDirection           += head.right;
        if (UnityInput.Current.GetKey(KeyCode.Space)) movementDirection       += head.up;
        if (UnityInput.Current.GetKey(KeyCode.LeftControl)) movementDirection -= head.up;

        Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;

        float speed = UnityInput.Current.GetKey(KeyCode.LeftShift) ? 40f : 10f;
        if (!DisableMovement)
            rigidbody.transform.position += movementDirection.normalized * (Time.fixedDeltaTime * speed);

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.AddForce(-Physics.gravity * rigidbody.mass);
    }

    protected override void OnDisable() => Cursor.lockState = CursorLockMode.None;
}