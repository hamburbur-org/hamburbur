using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Movement;

[hamburburmod("Teleport Gun", "Teleport to the gun position", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class TPGun : hamburburmod
{
    private readonly GunLib gunLib = new();

    private bool wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (isShooting && !wasShooting)
            Tools.Utils.TeleportPlayer(gunLib.Hit.point);

        wasShooting = isShooting;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}