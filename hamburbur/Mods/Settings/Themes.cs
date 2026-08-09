using System.Collections.Generic;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Theme: ", "Change the current theme", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Themes : hamburburmod
{
    public static          string                         PrefabName;
    public static readonly Dictionary<string, GameObject> ThemesDict = new();

    public static GameObject menuPrefab;

    public static readonly List<(
            string Name,
            string DisplayName,
            Vector3 Position,
            Quaternion Rotation,
            Color MainColour,
            Color SecondaryColour,
            float keyboardHeightOffset,
            bool isCanvasMenu)> AllThemes =
    [
            (
                "hamburburv2",
                nameof(hamburbur),
                Vector3.zero,
                Quaternion.identity,
                Plugin.Instance.MainColour,
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "hamburbur",
                "hamburbur OG",
                Vector3.zero,
                Quaternion.Euler(0f, 0f, 90f),
                Plugin.Instance.MainColour,
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "hansoloschoice",
                "HanSolo's Choice",
                new Vector3(0f, 0f, -0.02f),
                Quaternion.Euler(0f, 90f, 90f),
                Plugin.Instance.MainColour,
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "developertheme",
                "Developer theme",
                Vector3.zero,
                Quaternion.identity,
                Plugin.Instance.MainColour,
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "destiny",
                "Destiny",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.1764705f, 0.9058824f, 0.8862745f, 1f),
                new Color(0.1568627f, 0.1568627f, 0.1529411f, 1f),
                -0.29f,
                false
            ),

            (
                "stupidtheme",
                "ii's Stupid Menu",
                Vector3.zero,
                Quaternion.identity,
                new Color(1f,         0.5f,       0f, 1f),
                new Color(0.6627452f, 0.3254902f, 0f, 1f),
                -0.29f,
                false
            ),

            (
                "seralyth",
                "Seralyth",
                Vector3.zero,
                Quaternion.identity,
                new Color(0.03921569f, 0.03921569f, 0.03921569f, 1f),
                new Color(0.4666666f,  0.1254902f,  0.9686274f,  1f),
                -0.29f,
                false
            ),

            (
                "shibadark",
                "ShibaGT Dark",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.039f, 0f, 0.953f, 1f),
                new Color(0f,     0f, 0f,     1f),
                -0.29f,
                false
            ),

            (
                "sybauGold",
                "ShibaGT Gold",
                Vector3.zero,
                Quaternion.identity,
                new Color(0.4745098f, 0.4392157f, 0f,          1f),
                new Color(1f,         0.9215686f, 0.01960784f, 1f),
                -0.29f,
                false
            ),

            (
                "sybaugenesis",
                "ShibaGT Genesis",
                Vector3.zero,
                Quaternion.Euler(90f, 0f, 0f),
                Plugin.Instance.SecondaryColour,
                Plugin.Instance.MainColour,
                -0.29f,
                false
            ),

            (
                "zlothsimple",
                "Zloth's Simple Menu",
                new Vector3(0f, 0f, -0.03f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.5019608f, 0.5019608f, 0.5019608f, 1f),
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "cyclonereborn",
                "Cyclone Reborn",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, -90f),
                new Color(0.545098f,  0.172549f, 0.427451f, 1f),
                new Color(0.4549019f, 0f,        0.972549f, 1f),
                -0.29f,
                false
            ),

            (
                "gorillabuddies",
                "Gorilla Buddies",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.4431372f, 0f, 0.6862745f, 1f),
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "nuggetpad",
                "Nugget Pad",
                new Vector3(0f, 0f, -0.04f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.3607843f, 0f, 0.9568627f, 1f),
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "nxoremasteredslim",
                "NXO Remastered",
                new Vector3(0f, 0f, -0.01f),
                Quaternion.Euler(90f, 0f, 0f),
                new Color(0.4117647f, 0.172549f, 0.9843137f, 1f),
                Color.black,
                -0.29f,
                false
            ),

            (
                "nxoremastered",
                "NXO Remastered Wide",
                new Vector3(0f, 0f, -0.01f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.4117647f, 0.172549f, 0.9843137f, 1f),
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "xyfer",
                "Xyfer",
                Vector3.zero,
                Quaternion.Euler(90f, 0f, 0f),
                new Color(0.8627451f, 0.7843137f, 0.9372549f, 1f),
                new Color(0.7019608f, 0.6274511f, 0.7607843f, 1f),
                -0.29f,
                false
            ),

            (
                "simplicity",
                "Simplicity",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.1792453f, 0.1792453f, 0.1792453f, 1f),
                new Color(0.6f,       0.6f,       0.6f,       1f),
                -0.29f,
                false
            ),

            (
                "clickbait",
                "ClickBait Menu",
                Vector3.zero,
                Quaternion.Euler(0f, 0f, 90f),
                Color.white,
                Color.green,
                -0.29f,
                false
            ),

            (
                "shirtspad",
                "Shirts Pad",
                Vector3.zero,
                Quaternion.Euler(0f, 0f, -90f),
                new Color(0.2117647f, 0.1490196f, 0.1411764f, 1f),
                new Color(0.2862745f, 0.2862745f, 0.2509804f),
                -0.29f,
                false
            ),

            (
                "baggztheme",
                "BaggZ's Theme",
                Vector3.zero,
                Quaternion.Euler(0f, 270f, 270f),
                new Color(0f,          1f,         0.75f,      1f),
                new Color(0.05098037f, 0.2901961f, 0.3568627f, 1f),
                -0.29f,
                false
            ),

            (
                "r3",
                "R3",
                Vector3.zero,
                Quaternion.Euler(0f, 180f, 270f),
                new Color(0.7607843f, 0.8745098f, 0.7764706f, 1f),
                new Color(0.4392157f, 0.6705883f, 0.4745098f, 1f),
                -0.29f,
                false
            ),

            (
                "tupbuthamburbur",
                "TUP | Sakura",
                Vector3.zero,
                Quaternion.Euler(90f, 0f, 0f),
                Plugin.Instance.MainColour,
                Plugin.Instance.SecondaryColour,
                -0.29f,
                false
            ),

            (
                "phantom",
                "Phantom",
                Vector3.zero,
                Quaternion.Euler(90f, 0f, 0f),
                Color.black,
                new Color(0.04716978f, 0.04716978f, 0.04716978f, 1f),
                -0.32f,
                false
            ),

            (
                "bark",
                "Bark Menu",
                new Vector3(0f, 0f, -0.25f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.4627451f, 0.3058823f, 0.1686274f, 1f),
                new Color(0.7490196f, 0.7490196f, 0.7490196f, 1f),
                -0.29f,
                false
            ),

            (
                "dinostheme",
                "Dino's Theme",
                Vector3.zero,
                Quaternion.Euler(90f, 0f, 0f),
                new Color(0.6117647f, 0.1647058f, 0.7450981f, 1f),
                new Color(0.9607843f, 0.8705882f, 1f,         1f),
                -0.29f,
                false
            ),

            (
                "goldenstheme",
                "Golden's Theme",
                new Vector3(0f, 0f, 0.04f),
                Quaternion.Euler(0f, 90f, 90f),
                Color.black,
                new Color(0.517647f, 0f, 0.9803922f, 1f),
                -0.4f,
                false
            ),

            (
                "illusion",
                "Illusion",
                new Vector3(0f, 0f, -0.04f),
                Quaternion.Euler(90f, 0f, 0f),
                Color.black,
                Color.gray2,
                -0.29f,
                false
            ),

            (
                "bugerking",
                "Burger King",
                new Vector3(0f, 0f, -0.04f),
                Quaternion.identity,
                new Color(0.317647f,  0.1372549f, 0.07843135f, 1f),
                new Color(0.8431372f, 0.1372548f, 0f,          1f),
                -0.32f,
                false
            ),

            (
                "p4",
                "Project Four",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.1773585f, 0.1773585f, 0.1773585f, 1f),
                new Color(0.2905661f, 0.2905661f, 0.2905661f,         1f),
                -0.29f,
                true
            ),
            
            (
                "hamburburcanvas",
                "hamburbur UI",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(0f, 270f, 270f),
                new Color(0.09801987f, 0.05664827f, 0.1792453f, 1f),
                new Color(0.1825759f,  0.06314526f, 0.4056604f, 1f),
                -0.29f,
                true
            ),
    ];

    public static Themes Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + AllThemes[IncrementalValue].DisplayName;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue = (IncrementalValue + 1) % AllThemes.Count;
        LoadCurrentTheme();
    }

    protected override void Decrement()
    {
        IncrementalValue = (IncrementalValue - 1 + AllThemes.Count) % AllThemes.Count;
        LoadCurrentTheme();
    }

    private void LoadCurrentTheme()
    {
        (
                string Name,
                string DisplayName,
                Vector3 Position,
                Quaternion Rotation,
                Color MainColour,
                Color SecondaryColour,
                float Offset,
                bool isCanvasMenu
                ) theme = AllThemes[IncrementalValue];

        PrefabName = theme.Name;

        menuPrefab = ThemesDict.TryGetValue(PrefabName, out GameObject value)
                             ? value
                             : Plugin.Instance.HamburburBundle.LoadAsset<GameObject>(PrefabName);

        ThemesDict[PrefabName] = menuPrefab;

        MenuHandler.Instance.SetUpMenu(
                menuPrefab,
                MenuHandler.Instance.Menu.transform.parent,
                theme.Position,
                theme.Rotation,
                theme.MainColour,
                theme.SecondaryColour,
                theme.Offset,
                theme.isCanvasMenu,
                true
        );
    }

    protected override void OnIncrementalStateLoaded() => LoadCurrentTheme();
}