using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Wall Assist", "Helps you clims up surfaces whilst holding Left Grip", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class WallAssist : hamburburmod
{
    public static float AssistStrength = -3f;

    private Vector3 u;
    private Vector3 x;

    protected override void Update()
    {
        AssistStrength = -WallAssistStrength.Instance.IncrementalValue;

        if (GTPlayer.Instance.IsHandTouching(true) || GTPlayer.Instance.IsHandTouching(false))
        {
            RaycastHit hit = GTPlayer.Instance.lastHitInfoHand;
            x = hit.point;
            u = hit.normal;
        }

        if (x != Vector3.zero && InputManager.Instance.LeftGrip.IsPressed)
            GorillaTagger.Instance.rigidbody.AddForce(u * AssistStrength, ForceMode.Acceleration);
    }
}