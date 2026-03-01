using System;
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
    
    public static ScreenShotCamera Instance { get; private set; }

    public static GameObject menuPrefab;

    public static readonly List<Tuple<string, string, Vector3, Quaternion, Color>> AllThemes =
    [
            Tuple.Create("hamburburv2", "hamburbur", Vector3.zero, Quaternion.identity,
                    Plugin.Instance.MainColour),
            Tuple.Create("hamburbur", "hamburbur OG", Vector3.zero, Quaternion.Euler(0f, 0f, 90f),
                    Plugin.Instance.MainColour),

            Tuple.Create("hansoloschoice", "HanSolo's Choice", new Vector3(0f, 0f, -0.02f),
                    Quaternion.Euler(0f, 90f, 90f),
                    Plugin.Instance.MainColour),

            Tuple.Create("developertheme", "Developer theme", Vector3.zero, Quaternion.identity,
                    Plugin.Instance.MainColour),
            Tuple.Create("destiny", "Destiny", Vector3.zero, Quaternion.identity,
                    new Color(0.1764705f, 0.9058824f, 0.8862745f, 1f)),

            Tuple.Create("stupidtheme", "ii's Stupid Menu", Vector3.zero, Quaternion.identity,
                    new Color(1f, 0.5f, 0f, 1f)),

            Tuple.Create("shibadark", "ShibaGT Dark", Vector3.zero, Quaternion.Euler(0f, 90f, 90f),
                    new Color(0.039f, 0f, 0.953f, 1f)),

            Tuple.Create("sybauGold", "ShibaGT Gold", Vector3.zero, Quaternion.identity,
                    new Color(1f, 0.9215686f, 0.01960784f, 1f)),

            Tuple.Create("zlothsimple", "Zloth's Simple Menu", new Vector3(0f, 0f, -0.03f),
                    Quaternion.Euler(0f, 90f, 90f), new Color(0.3019608f, 0f, 0.9843137f, 1f)),

            Tuple.Create("cyclonereborn", "Cyclone Reborn", Vector3.zero,
                    Quaternion.Euler(0f, 90f, -90f),
                    new Color(0.4549019f, 0f, 0.972549f, 1f)),

            Tuple.Create("gorillabuddies", "Gorilla Buddies", Vector3.zero,
                    Quaternion.Euler(0f, 90f, 90f),
                    new Color(0.4431372f, 0f, 0.6862745f, 1f)),

            Tuple.Create("nuggetpad", "Nugget Pad", new Vector3(0f, 0f, -0.04f),
                    Quaternion.Euler(0f, 90f, 90f),
                    new Color(0.3607843f, 0f, 0.9568627f, 1f)),

            Tuple.Create("nxoremastered", "NXO Remastered", new Vector3(0f, 0f, -0.01f),
                    Quaternion.Euler(0f, 90f, 90f),
                    new Color(0.4117647f, 0.172549f, 0.9843137f, 1f)),

            Tuple.Create("xyfer", "Xyfer", new Vector3(0f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f),
                    new Color(0.7019608f, 0.6274511f, 0.7607843f)),

            Tuple.Create("simplicity", "Simplicity", new Vector3(0f, 0f, 0f),
                    Quaternion.Euler(0f, 90f, 90f),
                    new Color(0.6f, 0.6f, 0.6f)),

            Tuple.Create("clickbait", "ClickBait Menu", new Vector3(0f, 0f, 0f),
                    Quaternion.Euler(0f, 0f, 90f),
                    Color.white),

            Tuple.Create("shirtspad", "Shirts Pad", new Vector3(0f, 0f, 0f),
                    Quaternion.Euler(0f, 0f, -90f),
                    new Color(0.2862745f, 0.2862745f, 0.2509804f)),
            
            Tuple.Create("baggztheme", "BaggZ's Theme", Vector3.zero, Quaternion.Euler(0f, 270f, 270f), new Color(0.05098037f, 0.2901961f, 0.3568627f)),
    ];

    public override string ModName => AssociatedAttribute.Name + AllThemes[IncrementalValue].Item2;

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
        PrefabName = AllThemes[IncrementalValue].Item1;
        menuPrefab = ThemesDict.TryGetValue(PrefabName, out GameObject value)
                                        ? value
                                        : Plugin.Instance.HamburburBundle.LoadAsset<GameObject>(PrefabName);

        ThemesDict[PrefabName] = menuPrefab;

        MenuHandler.Instance.SetUpMenu(
                menuPrefab,
                MenuHandler.Instance.Menu.transform.parent,
                AllThemes[IncrementalValue].Item3,
                AllThemes[IncrementalValue].Item4,
                AllThemes[IncrementalValue].Item5,
                true
        );
    }

    protected override void OnIncrementalStateLoaded() => LoadCurrentTheme();
}