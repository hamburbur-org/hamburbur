using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Car", "Makes you drive around", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Car : hamburburmod
{
    private static Vector2 driveLerpDirection = Vector2.zero;

    protected override void Update()
    {
        Vector2 joy = InputManager.Instance.LeftJoystick.Axis;
        driveLerpDirection = Vector2.Lerp(driveLerpDirection, joy, 0.05f);

        Vector3 addition = GorillaTagger.Instance.bodyCollider.transform.forward * driveLerpDirection.y +
                           GorillaTagger.Instance.bodyCollider.transform.right   * driveLerpDirection.x;

        Physics.Raycast(GorillaTagger.Instance.bodyCollider.transform.position - new Vector3(0f, 0.2f, 0f),
                Vector3.down, out RaycastHit Ray, 512f, GTPlayer.Instance.locomotionEnabledLayers);

        Vector3 targetVelocity = addition * ChangeFlySpeed.Instance.IncrementalValue;

        if (Ray.distance < 0.2f && (Mathf.Abs(driveLerpDirection.x) > 0.05f || Mathf.Abs(driveLerpDirection.y) > 0.05f))
            GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(targetVelocity.x,
                    GorillaTagger.Instance.rigidbody.linearVelocity.y, targetVelocity.z);
    }
}