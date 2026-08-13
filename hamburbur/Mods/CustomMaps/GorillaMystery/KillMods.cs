using System.Linq;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.GorillaMystery;

[hamburburmod("Spawn Kill Skeletons", "Spawn and launch a skeleton when a Gorilla Mystery kill mod is used",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class SpawnKillSkeletons : hamburburmod
{
    public static bool IsEnabled { get; private set; }

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[hamburburmod("Murder Kill Gun", "Spam kills the selected player with PlayerDeath and high velocity while held",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class MurderKillGun : hamburburmod
{
    private const float KillDelay = 0.2f;

    private readonly GunLib gunLib = new() { ShouldFollow = true, };
    private          float  nextKillTime;

    protected override void Start()
    {
        MysteryTagState.EnsureInitialized();
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null || Time.time < nextKillTime)
            return;

        nextKillTime = Time.time + KillDelay;
        MysteryTagNetwork.Kill(gunLib.ChosenRig, true);
    }

    protected override void OnDisable()
    {
        nextKillTime = 0f;
        gunLib.OnDisable();
    }
}

[hamburburmod("Sheriff Kill Gun", "Spam kills the selected player with SheriffHit and high velocity while held",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SheriffKillGun : hamburburmod
{
    private const float KillDelay = 0.12f;

    private readonly GunLib gunLib = new() { ShouldFollow = true, };
    private          float  nextKillTime;

    protected override void Start()
    {
        MysteryTagState.EnsureInitialized();
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null || Time.time < nextKillTime)
            return;

        nextKillTime = Time.time + KillDelay;
        MysteryTagNetwork.SheriffKill(gunLib.ChosenRig, true);
    }

    protected override void OnDisable()
    {
        nextKillTime = 0f;
        gunLib.OnDisable();
    }
}

[hamburburmod("Kill All", "Kills every other living Gorilla Mystery player", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MysteryKillAll : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            MysteryTagNetwork.Kill(rig);
    }
}

[hamburburmod(                "Kill Sheriff",       "Kills the player currently assigned as sheriff", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class KillSheriff : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        if (MysteryTagState.SheriffActorNumber is { } actorNumber)
            MysteryTagNetwork.Kill(actorNumber.Rig());
    }
}

[hamburburmod(                "Kill Murderer",      "Kills the player currently assigned as murderer", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class KillMurderer : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        if (MysteryTagState.MurdererActorNumber is { } actorNumber)
            MysteryTagNetwork.Kill(actorNumber.Rig());
    }
}

[hamburburmod(            "Kill All Innocents", "Kills all living players who are neither murderer nor sheriff",
        ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class KillAllInnocents : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
        {
            if (rig?.Creator                                     == null ||
                MysteryTagState.GetRole(rig.Creator.ActorNumber) != MysteryTagRole.Innocent)
                continue;

            MysteryTagNetwork.Kill(rig);
        }
    }
}