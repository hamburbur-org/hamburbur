using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Shadow Clone Gun", "Copies a selected player's movement beside them in modded rooms",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class ShadowCloneGun : hamburburmod
{
    private const float SideOffset = 0.9f;

    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();
        
        VRRig target = gunLib.ChosenRig;

        if (!gunLib.IsShooting || target == null)
        {
            RestoreRig();
            return;
        }

        if (RigUtils.IsRigEnabled)
            RigUtils.ToggleRig(false);

        Vector3 offset = target.transform.right * SideOffset;

        RigUtils.RigPosition = target.transform.position + offset;
        RigUtils.RigRotation = target.transform.rotation;

        VRRig.LocalRig.head.rigTarget.rotation = target.head.rigTarget.rotation;

        VRRig.LocalRig.leftHand.rigTarget.position = target.leftHand.rigTarget.position + offset;
        VRRig.LocalRig.leftHand.rigTarget.rotation = target.leftHand.rigTarget.rotation;

        VRRig.LocalRig.rightHand.rigTarget.position = target.rightHand.rigTarget.position + offset;
        VRRig.LocalRig.rightHand.rigTarget.rotation = target.rightHand.rigTarget.rotation;

        VRRig.LocalRig.leftHand.calcT  = target.leftHand.calcT;
        VRRig.LocalRig.rightHand.calcT = target.rightHand.calcT;
    }

    protected override void OnDisable()
    {
        RestoreRig();
        gunLib.OnDisable();
    }

    private static void RestoreRig()
    {
        if (!RigUtils.IsRigEnabled)
            RigUtils.ToggleRig(true);
    }
}
