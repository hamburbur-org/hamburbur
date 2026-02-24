using System;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Rig;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "PC Rig Mode: ", "Changes the pc rig mode", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 1)]
public class ChangePCRig : hamburburmod
{
    private static RigMode[] CachedRigModes;

    public override string ModName => AssociatedAttribute.Name + PCRig.CurrentRigMode;

    protected override void Increment()
    {
        EnsureCache();

        IncrementalValue++;
        if (IncrementalValue >= CachedRigModes.Length)
            IncrementalValue = 0;

        PCRig.CurrentRigMode = CachedRigModes[IncrementalValue];
    }

    protected override void Decrement()
    {
        EnsureCache();

        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = CachedRigModes.Length - 1;

        PCRig.CurrentRigMode = CachedRigModes[IncrementalValue];
    }

    private static void EnsureCache()
    {
        if (CachedRigModes != null)
            return;

        CachedRigModes = (RigMode[])Enum.GetValues(typeof(RigMode));
    }

    protected override void OnIncrementalStateLoaded()
    {
        EnsureCache();
        PCRig.CurrentRigMode = CachedRigModes[IncrementalValue];
    }
}