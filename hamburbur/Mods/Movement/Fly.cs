using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Fly", "Lets you fly around on VR", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Fly : hamburburmod
{
    private int FlySpeed => ChangeFlySpeed.Instance.IncrementalValue;

    protected override void FixedUpdate()
    {
        if (InputManager.Instance.RightTrigger.IsPressed)
            GorillaTagger.Instance.rigidbody.transform.position +=
                    GTPlayer.Instance.headCollider.transform.forward * (FlySpeed * Time.fixedDeltaTime);
        else if (InputManager.Instance.RightGrip.IsPressed)
            GorillaTagger.Instance.rigidbody.transform.position -=
                    GTPlayer.Instance.headCollider.transform.forward * (FlySpeed * Time.fixedDeltaTime);

        if (InputManager.Instance.LeftTrigger.IsPressed)
            GorillaTagger.Instance.rigidbody.transform.position +=
                    GTPlayer.Instance.bodyCollider.transform.up * (FlySpeed * Time.fixedDeltaTime);
        else if (InputManager.Instance.LeftGrip.IsPressed)
            GorillaTagger.Instance.rigidbody.transform.position -=
                    GTPlayer.Instance.bodyCollider.transform.up * (FlySpeed * Time.fixedDeltaTime);

        GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity * GorillaTagger.Instance.rigidbody.mass);
        GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
    }
}