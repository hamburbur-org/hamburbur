using System;
using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Gun Type: ", "Lets you change the gun type", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 2)]
public class ChangeGunType : hamburburmod
{
    private static GunType[] cachedGunTypes;

    public override string ModName => AssociatedAttribute.Name + GunLib.GunType;

    protected override void Increment()
    {
        EnsureCache();

        IncrementalValue++;
        if (IncrementalValue >= cachedGunTypes.Length)
            IncrementalValue = 0;

        GunLib.GunType = cachedGunTypes[IncrementalValue];
    }

    protected override void Decrement()
    {
        EnsureCache();

        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = cachedGunTypes.Length - 1;

        GunLib.GunType = cachedGunTypes[IncrementalValue];
    }

    private static void EnsureCache()
    {
        if (cachedGunTypes != null)
            return;

        cachedGunTypes = (GunType[])Enum.GetValues(typeof(GunType));
    }

    protected override void OnIncrementalStateLoaded()
    {
        EnsureCache();
        GunLib.GunType = cachedGunTypes[IncrementalValue];
    }
}