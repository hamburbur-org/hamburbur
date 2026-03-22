using System.Collections;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine.Networking;

namespace hamburbur.Mods.Misc;

[hamburburmod("Automatic Reporting System", "The ARS mod by industry implemented into hamburbur", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ARS : hamburburmod
{
    private const string PlayersToReportUrl =
            "https://raw.githubusercontent.com/AutoReportSystem/ARSPlayerIDs/refs/heads/main/Player%20Ids.txt";

    private string[] playersToReport;

    protected override void Start() =>
            CoroutineManager.Instance.StartCoroutine(FetchPeopleToReport());

    protected override void OnEnable() =>
            RigUtils.OnRigLoaded += OnRigLoaded;

    protected override void OnDisable() =>
            RigUtils.OnRigLoaded -= OnRigLoaded;

    private IEnumerator FetchPeopleToReport()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(PlayersToReportUrl);

        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.Success)
            playersToReport = webRequest.downloadHandler.text.Split(",").Select(id => id.Trim())
                                        .Where(id => !id.IsNullOrEmpty()).ToArray();
    }

    private void OnRigLoaded(VRRig rig)
    {
        if (playersToReport == null || !playersToReport.Contains(rig.creator.UserId))
            return;

        NotificationManager.SendNotification("<color=#33ccff>ARS</color>",
                $"Player {rig.creator.SanitizedNickName} is on the ARS list, reporting...", 5f, true,
                false);

        foreach (GorillaPlayerScoreboardLine scoreboardLine in
                 GorillaScoreboardTotalUpdater.allScoreboardLines.Where(scoreboardLine =>
                                                                                scoreboardLine.playerVRRig ==
                                                                                rig))
        {
            scoreboardLine.reportedToxicity = true;
            scoreboardLine.PressButton(true, GorillaPlayerLineButton.ButtonType.Toxicity);
        }
    }
}