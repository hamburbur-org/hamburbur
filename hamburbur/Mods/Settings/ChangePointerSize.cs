using System.Reflection;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Pointer Size: ", "Change the menu's pointer size", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
        5)]
public class ChangePointerSize : hamburburmod
{
    private const int MinRange = 1;
    private const int MaxRange = 20;

    public static ChangePointerSize Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + IncrementalValue * 0.002f;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxRange) IncrementalValue = MinRange;

        GUI.MenuHandler.Instance.ButtonPresser.transform.localScale = Vector3.one * (IncrementalValue * 0.002f);
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinRange) IncrementalValue = MaxRange;
        
        GUI.MenuHandler.Instance.ButtonPresser.transform.localScale = Vector3.one * (IncrementalValue * 0.002f);
    }
}