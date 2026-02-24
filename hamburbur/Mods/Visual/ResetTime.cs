using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Visual;

[hamburburmod("Reset Time", "Resets the time to Gorilla Tag's default time", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class ResetTime : hamburburmod
{
    protected override void Pressed()
    {
        BetterDayNightManager.instance.currentSetting = TimeSettings.Normal;
        TimeSetter.NormalTime                         = true;
    }
}