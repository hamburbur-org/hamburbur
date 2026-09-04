using System;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Grip Speed Boost",   "Applies your speed boost multiplier while either grip is held", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GripSpeedBoost : hamburburmod
{
    private bool  boosting;
    private float previousJumpMultiplier;
    private float previousMaxJumpSpeed;

    protected override Type[] IncompatibleMods => [typeof(Speedboost),];

    protected override void FixedUpdate()
    {
        GTPlayer player = GTPlayer.Instance;

        if (player == null || InputManager.Instance == null)
            return;

        bool shouldBoost = InputManager.Instance.LeftGrip.IsPressed || InputManager.Instance.RightGrip.IsPressed;

        if (!shouldBoost)
        {
            RestoreMovement();

            return;
        }

        if (!boosting)
        {
            previousMaxJumpSpeed   = player.maxJumpSpeed;
            previousJumpMultiplier = player.jumpMultiplier;
            boosting               = true;
        }

        float multiplier = SpeedBoostMultiplier.Instance.IncrementalValue / 100f;
        player.maxJumpSpeed   = previousMaxJumpSpeed   * multiplier;
        player.jumpMultiplier = previousJumpMultiplier * multiplier;
    }

    protected override void OnDisable() => RestoreMovement();

    private void RestoreMovement()
    {
        if (!boosting || GTPlayer.Instance == null)
            return;

        GTPlayer.Instance.maxJumpSpeed   = previousMaxJumpSpeed;
        GTPlayer.Instance.jumpMultiplier = previousJumpMultiplier;
        boosting                         = false;
    }
}