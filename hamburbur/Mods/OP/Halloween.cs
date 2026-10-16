using System;
using System.Collections;
using GorillaLocomotion.Gameplay;
using GorillaTagScripts;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Visual;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.OP;

public static class HalloweenInteractables
{
    public static HalloweenGhostChaser Lucy
    {
        get
        {
            field ??= GameObject.Find("Environment Objects/05Maze_PersistentObjects/Ghosts/Halloween Ghost/FloatingChaseSkeleton").GetComponent<HalloweenGhostChaser>();

            return field;
        }
    }

    public static LurkerGhost Lurker
    {
        get
        {
            field ??= GameObject.Find("Environment Objects/05Maze_PersistentObjects/Ghosts/GhostLurker_Prefab").GetComponent<LurkerGhost>();

            return field;
        }
    }

    public static GameObject Terraformer
    {
        get
        {
            field ??= GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/2026_Halloween_Forest/Terraformer");
            
            return field;
        }
    }

    public static NoncontrollableBroomstick[] Broomsticks
    {
        get
        {
            const string BroomsticksPath = "Environment Objects/LocalObjects_Prefab/Forest/2026_Halloween_Forest/Broomsticks";

            return [
                           GameObject.Find(BroomsticksPath + "/Broomstick2/NoncontrollableBroomstick").GetComponent<NoncontrollableBroomstick>(),
                           GameObject.Find(BroomsticksPath + "/Broomstick3/NoncontrollableBroomstick").GetComponent<NoncontrollableBroomstick>(),
                   ];
        }
    }

    public static void LucyAttack(NetPlayer target)
    {
        HalloweenGhostChaser lucy = Lucy;

        if (Tools.Utils.IsMasterClient || lucy.IsMine)
        {
            if (lucy.targetPlayer != target)
            {
                lucy.currentState = HalloweenGhostChaser.ChaseState.Dormant;
                Tools.Utils.SendSerialize(lucy.GetView);
            }

            lucy.isSummoned   = Tools.Utils.RandomBoolean();
            lucy.currentState = HalloweenGhostChaser.ChaseState.Grabbing;
            lucy.grabTime     = Time.time;
            lucy.targetPlayer = target;
        }
        else
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
        }
    }
    
    public static void LurkerAttack(NetPlayer target)
    {
        LurkerGhost lurker = Lurker;

        if (Tools.Utils.IsMasterClient || lurker.IsMine)
        {
            if (lurker.targetPlayer != target)
            {
                lurker.ChangeState(LurkerGhost.ghostState.patrol);
                Tools.Utils.SendSerialize(lurker.GetView);
            }

            lurker.currentState = LurkerGhost.ghostState.possess;
            lurker.targetPlayer = target;
        }
        else
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
        }
    }
}

[hamburburmod("Disable Halloween Terraformer", "Disables the 'Terraformer' from the halloween update", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DisableTerraformer : hamburburmod
{
    protected override void OnEnable() => HalloweenInteractables.Terraformer.SetActive(false);
    protected override void OnDisable() => HalloweenInteractables.Terraformer.SetActive(true);
}

[hamburburmod("Spawn Lucy [M]", "Spawns the Blue Lucy", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpawnLucy : hamburburmod
{
    protected override void Pressed()
    {
        HalloweenGhostChaser lucy = HalloweenInteractables.Lucy;

        if (!Tools.Utils.IsMasterClient || !lucy.IsMine)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);

            return;
        }

        lucy.timeGongStarted = Time.time;
        lucy.currentState    = HalloweenGhostChaser.ChaseState.Gong;
        lucy.isSummoned      = false;
    }
}

[hamburburmod("Fast Lucy [M]", "Makes lucy fast", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FastLucy : hamburburmod
{
    protected override void Pressed()
    {
        HalloweenGhostChaser lucy = HalloweenInteractables.Lucy;

        if (!Tools.Utils.IsMasterClient || !lucy.IsMine)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);

            return;
        }

        lucy.currentSpeed = 15f;
    }
}

[hamburburmod("Slow Lucy [M]", "Makes lucy slow", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SlowLucy : hamburburmod
{
    protected override void Pressed()
    {
        HalloweenGhostChaser lucy = HalloweenInteractables.Lucy;

        if (!Tools.Utils.IsMasterClient || !lucy.IsMine)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);

            return;
        }

        lucy.currentSpeed = 1f;
    }
}

[hamburburmod("Freeze Lucy [M]", "Makes lucy frozen", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FreezeLucy : hamburburmod
{
    protected override void Pressed()
    {
        HalloweenGhostChaser lucy = HalloweenInteractables.Lucy;

        if (!Tools.Utils.IsMasterClient || !lucy.IsMine)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);

            return;
        }

        lucy.currentSpeed = 0f;
    }
}

[hamburburmod("Lucy Chase Gun [M]", "Makes lucy chase whoever you want", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LucyChaseGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private            bool wasShooting;
    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting && gunLib.ChosenRig;

        if (isShooting && !wasShooting)
        {
            HalloweenGhostChaser lucy = HalloweenInteractables.Lucy;

            if (Tools.Utils.IsMasterClient || lucy.IsMine)
            {
                lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                lucy.targetPlayer = gunLib.ChosenRig.creator;
                lucy.followTarget = gunLib.ChosenRig.head.rigTarget;
            }
            else
            {
                NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
            }
        }

        wasShooting = isShooting;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}

[hamburburmod("Lucy Attack Gun [M]" ,"Makes lucy attack the target", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LucyAttackGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private float lastTime;

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || !gunLib.ChosenRig || !(lastTime + 0.5f < Time.time))
            return;

        HalloweenInteractables.LucyAttack(gunLib.ChosenRig.creator);
            
        lastTime = Time.time;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}

[hamburburmod("Lucy Attack All [M]" ,"Makes lucy attack the target", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LucyAttackAll : hamburburmod
{
    private float lastTime;

    protected override void LateUpdate()
    {
        if (!(lastTime + 0.1f < Time.time))
            return;

        HalloweenInteractables.LucyAttack(PhotonNetwork.PlayerListOthers[Random.Range(0, PhotonNetwork.PlayerListOthers.Length)]);
            
        lastTime = Time.time;
    }
}

[hamburburmod("Spam Lucy Gong [M]" ,"Spams the lucy grave gong noise", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpamLucyGong : hamburburmod
{
    private float lastTime;

    protected override void Update()
    {
        if (!(lastTime + 0.2f < Time.time))
            return;
        
        HalloweenGhostChaser lucy = HalloweenInteractables.Lucy;

        if (!Tools.Utils.IsMasterClient || !lucy.IsMine)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
            Toggle(ButtonState.Normal, false);
            
            return;
        }

        CoroutineManager.Instance.StartCoroutine(DoubleGongIt(lucy));
            
        lastTime = Time.time;
    }

    private static IEnumerator DoubleGongIt(HalloweenGhostChaser lucy)
    {
        lucy.timeGongStarted = Time.time;
        lucy.currentState    = HalloweenGhostChaser.ChaseState.Gong;
        lucy.isSummoned      = false;

        yield return new WaitForSeconds(0.1f);
        
        lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
        lucy.targetPlayer = PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.PlayerList.Length)];
        lucy.followTarget = VRRig.LocalRig.head.rigTarget;
    }
}

[hamburburmod("Lurker Move Gun [M]", "Moves the lurker to wherever you want", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LurkerMoveGun : hamburburmod
{
    protected override Type[] IncompatibleMods => [typeof(AntiLurker),];

    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting)
            return;

        LurkerGhost lurker = HalloweenInteractables.Lurker;

        if (Tools.Utils.IsMasterClient || lurker.IsMine)
            lurker.transform.position = gunLib.Hit.point;
        else
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}

[hamburburmod("Lurker Attack Gun [M]" ,"Makes lurker attack the target", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class LurkerAttackGun : hamburburmod
{
    protected override Type[] IncompatibleMods => [typeof(AntiLurker),];
    
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private float lastTime;

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || !gunLib.ChosenRig || !(lastTime + 0.5f < Time.time))
            return;

        HalloweenInteractables.LurkerAttack(gunLib.ChosenRig.creator);
            
        lastTime = Time.time;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}

[hamburburmod("Fast Broomsticks [M]", "Makes the broomsticks fast", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FastBroomsticks : hamburburmod
{
    protected override void OnEnable()
    {
        if (!Tools.Utils.IsMasterClient)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
            Toggle(ButtonState.Normal, false);
            return;
        }
        
        foreach (NoncontrollableBroomstick broomstick in HalloweenInteractables.Broomsticks)
            broomstick.duration = 10f;
    }
    
    protected override void OnDisable()
    {
        if (!Tools.Utils.IsMasterClient)
            return;
        
        foreach (NoncontrollableBroomstick broomstick in HalloweenInteractables.Broomsticks)
            broomstick.duration = 30f;
    }
}

[hamburburmod("Slow Broomsticks [M]", "Makes the broomsticks slow", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SlowBroomsticks : hamburburmod
{
    protected override void OnEnable()
    {
        if (!Tools.Utils.IsMasterClient)
        {
            NotificationManager.SendNotification("<color=red>Error</color>", "You are not master client", 3f, true, false);
            Toggle(ButtonState.Normal, false);
            return;
        }
        
        foreach (NoncontrollableBroomstick broomstick in HalloweenInteractables.Broomsticks)
            broomstick.duration = 60f;
    }
    
    protected override void OnDisable()
    {
        if (!Tools.Utils.IsMasterClient)
            return;
        
        foreach (NoncontrollableBroomstick broomstick in HalloweenInteractables.Broomsticks)
            broomstick.duration = 30f;
    }
}