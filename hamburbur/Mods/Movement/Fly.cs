using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Fly", "Lets you fly around on VR", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Fly : hamburburmod
{
    private static int FlySpeed => ChangeFlySpeed.Instance.IncrementalValue;

    protected override void FixedUpdate()
    {
        if (!InputManager.Instance.LeftPrimary.IsPressed)
            return;

        GorillaTagger.Instance.rigidbody.linearVelocity +=
                GTPlayer.Instance.headCollider.transform.forward * (Time.deltaTime * (FlySpeed * 2));
    }
}