using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod("Teleport All Gun", "A gun that lets you teleport everyone that has console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class TpAllGun : hamburburmod
{
    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting)
            Components.Console.ExecuteCommand("tp", ReceiverGroup.Others,
                    gunLib.Hit.point);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}