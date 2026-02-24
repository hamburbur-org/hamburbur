using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Rig;

[hamburburmod(                "Look At Gun", "Stares at someone you shot", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class LookAtGun : hamburburmod
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
            return;

        VRRig.LocalRig.head.rigTarget.LookAt(gunLib.ChosenRig.head.rigTarget.transform);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}