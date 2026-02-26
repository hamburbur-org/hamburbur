using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;

namespace hamburbur.Mods.Rig;

[hamburburmod(                "Copy Movement Gun",  "Use a gun on people to copy their position", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class CopyMovementGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        if (!gunLib.IsShooting || gunLib.ChosenRig == null)
        {
            if (!RigUtils.IsRigEnabled)
                RigUtils.ToggleRig(true);
            
            return;
        }
        
        if (RigUtils.IsRigEnabled)
            RigUtils.ToggleRig(false);
        
        RigUtils.RigPosition = gunLib.ChosenRig.transform.position;
        RigUtils.RigRotation = gunLib.ChosenRig.transform.rotation;
        
        VRRig.LocalRig.head.rigTarget.transform.rotation = gunLib.ChosenRig.head.rigTarget.transform.rotation;
        
        VRRig.LocalRig.leftHand.rigTarget.transform.position = gunLib.ChosenRig.leftHand.rigTarget.transform.position;
        VRRig.LocalRig.leftHand.rigTarget.transform.rotation = gunLib.ChosenRig.leftHand.rigTarget.transform.rotation;
        
        VRRig.LocalRig.rightHand.rigTarget.transform.position = gunLib.ChosenRig.rightHand.rigTarget.transform.position;
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = gunLib.ChosenRig.rightHand.rigTarget.transform.rotation;
        
        VRRig.LocalRig.leftHand.calcT  = gunLib.ChosenRig.leftHand.calcT;
        VRRig.LocalRig.rightHand.calcT = gunLib.ChosenRig.rightHand.calcT;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}