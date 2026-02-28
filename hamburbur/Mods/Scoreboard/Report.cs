using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;

namespace hamburbur.Mods.Scoreboard;

[hamburburmod(                "Report", "Report the selected player", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Report : hamburburmod
{
    public static List<(GorillaPlayerLineButton.ButtonType type, string name)> reportTypes =
    [
        (GorillaPlayerLineButton.ButtonType.Cheating, "Cheating"),
        (GorillaPlayerLineButton.ButtonType.HateSpeech, "Hate Speech"),
        (GorillaPlayerLineButton.ButtonType.Toxicity, "Toxicity")
    ];

    protected override void Pressed()
    {
        NetPlayer player = PlayerLine.CurrentRig.creator;

        if (player == null || player.IsNull) return;

        GorillaPlayerScoreboardLine[] lines = GorillaScoreboardTotalUpdater.allScoreboards
            .SelectMany(s => s.lines)
            .Where(l => l.playerActorNumber == player.ActorNumber ||
                        player.Equals(l.linePlayer))
            .ToArray();

        for (int i = 0; i < lines.Length; i++)
            if (i == 0)
            {
                lines[i].reportButton.isOn = true;
                lines[i].PressButton(true, reportTypes[ChangeReportType.Instance.IncrementalValue].type);
            }
            else
            {
                lines[i].InitializeLine();
            }
    }
}