using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Tag Aura", "Increases players Tag hitbox's when holding dont right grip", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagAura : hamburburmod
{
    protected override void Update()
    {
        if (TagAuraRG.IsEnabled)
            if (InputManager.Instance.RightGrip.IsReleased && !TagFix.IsEnabled)
            {
                GorillaTagger.Instance.maxTagDistance = 1.2f;

                return;
            }

        if (!VRRig.LocalRig.IsTagged())
            return;

        GorillaTagger.Instance.maxTagDistance         = float.MaxValue;
        GorillaTagger.Instance.tagRadiusOverride      = 1f;
        GorillaTagger.Instance.tagRadiusOverrideFrame = Time.frameCount + 16;
    }

    protected override void OnDisable()
    {
        if (!TagAuraRG.IsEnabled)
            GorillaTagger.Instance.maxTagDistance = 1.2f;
    }
}