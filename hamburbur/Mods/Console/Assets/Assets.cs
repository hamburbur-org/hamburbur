using System.Linq;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Spawn iPhone Asset", "Spawns a iPhone in your left hand", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class SpawnPhoneAsset : hamburburmod
{
    private static readonly string[] VideoLinks =
    [
            "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/REmZhFKmOmo.mp4",
            "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/Playboi%20Cart%20-%20Sky.mp4",
            "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/monkeys_dancing.mp4",
            "https://drive.iidk.online/resources/iidk/shiba%20youtube.mp4",
            "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/hamburger.mp4",
    ];

    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "iphone", "iPhone", assetId);

        if (BigAssets.isEnabled)
            Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 1);
        Components.Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "Model/Video",
                VideoLinks[Random.Range(0, VideoLinks.Length)]);

        Components.Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, assetId);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}

[hamburburmod("Spawn Travis Asset", "Spawns Travis Event", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class SpawnTravisAsset : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "travis", "travisscott",
                assetId);

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(-70f, 2f, -52f));

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 0.38f);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}

[hamburburmod("Spawn Mini Travis Asset", "Spawns Mini Travis Event in your left hand", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class SpawnMiniTravisAsset : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "minitravis", "travisscott",
                assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 1);

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
                new Vector3(-0.6f, 0.2f, 0f));

        Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, assetId,
                new Vector3(80f, 160f, 180f));
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}

[hamburburmod("Flash Trail: ", "Change the flash effect", ButtonType.Incremental, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class FlashTrailType : hamburburmod
{
    public static readonly string[] EffectNames =
    [
            "Zoom Body Trail",
            "Ares Body Trail",
    ];

    public static   FlashTrailType Instance { get; private set; }
    public override string         ModName  => AssociatedAttribute.Name + EffectNames[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue >= EffectNames.Length)
            IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = EffectNames.Length - 1;
    }
}

[hamburburmod(                "Spawn Flash Effects Asset", "Gives you flash effects for everyone that has console",
        ButtonType.Togglable, AccessSetting.AdminOnly,     EnabledType.AlwaysDisabled, 0)]
public class SpawnFlashEffectsAsset : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "flasheffects",
                FlashTrailType.EffectNames[FlashTrailType.Instance.IncrementalValue], assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 3);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}

[hamburburmod("Concert Type: ", "Change the concert type", ButtonType.Incremental, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class ConcertVideoType : hamburburmod
{
    public static readonly string[] ConcertVideoNames =
    [
            "MOJO JOJO",
            "New Tank",
            "CRANK",
            "Over",
            "POP OUT",
            "OPM BABI",
            "Long Time",
            "Punk Monk",
            "R.I.P. Fredo (Notice Me)",
            "Foreign",
            "Sky",
            "FINE SHIT",
            "JumpOutTheHouse",
            "Lean 4 Real",
            "DIAMONDS SPECIAL",
            "RADAR",
            "Mileage",
            "Rockstar Made",
            "SOME MORE",
            "I SEEEE YOU BABY BOI",
            "DRUGS GOT ME NUMB",
            "OLYMPIAN",
            "F33l Lik3 Dyin",
            "BACKD00R",
    ];

    public static ConcertVideoType Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + ConcertVideoNames[IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue >= ConcertVideoNames.Length)
            IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = ConcertVideoNames.Length - 1;
    }
}

[hamburburmod("Spawn Concert Asset", "Spawn in a concert stage and plays carti.", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class SpawnConcertAsset : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Vector3 position = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").activeInHierarchy
                                   ? new Vector3(-27f,      2.4f,     -49.9f)
                                   : new Vector3(-28.4873f, 15.5272f, -117.8634f);

        Quaternion rotation = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").activeInHierarchy
                                      ? Quaternion.Euler(0f, 250f, 0f)
                                      : Quaternion.Euler(0f, 300f, 0f);

        Vector3 scale = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").activeInHierarchy
                                ? new Vector3(0.5f, 0.5f, 0.5f)
                                : new Vector3(0.8f, 0.8f, 0.8f);

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "concert",
                "concert", assetId);

        Components.Console.ExecuteCommand("asset-settransform", ReceiverGroup.All, assetId,
                position, rotation);

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId,
                scale);

        Components.Console.ExecuteCommand("asset-destroychild", ReceiverGroup.All, assetId, "stage/Targetphoto");

        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, assetId, "audio",
                ConcertVideoType.ConcertVideoNames[ConcertVideoType.Instance.IncrementalValue]);
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
    }
}

[hamburburmod("Jail Cell Asset Gun", "Traps people inside of the jail", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class JailCellAssetGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };
    private          int    assetId;
    private          bool   wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
        {
            if (wasShooting) return;

            Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                    gunLib.ChosenRig.transform.position + new Vector3(-1f, -3f, -18f));

            wasShooting = true;
        }
        else
        {
            wasShooting = false;
        }
    }

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "jailcell",
                "jail", assetId);
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
    }
}

[hamburburmod(                   "Spawn Donation Nuke Asset", "Donation Nuke from pls donate.", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled,  0)]
public class SpawnDonationNukeAsset : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "donationnuke",
                "plsdonatenuke", assetId);

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(-64.16f, 2.99f, -82.07f));

        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, assetId, "nuke", "nukesound");
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
    }
}

[hamburburmod(                   "Spawn Hamburbur Asset", "Spawns a hamburbur in your right hand", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class SpawnHamburburAsset : hamburburmod
{
    private int   assetId;
    private float nextPlayTime;

    protected override void LateUpdate()
    {
        if (!NetworkSystem.Instance.InRoom || Time.time < nextPlayTime)
            return;

        foreach (VRRig rig in GorillaParent.instance.vrrigs.Where(rig => Vector3.Distance(
                                                                                 rig.headMesh.transform.position,
                                                                                 GorillaTagger.Instance.offlineVRRig
                                                                                        .rightHandTransform.position) <=
                                                                         0.4f))
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, assetId, "Sound",
                    "mmmchezburger");

        nextPlayTime = Time.time + 2f;
    }

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "effects", "rblxcheezburger",
                assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2);
        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, assetId, "Sound",
                "canihaveachezburger");
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
    }
}

[hamburburmod("Spawn Pistol Asset", "Spawns a shootable pistol in your right hand", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class SpawnPistolAsset : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Pistol", assetId);

        if (BigAssets.isEnabled)
            Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 2);
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
    }

    protected override void LateUpdate()
    {
        if (!NetworkSystem.Instance.InRoom)
            return;

        if (InputManager.Instance.RightTrigger.WasPressed)
        {
            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, assetId, "Model",
                    "PistolShoot");

            Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, assetId, "Model",
                    "Shoot");
        }

        if (InputManager.Instance.RightTrigger.WasReleased)
            Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, assetId, "Model",
                    "Default");
    }
}