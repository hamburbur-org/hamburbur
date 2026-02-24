using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.OP;

[hamburburmod(
        "Jman Annoy Gun",
        "Spaz your rig around someone whilst playing the jman sounds",
        ButtonType.Togglable,
        AccessSetting.Public,
        EnabledType.Disabled,
        0
)]
public class JmanAnnoyGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null)
        {
            if (!RigUtils.IsRigEnabled)
                RigUtils.ToggleRig(true);

            return;
        }

        if (RigUtils.IsRigEnabled)
            RigUtils.ToggleRig(false);

        Vector3 position = gunLib.ChosenRig.transform.position + Tools.Utils.RandomVector3();

        VRRig.LocalRig.transform.position = position;
        VRRig.LocalRig.transform.LookAt(gunLib.ChosenRig.transform.position);

        VRRig.LocalRig.head.rigTarget.transform.rotation = Tools.Utils.RandomQuaternion();
        VRRig.LocalRig.leftHand.rigTarget.transform.position =
                gunLib.ChosenRig.transform.position + Tools.Utils.RandomVector3();

        VRRig.LocalRig.rightHand.rigTarget.transform.position =
                gunLib.ChosenRig.transform.position + Tools.Utils.RandomVector3();

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation  = Tools.Utils.RandomQuaternion();
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Tools.Utils.RandomQuaternion();

        SoundSpam.PlaySound(Random.Range(336, 338));
    }

    protected override void OnDisable() => gunLib.OnDisable();
}