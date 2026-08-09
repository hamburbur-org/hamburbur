using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;

namespace hamburbur.Mods.Movement;

[hamburburmod(                nameof(Speedboost), "Gives you a speedboost", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Speedboost : hamburburmod
{
    private float previousJumpMultiplier;
    private float previousMaxJumpSpeed;

    protected override void OnEnable()
    {
        if (GTPlayer.Instance == null)
            return;

        previousMaxJumpSpeed   = GTPlayer.Instance.maxJumpSpeed;
        previousJumpMultiplier = GTPlayer.Instance.jumpMultiplier;
    }

    protected override void FixedUpdate()
    {
        if (GTPlayer.Instance == null)
            return;

        GTPlayer.Instance.maxJumpSpeed   = 6.5f * (SpeedBoostMultiplier.Instance.IncrementalValue / 100f);
        GTPlayer.Instance.jumpMultiplier = 1.1f * (SpeedBoostMultiplier.Instance.IncrementalValue / 100f);
    }

    protected override void OnDisable()
    {
        if (GTPlayer.Instance == null)
            return;

        GTPlayer.Instance.maxJumpSpeed   = previousMaxJumpSpeed;
        GTPlayer.Instance.jumpMultiplier = previousJumpMultiplier;
    }
}