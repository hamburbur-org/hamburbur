using System.Linq;
using hamburbur.Mod_Backend;
using GorillaNetworking;

namespace hamburbur.Mods.Scoreboard;

[hamburburmod(                "Mute", "Teleport to the selected player", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Mute : hamburburmod
{
    protected override void OnEnable()
    {
        var player = PlayerLine.CurrentRig.OwningNetPlayer;
        
        if (player == null || player.IsNull) return;

        var lines = GorillaScoreboardTotalUpdater.allScoreboards
            .SelectMany(s => s.lines)
            .Where(l => l.playerActorNumber == player.ActorNumber || player.Equals(l.linePlayer))
            .ToArray();

        for (int i = 0; i < lines.Length; i++)
            if (i == 0)
            {
                lines[i].muteButton.isOn = true;
                lines[i].PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
            }
            else
                lines[i].InitializeLine();
    }
    
    protected override void OnDisable()
    {
        var player = PlayerLine.CurrentRig.OwningNetPlayer;
        
        if (player == null || player.IsNull) return;

        var lines = GorillaScoreboardTotalUpdater.allScoreboards
            .SelectMany(s => s.lines)
            .Where(l => l.playerActorNumber == player.ActorNumber || player.Equals(l.linePlayer))
            .ToArray();

        for (int i = 0; i < lines.Length; i++)
            if (i == 0)
            {
                lines[i].muteButton.isOn = false;
                lines[i].PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
            }
            else
                lines[i].InitializeLine();
    }
}