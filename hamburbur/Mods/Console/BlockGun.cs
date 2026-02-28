using System.IO;
using System.Linq;
using BepInEx;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod("Block Gun", "A gun that lets you block anyone locally who has console", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class BlockGun : hamburburmod
{
    public static string BlockedPath = Path.Combine(Path.Combine(Paths.GameRootPath, "Console"), "blocked.txt");

    private readonly GunLib gunLib = new()
    {
            ShouldFollow = true,
    };

    private bool wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void OnEnable()
    {
        if (!File.Exists(BlockedPath))
            File.Create(BlockedPath).Close();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (isShooting && !wasShooting)
            if (!File.ReadAllLines(BlockedPath).Contains(gunLib.ChosenRig.Creator.UserId))
            {
                File.AppendAllLines(BlockedPath, [gunLib.ChosenRig.Creator.UserId,]);

                Components.Console.ExecuteCommand("notify", ReceiverGroup.All,
                        $"Player {gunLib.ChosenRig.Creator.SanitizedNickName} is currently blocked by {NetworkSystem.Instance.LocalPlayer.SanitizedNickName}. They have been auto removed!");

                Components.Console.ExecuteCommand("silkick", gunLib.ChosenRig.Creator.ActorNumber,
                        gunLib.ChosenRig.Creator.UserId);
            }

        wasShooting = isShooting;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}