using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(                      "Video Player", "Plays videos", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class VideoPlayer : hamburburmod
{
    private int assetId;

    protected override void OnEnable()
    {
        assetId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "VideoPlayer", assetId);

        Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, assetId, 1);
        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId,
                new Vector3(0.05f, 0.05f, 0.05f));

        Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, assetId,
                new Vector3(0f, 0.04f, 0.12f));

        Components.Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, assetId);

        Components.Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, assetId, "Video",
                VideoPlayerType.Instance.CurrentUrl);
    }

    protected override void OnDisable() =>
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
}

[hamburburmod("Video Player Video: ", "Change the video that plays on the video player", ButtonType.Incremental,
        AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class VideoPlayerType : hamburburmod
{
    private static readonly Dictionary<string, string> Videos = new()
    {
            { "Elliot Likes Femboys", "https://files.hamburbur.org/ElliotLikesFemboys.mp4" },
            {
                    "Dancing Monkeys",
                    "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/monkeys_dancing.mp4"
            },
            {
                    "Sky - Carti",
                    "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/Playboi%20Cart%20-%20Sky.mp4"
            },
            { "Over - Carti", "https://files.hamburbur.org/Over-PlayboiCarti.mp4" },
            { "Rendezvous - Don Toliver", "https://files.hamburbur.org/Rendezvous-DonToliver.mp4" },
            {
                    "wokeuplikethis* - Carti",
                    "https://github.com/ZlothY29IQ/Mod-Resources/raw/refs/heads/main/REmZhFKmOmo.mp4"
            },
            { "GPT Mod Menu - SoupVR", "https://files.hamburbur.org/gptmodmenu-soupvr.mp4" },
            { "Did you pray today?", "https://files.hamburbur.org/didyoupraytoday.mp4" },
            { "Zimble Mod Checker", "https://files.hamburbur.org/zimblemodchecker.mov" },
            { "Crazy Russian Guy", "https://files.hamburbur.org/crazyrussianguy.mp4" },
            { "Tom Holland Moment", "https://files.hamburbur.org/tomhollandmoment.mp4" },
            { "Im a Korean", "https://files.hamburbur.org/imakorean.mov" },
            { "ShibaGT Gold Rat", "https://files.hamburbur.org/shibagoldrat.mov" },
            { "USA Rat", "https://files.hamburbur.org/usamenu.mp4" },
            { "Press Option 1 Now", "https://files.hamburbur.org/gorilla-tag-gorilla.mp4" },
            { "Zimble Bad Boy", "https://files.hamburbur.org/zimblebadboy.mp4" },
            { "Caramell Dansen", "https://files.hamburbur.org/caramelldansen.mp4" },
            {
                    "How to Protect Your Shopping Trolley From Improvised Explosives",
                    "https://files.hamburbur.org/How%20to%20Protect%20Your%20Shopping%20Trolley%20From%20Improvised%20Explosives.mp4"
            },
            { "Theo Does Snacks", "https://files.hamburbur.org/TheoDoesSnacks.mov" },
            { "ZlothY Locura", "https://files.hamburbur.org/ZlothYLocura.mov" },
            { "Skidding is a Crime", "https://files.hamburbur.org/SkiddingIsACrime.mp4" },
            { "Rizz", "https://files.hamburbur.org/rizz.mp4"},
            { "Shimmy Shimmy ya", "https://files.hamburbur.org/shimmy%20shimmy%20ya%20but%20high%20quality%20(full).mp4"},
            { "You got me jumping like", "https://files.hamburbur.org/YouGotMeJumpingLike.mov"},
            { "Guardians of the Galaxy Vol 2", "https://files.hamburbur.org/Guardians%20of%20the%20Galaxy%20Vol.%202%20(2017)%20(Awafim.tv).mp4" },
            { "Five Nights at Freddy's 2", "https://files.hamburbur.org/FNaF2_UnityReady.mp4" },
    };

    private static readonly List<string> Keys = Videos.Keys.ToList();

    public static VideoPlayerType Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + Keys[IncrementalValue];

    public string CurrentUrl => Videos[Keys[IncrementalValue]];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue >= Keys.Count)
            IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = Keys.Count - 1;
    }
}