using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Misc;

[hamburburmod("Sound: ", "", ButtonType.Fixed, AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class TestSoundEffectGT : hamburburmod
{
    public static   int    CurrentSoundID;
    public override string ModName => AssociatedAttribute.Name + CurrentSoundID;

    protected override void Pressed() => VRRig.LocalRig.PlayHandTapLocal(CurrentSoundID, false, 5f);
}

[hamburburmod(                       "Change Test gt sounds", "Play Different Sounds from GT", ButtonType.Incremental,
        AccessSetting.BetaBuildOnly, EnabledType.Disabled,    0)]
public class ChangeTestSoundEffectGT : hamburburmod
{
    protected override void Increment()
    {
        IncrementalValue++;
        TestSoundEffectGT.CurrentSoundID = IncrementalValue;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = 0;

        TestSoundEffectGT.CurrentSoundID = IncrementalValue;
    }
}