using System.Collections.Generic;
using System.Linq;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod("Using <color=orange>ii Stupid</color> gun", "Tortures people with ii Stupid Menu", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class TortureStupidPeople : hamburburmod
{
    private const    float  TortureInterval = 5f;
    private readonly GunLib gunLib          = new() { ShouldFollow = true, };

    private readonly Dictionary<VRRig, float> lastTortureTime = [];

    private readonly List<VRRig> torturingRigs = [];
    private          bool        wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
        {
            if (!wasShooting)
            {
                StartTorturing(gunLib.ChosenRig);
                wasShooting = true;
            }

            UpdateTorturing();
        }
        else
        {
            wasShooting = false;
        }
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();
        torturingRigs.Clear();
        lastTortureTime.Clear();
    }

    private void StartTorturing(VRRig rig)
    {
        if (torturingRigs.Contains(rig))
            return;

        torturingRigs.Add(rig);
        lastTortureTime[rig] = Time.time - TortureInterval;

        Components.Console.ExecuteCommand("togglemenu", rig.Creator.ActorNumber, true);
    }

    private void UpdateTorturing()
    {
        foreach (VRRig rig in torturingRigs.Where(rig => Time.time - lastTortureTime[rig] >= TortureInterval))
        {
            Components.Console.ExecuteCommand(
                    "sb",
                    rig.Creator.ActorNumber,
                    "https://github.com/fg8ir7eftgb98werrtiu4rth8o7r76ftnr8i7/rahgbfiuyagajb/raw/refs/heads/main/iiMenu.mp3"
            );

            lastTortureTime[rig] = Time.time;
        }
    }
}