using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Tools;

[AttributeUsage(AttributeTargets.Method)]
public class AccessSettingsAllowedCheckAttribute(AccessSetting accessSetting) : Attribute
{
    public AccessSetting AccessSetting = accessSetting;
}