using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod(                   "Kick Gun", "A gun that lets you kick anyone who has console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class KickGun : hamburburmod
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

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
            Components.Console.ExecuteCommand("kick", ReceiverGroup.All,
                    gunLib.ChosenRig.Creator.UserId);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}