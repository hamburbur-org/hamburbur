using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Dance", "Makes you boogie", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Dance : hamburburmod
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

        float t = Time.time;

        float sway   = Mathf.Sin(t * 2.2f)            * 0.35f;
        float bounce = Mathf.Abs(Mathf.Sin(t * 3.4f)) * 0.18f;
        float twist  = Mathf.Sin(t * 1.6f)            * 20f;

        Vector3 bodyOffset =
                GorillaTagger.Instance.bodyCollider.transform.right * sway +
                GorillaTagger.Instance.bodyCollider.transform.up    * bounce;

        RigUtils.RigPosition =
                GorillaTagger.Instance.bodyCollider.transform.position +
                new Vector3(0f, 0.15f, 0f)                             +
                bodyOffset;

        RigUtils.RigRotation =
                GorillaTagger.Instance.bodyCollider.transform.rotation *
                Quaternion.Euler(0f, twist, 0f);

        VRRig.LocalRig.head.rigTarget.transform.rotation =
                VRRig.LocalRig.transform.rotation;

        float armSwing = Mathf.Sin(t * 4f) * 0.25f;
        float armLift  = Mathf.Sin(t * 2f) * 0.2f;

        VRRig.LocalRig.leftHand.rigTarget.transform.position =
                VRRig.LocalRig.transform.position                      +
                VRRig.LocalRig.transform.forward * 0.25f               +
                VRRig.LocalRig.transform.right   * (-0.45f + armSwing) +
                VRRig.LocalRig.transform.up      * (0.35f  + armLift);

        VRRig.LocalRig.rightHand.rigTarget.transform.position =
                VRRig.LocalRig.transform.position                           +
                VRRig.LocalRig.transform.forward * 0.25f                    +
                VRRig.LocalRig.transform.right   * (0.45f + armSwing * -1f) +
                VRRig.LocalRig.transform.up      * (0.35f - armLift);

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation =
                VRRig.LocalRig.transform.rotation;

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation =
                VRRig.LocalRig.transform.rotation;

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation *=
                Quaternion.Euler(VRRig.LocalRig.leftHand.trackingRotationOffset);

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation *=
                Quaternion.Euler(VRRig.LocalRig.rightHand.trackingRotationOffset);
    }
}