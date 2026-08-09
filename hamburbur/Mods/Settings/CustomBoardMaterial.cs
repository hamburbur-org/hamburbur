using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Custom Board Material: ", "Changes the board material", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class CustomBoardMaterial : hamburburmod
{
    public static Dictionary<string, Material> materials = new();

    private static string[] Keys;

    public static CustomBoardMaterial Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + Keys[IncrementalValue];

    public static Material Current => materials[Keys[Instance.IncrementalValue]];

    protected override void Start()
    {
        Instance = this;

        materials.Add("Main Material", Plugin.Instance.MainMaterial);
        
        materials.Add("Caves Purple Crystal", GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToCave/C_Crystal_Chunk").GetComponent<MeshRenderer>().material);
        
        //These one here make it so you can't see text on the coc + motd, and the proportions on them are off.
        materials.Add("Holo Portal Space", Plugin.Instance.HamburburBundle.LoadAsset<Material>("HoloPortalSpaceMaterial"));
        materials.Add("Animated Galaxy", Plugin.Instance.HamburburBundle.LoadAsset<Material>("AnimatedGalaxyMat"));
        //
        
        materials.Add("Black", new Material(Shaders.UberShader) { color = Color.gray1,});
        
        materials.Add("Slate Blue", new Material(Shaders.UberShader) { color = Color.mediumSlateBlue,});
        
        materials.Add("Default Gorilla Tag", new Material(Shaders.UberShader) { color = new Color32(0, 53, 3, 255),});
        
        Keys = materials.Keys.ToArray();
    }

    protected override void Increment()
    {
        IncrementalValue = (IncrementalValue + 1) % Keys.Length;
        CustomBoardManager.Instance.ReloadAllBoards();
    }

    protected override void Decrement()
    {
        IncrementalValue = (IncrementalValue - 1 + Keys.Length) % Keys.Length;
        CustomBoardManager.Instance.ReloadAllBoards();
    }

    protected override void OnIncrementalStateLoaded()
    {
        IncrementalValue = Mathf.Clamp(IncrementalValue, 0, Keys.Length - 1);
        CustomBoardManager.Instance.ReloadAllBoards();
    }
}