using System;
using System.Collections;
using System.Collections.Generic;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod(nameof(Themes), "Open the themes category", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
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
                "untitled",
                "Untitled Menu",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.2313725f, 0.05098037f, 0.08235291f, 1f),
                new Color(0.4470588f, 0.145098f,   0.1960784f),
                -0.31f,
                false
            ),

            (
                "untitledui",
                "Untitled UI",
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.05098037f, 0.05098037f, 0.05098037f, 1f),
                new Color(0.3647059f,  0.2588235f,  0.4509804f),
                -0.29f,
                true
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
                new Color(0.2905661f, 0.2905661f, 0.2905661f, 1f),
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

            (
                "Sweet",
                "Magma",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.1509434f,  0.1509434f,  0.1509434f,  1f),
                new Color(0.09433959f, 0.09433959f, 0.09433959f, 1f),
                -0.29f,
                true
            ),

            (
                "spectral",
                "Spectral Menu",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                new Color(0.098039194f, 0.01960784f, 0.2f,       1f),
                new Color(0.545098f,    0.2352941f,  0.8156863f, 1f),
                -0.29f,
                false
            ),

            (
                "Vivid",
                "Vivid",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                Color.black,
                new Color(1f, 0.6542654f, 0.7911164f, 1f),
                -0.29f,
                false
            ),

            (
                "VividV2",
                "Vivid V2",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                Color.black,
                new Color(0.2f, 0.2f, 0.2f, 1f),
                -0.29f,
                false
            ),

            (
                "morphine",
                "Morphine",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(0f, 90f, 90f),
                Color.black,
                new Color(0.1169811f, 0.1169811f, 0.1169811f, 0.5019608f),
                -0.29f,
                true
            ),

            (
                "juul",
                "Juul",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                new Color(0.6509804f, 0.8156863f, 0.8078431f, 1f),
                new Color(0.3396226f, 0.3396226f, 0.3396226f, 0.6f),
                -0.29f,
                false
            ),

            (
                "elixir",
                "Elixir",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(0f, 90f, 90f),
                new Color(0.06378649f, 0f, 0.1603774f, 1f),
                new Color(0.2726023f,  0f, 0.6886792f, 0.6f),
                -0.29f,
                true
            ),

            (
                "silliness",
                "Silliness",
                new Vector3(0f, 0f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                new Color(0.06666665f, 0f, 0.02745098f, 1f),
                new Color(1f,          0f, 0.2509804f,  0.6f),
                -0.29f,
                false
            ),
    ];

    public static   Themes Instance               { get; private set; }
    internal static bool   IsSynchronizingButtons { get; private set; }

    public static int CurrentIndex => NormalizeIndex(Instance?.IncrementalValue ?? 0);

    public static (Type, hamburburmod)[] CreateThemeButtons()
    {
        (Type, hamburburmod)[] buttons = new (Type, hamburburmod)[AllThemes.Count];

        for (int i = 0; i < buttons.Length; i++)
            buttons[i] = (typeof(ThemeButton), new ThemeButton(i));

        return buttons;
    }

    protected override void Start()
    {
        Instance         = this;
        IncrementalValue = NormalizeIndex(IncrementalValue);
        CoroutineManager.Instance?.StartCoroutine(SynchronizeButtonsAfterStartup());
    }

    protected override void Pressed() => ButtonHandler.Instance?.SetCategory(nameof(Themes));

    internal static void SelectThemeFromButton(int themeIndex)
    {
        if (Instance == null)
            return;

        themeIndex = NormalizeIndex(themeIndex);

        if (Instance.IncrementalValue == themeIndex)
        {
            SynchronizeThemeButtons();

            return;
        }

        Instance.IncrementalValue = themeIndex;
        Instance.LoadCurrentTheme(true, themeIndex);
    }

    private IEnumerator SynchronizeButtonsAfterStartup()
    {
        yield return null;
        SynchronizeThemeButtons();
    }

    private void LoadCurrentTheme(bool preserveSelectedThemePage = false, int selectedThemeIndex = -1)
    {
        IncrementalValue = NormalizeIndex(IncrementalValue);

        bool keepSelectedThemeVisible = preserveSelectedThemePage             &&
                                        MenuHandler.Instance          != null &&
                                        MenuHandler.Instance.Category == nameof(Themes);

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

        SynchronizeThemeButtons(false);

        if (keepSelectedThemeVisible && ButtonHandler.ButtonsPerPage > 0)
            MenuHandler.Instance.PageIndex = NormalizeIndex(selectedThemeIndex) / ButtonHandler.ButtonsPerPage;

        ButtonHandler.Instance?.UpdateButtons();
    }

    protected override void OnIncrementalStateLoaded()
    {
        IncrementalValue = NormalizeIndex(IncrementalValue);
        LoadCurrentTheme();
    }

    private static void SynchronizeThemeButtons(bool updateButtons = true)
    {
        if (!Buttons.Categories.TryGetValue(nameof(Themes), out (Type, hamburburmod)[] themeButtons))
            return;

        IsSynchronizingButtons = true;

        try
        {
            foreach ((Type _, hamburburmod mod) in themeButtons)
                if (mod is ThemeButton themeButton)
                    themeButton.SetEnabledFromSystem(themeButton.ThemeIndex == CurrentIndex);
        }
        finally
        {
            IsSynchronizingButtons = false;
        }

        if (updateButtons)
            ButtonHandler.Instance?.UpdateButtons();
    }

    private static int NormalizeIndex(int index)
    {
        if (AllThemes.Count == 0)
            return 0;

        return (index % AllThemes.Count + AllThemes.Count) % AllThemes.Count;
    }
}

[hamburburmod(                      "Theme", "Switch to this menu theme", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.AlwaysDisabled, 0)]
public sealed class ThemeButton(int themeIndex) : hamburburmod
{
    public int ThemeIndex { get; } = themeIndex;

    public override string ModName => ThemeIndex >= 0 && ThemeIndex < Themes.AllThemes.Count
                                              ? Themes.AllThemes[ThemeIndex].DisplayName
                                              : AssociatedAttribute.Name;

    protected override void OnEnable()
    {
        if (!Themes.IsSynchronizingButtons)
            Themes.SelectThemeFromButton(ThemeIndex);
    }

    protected override void OnDisable()
    {
        if (!Themes.IsSynchronizingButtons && ThemeIndex == Themes.CurrentIndex)
            SetEnabledFromSystem(true);
    }
}