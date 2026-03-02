using hamburbur.Libs;
using hamburbur.Mod_Backend;
using Photon.Realtime;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod(                   "Lightning Gun", "A gun that lets you summon lightning", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class LightningGun : hamburburmod
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
            Components.Console.ExecuteCommand("strike", ReceiverGroup.All,
                    gunLib.Hit.point);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}