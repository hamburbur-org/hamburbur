using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Griddy", "Makes you do the right foot creep, ooh, walkin' with that heater", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class Griddy : hamburburmod
{
    protected override void Update()
    {
        if (!InputManager.Instance.RightPrimary.IsPressed)
        {
            if (!RigUtils.IsRigEnabled)
                RigUtils.ToggleRig(true);

            return;
        }

        RigUtils.ToggleRig(false);

        Vector3 bodyOffset = VRRig.LocalRig.transform.forward * (5f * Time.deltaTime);
        RigUtils.RigPosition                             += bodyOffset;
        VRRig.LocalRig.head.rigTarget.transform.rotation =  VRRig.LocalRig.transform.rotation;

        VRRig.LocalRig.leftHand.rigTarget.transform.position = VRRig.LocalRig.transform.position       +
                                                               VRRig.LocalRig.transform.right * -0.33f +
                                                               VRRig.LocalRig.transform.forward *
                                                               (0.5f * Mathf.Cos(Time.frameCount / 10f)) +
                                                               VRRig.LocalRig.transform.up *
                                                               (-0.5f * Mathf.Abs(Mathf.Sin(Time.frameCount / 10f)));

        VRRig.LocalRig.rightHand.rigTarget.transform.position = VRRig.LocalRig.transform.position      +
                                                                VRRig.LocalRig.transform.right * 0.33f +
                                                                VRRig.LocalRig.transform.forward *
                                                                (0.5f * Mathf.Cos(Time.frameCount / 10f)) +
                                                                VRRig.LocalRig.transform.up *
                                                                (-0.5f * Mathf.Abs(Mathf.Sin(Time.frameCount / 10f)));

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation  = VRRig.LocalRig.transform.rotation;
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = VRRig.LocalRig.transform.rotation;

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation *=
                Quaternion.Euler(VRRig.LocalRig.leftHand.trackingRotationOffset);

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation *=
                Quaternion.Euler(VRRig.LocalRig.rightHand.trackingRotationOffset);
    }
}