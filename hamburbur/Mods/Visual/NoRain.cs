using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Visual;

[hamburburmod("No Rain", "Disables rain", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class NoRain : hamburburmod
{
    private BetterDayNightManager.WeatherType[] originalWeatherCycle;

    protected override void OnEnable()
    {
        BetterDayNightManager manager = BetterDayNightManager.instance;

        originalWeatherCycle = new BetterDayNightManager.WeatherType[manager.weatherCycle.Length];
        for (int i = 0; i < manager.weatherCycle.Length; i++)
            originalWeatherCycle[i] = manager.weatherCycle[i];

        for (int i = 0; i < manager.weatherCycle.Length; i++)
            manager.weatherCycle[i] = BetterDayNightManager.WeatherType.None;
    }
    
    protected override void OnDisable()
    {
        BetterDayNightManager manager = BetterDayNightManager.instance;

        if (originalWeatherCycle == null || originalWeatherCycle.Length != manager.weatherCycle.Length)
            return;

        for (int i = 0; i < manager.weatherCycle.Length; i++)
            manager.weatherCycle[i] = originalWeatherCycle[i];
    }
}