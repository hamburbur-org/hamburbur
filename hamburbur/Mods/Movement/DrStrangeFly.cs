using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;
using GorillaLocomotion;

namespace hamburbur.Mods.Movement;

[hamburburmod("Dr.Strange Fly", "Fly Like Dr.Strange",
    ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DrStrangeFly : hamburburmod
{
    protected override void FixedUpdate()
    {
        if (ControllerInputPoller.instance.rightGrab &&
            ControllerInputPoller.instance.leftGrab)
        {
            Vector3 handR = GorillaTagger.Instance.rightHandTransform.position;
            Vector3 handL = GorillaTagger.Instance.leftHandTransform.position;

            GTPlayer.Instance.AddForce(
                (handR - handL) * (hamburbur.Mods.Settings.ChangeFlySpeed.Instance.IncrementalValue / 10f),
                ForceMode.VelocityChange);
            GTPlayer.Instance.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }
    }
}