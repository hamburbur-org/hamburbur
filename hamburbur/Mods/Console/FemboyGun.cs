using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Console;

[hamburburmod("Femboy Gun", "Jarvis says 'I like femboys' through the selected players mic", ButtonType.Togglable,
        AccessSetting.SuperAdminOnly, EnabledType.Disabled, 0)]
public class FemboyGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };
    private          bool   wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
        {
            if (wasShooting)
                return;

            Components.Console.ExecuteCommand("sb", gunLib.ChosenRig.Creator.ActorNumber,
                    "https://files.hamburbur.org/ilikefemboys.mp3");

            wasShooting = true;
        }
        else
        {
            wasShooting = false;
        }
    }

    protected override void OnDisable() => gunLib.OnDisable();
}