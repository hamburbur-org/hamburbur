using hamburbur.Libs;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod("Break Game Gun", "A gun that lets you break anyones game if they have console", ButtonType.Togglable,
        AccessSetting.SuperAdminOnly, EnabledType.Disabled, 0)]
public class BreakGameGun : hamburburmod
{
    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    private float eventDelay;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null || Time.time < eventDelay)
            return;

        eventDelay = Time.time + 0.1f;
        Components.Console.ExecuteCommand("tp", gunLib.ChosenRig.Creator.ActorNumber,
                new Vector3(0f, 1000000f, 0f));
    }

    protected override void OnDisable() => gunLib.OnDisable();
}