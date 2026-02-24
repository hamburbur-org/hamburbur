using System;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;

namespace hamburbur.Mods.Movement;

[hamburburmod("Modify Gravity", "Alters what gravity in-game", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GravityModifier : hamburburmod
{
    private GravityModifierTypes last;

    protected override void Update()
    {
        GravityModifierTypes current = GravityModifierType.Current;

        if (current != last)
        {
            RigUtils.DisableZeroGravity();
            RigUtils.DisableHighGravity();
            RigUtils.DisableLowGravity();
            RigUtils.DisableReverseGravity();

            switch (current)
            {
                case GravityModifierTypes.High:
                    RigUtils.EnableHighGravity();

                    break;

                case GravityModifierTypes.Low:
                    RigUtils.EnableLowGravity();

                    break;

                case GravityModifierTypes.Reverse:
                    RigUtils.EnableReverseGravity();

                    break;

                case GravityModifierTypes.Zero:
                    RigUtils.EnableZeroGravity();

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        last = current;
    }

    protected override void OnEnable()
    {
        switch (GravityModifierType.Current)
        {
            case GravityModifierTypes.High:
                RigUtils.EnableHighGravity();

                break;

            case GravityModifierTypes.Low:
                RigUtils.EnableLowGravity();

                break;

            case GravityModifierTypes.Reverse:
                RigUtils.EnableReverseGravity();

                break;

            case GravityModifierTypes.Zero:
                RigUtils.EnableZeroGravity();

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void OnDisable()
    {
        switch (GravityModifierType.Current)
        {
            case GravityModifierTypes.High:
                RigUtils.DisableHighGravity();

                break;

            case GravityModifierTypes.Low:
                RigUtils.DisableLowGravity();

                break;

            case GravityModifierTypes.Reverse:
                RigUtils.DisableReverseGravity();

                break;

            case GravityModifierTypes.Zero:
                RigUtils.DisableZeroGravity();

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}