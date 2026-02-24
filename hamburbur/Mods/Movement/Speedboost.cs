using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;

namespace hamburbur.Mods.Movement;

[hamburburmod("Speedboost", "Gives you a speedboost", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Speedboost : hamburburmod
{
    protected override void FixedUpdate()
    {
        GTPlayer.Instance.maxJumpSpeed   = 6.5f * (SpeedBoostMultiplier.Instance.IncrementalValue / 100f);
        GTPlayer.Instance.jumpMultiplier = 1.1f * (SpeedBoostMultiplier.Instance.IncrementalValue / 100f);
    }
}