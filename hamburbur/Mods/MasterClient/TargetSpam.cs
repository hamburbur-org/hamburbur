using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.MasterClient;

[hamburburmod(                "Target Spam", "Spams all the targets if you're master client", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class TargetSpam : hamburburmod
{
    private HitTargetNetworkState[] hitTargetNetworkStates;

    protected override void OnEnable() => hitTargetNetworkStates =
                                                  Object.FindObjectsByType<HitTargetNetworkState>(FindObjectsSortMode
                                                         .None);

    protected override void Update()
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

            return;
        }

        foreach (HitTargetNetworkState hitTargetNetworkState in hitTargetNetworkStates)
        {
            Vector3 targetPosition = hitTargetNetworkState.transform.position;

            hitTargetNetworkState.hitCooldownTime = 0;
            hitTargetNetworkState.TargetHit(targetPosition, targetPosition);
        }
    }
}