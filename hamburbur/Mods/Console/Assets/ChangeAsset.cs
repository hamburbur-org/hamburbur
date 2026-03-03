using System.Collections.Generic;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Change Asset: ", "Change the asset to spawn", ButtonType.Incremental, AccessSetting.AdminOnly,
        EnabledType.Disabled,
        5)]
public class ChangeAsset : hamburburmod
{
    public static readonly List<(string file, string prefabName, string displayName, Vector3 position, Quaternion
            rotation, Vector3 scale)> Assets =
    [
            ("consolehamburburassets", "Axe", "Axe", new Vector3(0.05f, 0.03f, 0f), Quaternion.Euler(0f, 0f, 90f),
             new Vector3(5,                                             5,     5)),
            ("consolehamburburassets", "bag", "Bag", new Vector3(0.05f, 0.03f, 0f), Quaternion.Euler(0f, 0f, 90f),
             new Vector3(5,                                             5,     5)),
            ("banhammer", "BanHammer", "Ban Hammer(dont work)", Vector3.zero, Quaternion.Euler(0f, 0f, 90f),
             new Vector3(5, 5, 5)),
            ("iphone", "iPhone", "Iphone", Vector3.zero, Quaternion.identity, new Vector3(5, 5, 5)),
            ("consolehamburburassets", "KormakurSign", "Kormakur Sign", new Vector3(0.29f, -0.2f, -0.1272f),
             Quaternion.Euler(355f, 275f, 265f), Vector3.one),
            ("clickbaitmenu‎", "Mod Menu", "Mod Menu", new Vector3(-0.09f, 0.125f, 0f), Quaternion.Euler(0f, 110f, 80f),
             Vector3.one),
            ("console.main1", "PhysicsGun", "Physics Gun", Vector3.zero, Quaternion.identity, new Vector3(5, 5, 5)),
            ("console.main1", "Sword", "Roblox Sword", Vector3.zero, Quaternion.identity, new Vector3(5,     5, 5)),
            ("consolehamburburassets", "Sword", "Sword", new Vector3(0.1f, 0.1f, 0.2f), Quaternion.Euler(0f, 90f, 90f),
             new Vector3(0.1f,                                             0.1f, 0.1f)),
    ];

    public static ChangeAsset Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + Assets[IncrementalValue].displayName;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > Assets.Count - 1) IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0) IncrementalValue = Assets.Count - 1;
    }
}