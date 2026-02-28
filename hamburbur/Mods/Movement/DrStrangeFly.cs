using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Dr.Strange Fly",     "Fly Like Dr.Strange",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DrStrangeFly : hamburburmod
{
    protected override void FixedUpdate()
    {
        if (!InputManager.Instance.LeftGrip.IsPressed ||
            !InputManager.Instance.RightGrip.IsPressed)
            return;

        Vector3 handR = GorillaTagger.Instance.rightHandTransform.position;
        Vector3 handL = GorillaTagger.Instance.leftHandTransform.position;

        GTPlayer.Instance.AddForce(
                (handR - handL) * (ChangeFlySpeed.Instance.IncrementalValue / 10f),
                ForceMode.VelocityChange);

        GTPlayer.Instance.AddForce(-Physics.gravity, ForceMode.Acceleration);
    }
}