using hamburbur.Mod_Backend;
using UnityEngine;
using GorillaLocomotion;

namespace hamburbur.Mods.Movement;

[hamburburmod("Bark Fly", "Fly like the famous Bark mod menu.",
    ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class BarkFly : hamburburmod
{
    protected override void FixedUpdate()
    {
        GTPlayer.Instance.AddForce(-Physics.gravity, ForceMode.Acceleration);

        Vector2 xz = ControllerInputPoller.instance.leftControllerPrimary2DAxis;
        float y = ControllerInputPoller.instance.rightControllerPrimary2DAxis.y;

        var inputDirection = new Vector3(xz.x, y, xz.y);
        
        var playerForward = GTPlayer.Instance.bodyCollider.transform.forward;
        playerForward.y = 0;
        
        var playerRight = GTPlayer.Instance.bodyCollider.transform.right;
        playerRight.y = 0;

        var velocity =
            inputDirection.x * playerRight +
            y * Vector3.up +
            inputDirection.z * playerForward;
        velocity *= hamburbur.Mods.Settings.ChangeFlySpeed.Instance.IncrementalValue;
        GorillaTagger.Instance.rigidbody.velocity = Vector3.Lerp(GorillaTagger.Instance.rigidbody.velocity, velocity, 0.125f);
    }
}