using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Visual;

[hamburburmod("Rain", "Enable rain", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Rain : hamburburmod
{
    protected override void Pressed()
    {
        for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
            BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.Raining;
    }
}