using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Bark Fly",           "Fly like the famous Bark mod menu.",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class BarkFly : hamburburmod
{
    protected override void FixedUpdate()
    {
        GTPlayer.Instance.AddForce(-Physics.gravity, ForceMode.Acceleration);

        Vector2 xz = InputManager.Instance.LeftJoystick.Axis;
        float   y  = InputManager.Instance.RightJoystick.Axis.y;

        Vector3 inputDirection = new(xz.x, y, xz.y);

        Vector3 playerForward = GTPlayer.Instance.bodyCollider.transform.forward;
        playerForward.y = 0;

        Vector3 playerRight = GTPlayer.Instance.bodyCollider.transform.right;
        playerRight.y = 0;

        Vector3 velocity =
                inputDirection.x * playerRight +
                y                * Vector3.up  +
                inputDirection.z * playerForward;

        velocity *= ChangeFlySpeed.Instance.IncrementalValue;
        GorillaTagger.Instance.rigidbody.linearVelocity =
                Vector3.Lerp(GorillaTagger.Instance.rigidbody.linearVelocity, velocity, 0.125f);
    }
}