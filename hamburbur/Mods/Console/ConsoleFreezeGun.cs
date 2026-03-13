using hamburbur.Libs;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod(                   "Freeze Gun", "A gun that lets you freeze anyone who has console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class ConsoleFreezeGun : hamburburmod
{
    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    private float lastLagTime;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null || !(Time.time - lastLagTime > 0.1f))
            return;

        lastLagTime = Time.time;
        Components.Console.ExecuteCommand("sleep", gunLib.ChosenRig.Creator.ActorNumber, 1000);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}