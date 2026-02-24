using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.OP;

[hamburburmod("Tick Does Something", "Does something in tick!", ButtonType.Togglable, AccessSetting.BetaBuildOnly,
        EnabledType.Disabled, 0)]
public class TickDoesSomething : hamburburmod
{
    private const float Delay = 0.2f;

    private float lastTime;

    protected override void Update()
    {
        if (lastTime + Delay < Time.time)
            return;

        lastTime = Time.time;
    }
}