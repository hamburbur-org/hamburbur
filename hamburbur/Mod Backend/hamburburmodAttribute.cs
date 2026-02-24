using System;

namespace hamburbur.Mod_Backend;

[AttributeUsage(AttributeTargets.Class)]
public class hamburburmodAttribute(
        string        name,
        string        description,
        ButtonType    buttonType,
        AccessSetting accessSetting,
        EnabledType   enabledType,
        int           incrementalValue) : Attribute
{
    public readonly AccessSetting AccessSetting    = accessSetting;
    public readonly ButtonType    ButtonType       = buttonType;
    public readonly string        Description      = description;
    public readonly EnabledType   EnabledType      = enabledType;
    public readonly int           IncrementalValue = incrementalValue;
    public readonly string        Name             = name;
}