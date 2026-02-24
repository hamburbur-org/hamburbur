using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MasterClient;

[hamburburmod(                "Spam Gray Zone", "Read the title dumbahh", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SpamGrayZone : hamburburmod
{
    private const float Delay = 0.08f;

    private GreyZoneManager[] greyZones;

    private float lastTime;

    protected override void OnEnable() => greyZones = Object.FindObjectsOfType<GreyZoneManager>();

    protected override void Update()
    {
        if (!(lastTime + Delay < Time.time) || !Tools.Utils.IsMasterClient)
            return;

        lastTime = Time.time;

        foreach (GreyZoneManager greyZone in greyZones)
            if (greyZone.GreyZoneActive)
            {
                greyZone.DeactivateGreyZoneAuthority();
                greyZone.gravityFactorOptionSelection = 0;
            }
            else
            {
                greyZone.greyZoneAmbienceVolume = float.MaxValue;
                greyZone.ActivateGreyZoneAuthority();
                greyZone.gravityFactorOptionSelection = 100;
            }
    }

    protected override void OnDisable()
    {
        foreach (GreyZoneManager greyZone in greyZones)
            if (greyZone.GreyZoneActive)
            {
                greyZone.greyZoneAmbienceVolume = 1f;
                greyZone.DeactivateGreyZoneAuthority();
            }
    }
}