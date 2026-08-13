using System.Linq;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.MecchaGorilla;

public abstract class MecchaTargetGun : hamburburmod
{
    protected readonly GunLib Gun = new() { ShouldFollow = true, };
    private float nextUse;
    protected virtual float Delay => 0.3f;

    protected override void Start()
    {
        MecchaState.EnsureInitialized();
        Gun.Start();
    }

    protected override void LateUpdate()
    {
        Gun.LateUpdate();
        if (!Gun.IsShooting || Gun.ChosenRig?.Creator == null || Time.time < nextUse)
            return;

        nextUse = Time.time + Delay;
        Use(Gun.ChosenRig);
    }

    protected abstract void Use(VRRig rig);

    protected override void OnDisable()
    {
        nextUse = 0f;
        Gun.OnDisable();
    }
}

[hamburburmod("Kill Gun", "Repeatedly finds a targeted painter with a short delay", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaKillGun : MecchaTargetGun
{
    protected override void Use(VRRig rig) => MecchaNetwork.Kill(rig.Creator.ActorNumber);
}

[hamburburmod("Kill Shot Gun", "Kills the target and broadcasts a coloured shotgun explosion", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaKillShotGun : MecchaTargetGun
{
    protected override void Use(VRRig rig)
    {
        Vector3 from = GorillaTagger.Instance?.bodyCollider != null
                ? GorillaTagger.Instance.bodyCollider.transform.position : rig.transform.position + Vector3.up;
        MecchaNetwork.Shot(from, rig.transform.position, true, MecchaNetwork.Rainbow());
        MecchaNetwork.Kill(rig.Creator.ActorNumber);
    }
}

[hamburburmod("Respawn Gun", "Repeatedly respawns the targeted painter", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaRespawnGun : MecchaTargetGun
{
    protected override void Use(VRRig rig) => MecchaNetwork.Respawn(rig.Creator.ActorNumber);
}

[hamburburmod("Whistle Gun", "Makes the targeted painter whistle", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaWhistleGun : MecchaTargetGun
{
    protected override void Use(VRRig rig) => MecchaNetwork.Whistle(rig.Creator.ActorNumber);
}

[hamburburmod("Splatter Gun", "Creates rainbow shotgun beams and splatters at the target", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaSplatterGun : MecchaTargetGun
{
    protected override float Delay => 0.2f;
    protected override void Use(VRRig rig)
    {
        Vector3 from = GunLib.GetGunHand() != null
                ? GunLib.GetGunHand().position : rig.transform.position + Vector3.up;
        MecchaNetwork.Shot(from, rig.transform.position, false, MecchaNetwork.Rainbow(0.35f));
    }
}

[hamburburmod("Explosion Gun", "Creates a coloured painter explosion at the target without killing", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaExplosionGun : MecchaTargetGun
{
    protected override void Use(VRRig rig)
    {
        Vector3 from = GunLib.GetGunHand() != null
                               ? GunLib.GetGunHand().position : rig.transform.position + Vector3.up;
        MecchaNetwork.Shot(from, rig.transform.position, true, MecchaNetwork.Rainbow(0.35f));
    }
}

[hamburburmod("Kill All", "Finds every other painter", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaKillAll : hamburburmod
{
    protected override void Pressed()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null) MecchaNetwork.Kill(rig.Creator.ActorNumber);
    }
}

[hamburburmod("Kill All Hiders", "Finds every player currently marked as a hider", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaKillHiders : hamburburmod
{
    protected override void Start() => MecchaState.EnsureInitialized();
    protected override void Pressed()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null && MecchaState.GetRole(rig.Creator.ActorNumber) == MecchaRole.Hider)
                MecchaNetwork.Kill(rig.Creator.ActorNumber);
    }
}

[hamburburmod("Kill All Seekers", "Finds every player currently marked as a seeker", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaKillSeekers : hamburburmod
{
    protected override void Start() => MecchaState.EnsureInitialized();
    protected override void Pressed()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null && MecchaState.GetRole(rig.Creator.ActorNumber) == MecchaRole.Seeker)
                MecchaNetwork.Kill(rig.Creator.ActorNumber);
    }
}

[hamburburmod("Respawn All", "Respawns every painter", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaRespawnAll : hamburburmod
{
    protected override void Pressed()
    {
        MecchaNetwork.Respawn(MecchaNetwork.LocalId);
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null) MecchaNetwork.Respawn(rig.Creator.ActorNumber);
    }
}

[hamburburmod("Always Alive", "Keeps respawning your painter every half second", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaAlwaysAlive : hamburburmod
{
    private float next;
    protected override void Update()
    {
        if (Time.time < next) return;
        next = Time.time + 0.5f;
        MecchaNetwork.Respawn(MecchaNetwork.LocalId);
    }
}

[hamburburmod("Whistle All", "Makes every painter whistle once", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaWhistleAll : hamburburmod
{
    protected override void Pressed()
    {
        MecchaNetwork.Whistle(MecchaNetwork.LocalId);
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null) MecchaNetwork.Whistle(rig.Creator.ActorNumber);
    }
}

[hamburburmod("Whistle Spam", "Rapidly whistles from your painter", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaWhistleSpam : hamburburmod
{
    private float next;
    protected override void Update()
    {
        if (Time.time < next) return;
        next = Time.time + 0.3f;
        MecchaNetwork.Whistle(MecchaNetwork.LocalId);
    }
}
