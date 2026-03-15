using ExitGames.Client.Photon;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.OP;

[hamburburmod(
        "Gun Does Something",
        "A gun that does something!",
        ButtonType.Togglable,
        AccessSetting.BetaBuildOnly,
        EnabledType.Disabled,
        0
)]
public class GunDoesSomething : hamburburmod
{
    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    private float lastTime;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null && lastTime + 3f < Time.time)
            lastTime = Time.time;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}