using System.Reflection;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Menu Size: ", "Change the menu's size", ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled,
        10)]
public class ChangeMenuSize : hamburburmod
{
    private const int MinRange = 5;
    private const int MaxRange = 30;

    public static ChangeMenuSize Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + IncrementalValue * 0.1f;

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxRange) IncrementalValue = MinRange;
        
        GUI.MenuHandler.Instance.Menu.transform.parent.localScale = Vector3.one * (IncrementalValue * 0.1f);
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinRange) IncrementalValue = MaxRange;
        
        GUI.MenuHandler.Instance.Menu.transform.parent.localScale = Vector3.one * (IncrementalValue * 0.1f);
    }
}