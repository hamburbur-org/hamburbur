using System.Collections;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.MasterClient;

[hamburburmod("Guardian Spam", "Spam changes whos the Guardian", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class GuardianSpam : hamburburmod
{
    private Coroutine guardianSpamRoutine;

    protected override void OnEnable()
    {
        if (!Tools.Utils.IsMasterClient)
        {
            NotificationManager.SendNotification(
                    "<color=red>Error</color>",
                    "You are not master client.",
                    5f,
                    false,
                    false);
            
            Toggle(ButtonState.Normal, false, false);
        }

        guardianSpamRoutine = CoroutineManager.Instance.StartCoroutine(GuardianSpamCoroutine());
    }

    private IEnumerator GuardianSpamCoroutine()
    {
        while (Tools.Utils.IsMasterClient)
        {
            GorillaGuardianZoneManager[] zones = GorillaGuardianZoneManager.zoneManagers
                                                                           .Where(z => z != null && z.enabled && z.IsZoneValid())
                                                                           .ToArray();

            Player[] players = PhotonNetwork.PlayerListOthers
                                            .Where(p => p != null)
                                            .ToArray();

            if (zones.Length == 0 || players.Length == 0)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            GorillaGuardianZoneManager zone   = zones[Random.Range(0,   zones.Length)];
            Player                     player = players[Random.Range(0, players.Length)];

            zone.SetGuardian(player);
            
            yield return new WaitForSeconds(0.5f);

            if (zone != null && Equals(zone.CurrentGuardian.GetPlayerRef(), player))
                zone.SetGuardian(null);
        }
        
        Toggle(ButtonState.Normal, false, false);
    }

    protected override void OnDisable()
    {
        if (guardianSpamRoutine == null)
            return;

        CoroutineManager.Instance.StopCoroutine(guardianSpamRoutine);
        guardianSpamRoutine = null;

        if (!Tools.Utils.IsMasterClient)
            return;

        foreach (GorillaGuardianZoneManager zone in GorillaGuardianZoneManager.zoneManagers.Where(z => z != null && z.enabled && z.IsZoneValid()))
            zone.SetGuardian(null);
    }
}