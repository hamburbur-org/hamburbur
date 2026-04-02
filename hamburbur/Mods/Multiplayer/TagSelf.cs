using System.Collections;
using System.Linq;
using GorillaGameModes;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Tag Self", "Go to tagged player", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagSelf : hamburburmod
{
    protected override void OnEnable() => CoroutineManager.Instance.StartCoroutine(GetTagged());

    private IEnumerator GetTagged()
    {
        if (VRRig.LocalRig.IsTagged())
        {
            NotificationManager.SendNotification(
                "<color=red>Error</color>",
                "You're already tagged!",
                5f,
                true,
                false);

            Toggle(ButtonState.Normal, false, false);
            yield break;
        }
        
        if (Tools.Utils.IsMasterClient)
        {
            TagManager.Instance.AddInfected(PhotonNetwork.LocalPlayer);
            yield break;
        }
        VRRig taggedPlayer = VRRigCache.m_activeRigs
            .FirstOrDefault(rig => rig != null && !rig.isLocal && rig.IsTagged());

        if (taggedPlayer == null)
        {
            NotificationManager.SendNotification(
                "<color=red>Error</color>",
                "No tagged players found!",
                5f,
                true,
                false);

            Toggle(ButtonState.Normal, false, false);
            yield break;
        }

        RigUtils.ToggleRig(false);

        const float Timeout = 8f;
        float timer = 0f;

        while (timer < Timeout && !VRRig.LocalRig.IsTagged())
        {
            timer += Time.deltaTime;

            Transform targetHand = taggedPlayer.rightHand.rigTarget.transform;

            RigUtils.RigPosition = targetHand.position;

            yield return null;
        }

        RigUtils.ToggleRig(true);
        Toggle(ButtonState.Normal, false, false);
    }
}