using System.Collections;
using System.Linq;
using GorillaGameModes;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Tag All", "Tag people from afar", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagAll : hamburburmod
{
    protected override void OnEnable() => CoroutineManager.Instance.StartCoroutine(TryTagAll());

    private IEnumerator TryTagAll()
    {
        if (!VRRig.LocalRig.IsTagged())
        {
            NotificationManager.SendNotification(
                    "<color=red>Error</color>",
                    "You're not tagged!",
                    5f,
                    true,
                    false);

            Toggle(ButtonState.Normal, false, false);

            yield break;
        }

        RigUtils.ToggleRig(false);

        foreach (VRRig rig in
                 GorillaParent.instance.vrrigs.Where(rig => rig != null && !rig.isLocal && !rig.IsTagged()))
            yield return CoroutineManager.Instance.StartCoroutine(TryTagPlayer(rig));

        RigUtils.ToggleRig(true);
        Toggle(ButtonState.Normal, false, false);
    }

    private IEnumerator TryTagPlayer(VRRig rigToTag)
    {
        bool        hasReportedTag = false;
        const float Timeout        = 20f;
        float       timer          = 0f;
        while (timer < Timeout && !rigToTag.IsTagged())
        {
            timer += Time.deltaTime;

            RigUtils.RigPosition = rigToTag.transform.position + new Vector3(0f, -3f, 0f);

            GTPlayer.Instance.leftHand.controllerTransform.position = rigToTag.transform.position;
            VRRig.LocalRig.leftHand.rigTarget.transform.position    = rigToTag.transform.position;

            if (timer > 1f && !hasReportedTag)
            {
                GameMode.ReportTag(rigToTag.OwningNetPlayer());
                hasReportedTag = true;
            }

            yield return null;
        }
    }
}