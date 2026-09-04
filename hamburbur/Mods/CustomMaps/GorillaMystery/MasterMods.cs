using hamburbur.Libs;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.GorillaMystery;

[hamburburmod(            "[Master] Spawn Sheriff Pistol", "Drops the sheriff pistol at your current position",
        ButtonType.Fixed, AccessSetting.Public,            EnabledType.Disabled, 0)]
public class SpawnSheriffPistol : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        if (!MysteryTagNetwork.RequireMasterClient() || GorillaTagger.Instance?.bodyCollider == null)
            return;

        Vector3 position = GorillaTagger.Instance.bodyCollider.transform.position;
        MysteryTagNetwork.Send(
                MysteryTagEvents.DropGun,
                (double)position.x,
                (double)position.y,
                (double)position.z);
    }
}

public abstract class MysteryRoleGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };
    private          bool   wasShooting;

    protected abstract void ApplyRole(VRRig rig);

    protected override void Start()
    {
        MysteryTagState.EnsureInitialized();
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting && gunLib.ChosenRig != null;
        if (isShooting && !wasShooting && MysteryTagNetwork.RequireMasterClient())
            ApplyRole(gunLib.ChosenRig);

        wasShooting = isShooting;
    }

    protected override void OnDisable()
    {
        wasShooting = false;
        gunLib.OnDisable();
    }
}

[hamburburmod(                "[Master] Make Murderer Gun", "Assigns the selected player as the murderer",
        ButtonType.Togglable, AccessSetting.Public,         EnabledType.Disabled, 0)]
public class MakeMurdererGun : MysteryRoleGun
{
    protected override void ApplyRole(VRRig rig)
    {
        if (rig?.Creator != null)
            MysteryTagNetwork.Send(MysteryTagEvents.ChooseMurder, (double)rig.Creator.ActorNumber);
    }
}

[hamburburmod(                "[Master] Make Sheriff Gun", "Drops the current pistol and assigns it to the selected player",
        ButtonType.Togglable, AccessSetting.Public,        EnabledType.Disabled, 0)]
public class MakeSheriffGun : MysteryRoleGun
{
    protected override void ApplyRole(VRRig rig)
    {
        if (rig?.Creator == null)
            return;

        Vector3 position = rig.transform.position;
        MysteryTagNetwork.Send(
                MysteryTagEvents.DropGun,
                (double)position.x,
                (double)position.y,
                (double)position.z);

        MysteryTagNetwork.Send(MysteryTagEvents.ChooseSheriff, (double)rig.Creator.ActorNumber);
    }
}