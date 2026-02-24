using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MasterClient;

[hamburburmod(                "Gray Zone", "Enabled the gray zone", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GrayZone : hamburburmod
{
    private GreyZoneManager[] greyZones;

    protected override void OnEnable()
    {
        greyZones = Object.FindObjectsOfType<GreyZoneManager>();

        foreach (GreyZoneManager greyZone in greyZones)
        {
            greyZone.greyZoneAmbienceVolume = float.MaxValue;
            greyZone.forceTimeOfDayToNight  = true;
            greyZone.greyZoneActiveDuration = float.MaxValue;
            greyZone.ActivateGreyZoneAuthority();
            greyZone.gravityFactorOptionSelection = 100;
        }
    }

    protected override void OnDisable()
    {
        foreach (GreyZoneManager greyZone in greyZones)
        {
            greyZone.greyZoneAmbienceVolume = 1f;
            greyZone.DeactivateGreyZoneAuthority();
            greyZone.gravityFactorOptionSelection = 0;
        }
    }
}