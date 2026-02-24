using GorillaGameModes;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Better Tag Aura", "Reports a tag when someone is near", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class BetterTagAura : hamburburmod
{
    private const int Range = 3;

    protected override void Update()
    {
        if (TagAuraRG.IsEnabled)
            if (InputManager.Instance.RightGrip.IsReleased)
            {
                GorillaTagger.Instance.maxTagDistance = 1.2f;

                return;
            }

        if (!VRRig.LocalRig.IsTagged())
            return;

        float closestDistance     = float.MaxValue;
        VRRig closestNonTaggedRig = null;

        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
        {
            if (vrrig == null || vrrig.isLocal)
                continue;

            if (vrrig.IsTagged())
                continue;

            float distance = Vector3.Distance(VRRig.LocalRig.transform.position, vrrig.transform.position);

            if (distance > closestDistance)
                continue;

            closestDistance     = distance;
            closestNonTaggedRig = vrrig;
        }

        if (closestNonTaggedRig != null && closestDistance <= Range)
            GameMode.ReportTag(closestNonTaggedRig.OwningNetPlayer());
    }

    protected override void OnDisable() => GorillaTagger.Instance.maxTagDistance = 1.2f;
}