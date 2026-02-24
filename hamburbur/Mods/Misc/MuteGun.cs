using System.Linq;
using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Misc;

[hamburburmod(                "Mute Gun", "Lets you mute people from afar!", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MuteGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null)
            return;

        foreach (GorillaPlayerScoreboardLine scoreboardLine in
                 GorillaScoreboardTotalUpdater.allScoreboardLines.Where(scoreboardLine =>
                                                                                scoreboardLine.playerVRRig ==
                                                                                gunLib.ChosenRig))
        {
            scoreboardLine.muteButton.isOn = true;
            scoreboardLine.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
        }
    }

    protected override void OnDisable() => gunLib.OnDisable();
}