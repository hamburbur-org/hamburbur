using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
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
        
        materials.Add("Caves Spectral Goo",
                GameObject.Find(
                                   "Environment Objects/LocalObjects_Prefab/TreeRoom/SpectralGooPile (combined by EdMeshCombiner)")
                          .GetComponent<MeshRenderer>().material);
        
        materials.Add("Caves Purple Crystal", GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToCave/C_Crystal_Chunk").GetComponent<MeshRenderer>().material);
        
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

    protected override void OnIncrementalStateLoaded() => CustomBoardManager.Instance.ReloadAllBoards();
}