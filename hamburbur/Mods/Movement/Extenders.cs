using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;
using AccessSetting = hamburbur.Mod_Backend.AccessSetting;
using hamburburmod = hamburbur.Mod_Backend.hamburburmod;

namespace hamburbur.Mods.Movement;

[hamburburmod("Extenders", "Makes your arm longer when you hold down the left joystick", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class Extenders : hamburburmod
{
    private float extendingTime;

    protected override void Update()
    {
        extendingTime += InputManager.Instance.LeftJoystick.IsPressed
                                 ? -Time.unscaledDeltaTime
                                 : Time.unscaledDeltaTime;

        if (extendingTime > 1) extendingTime = 1;
        if (extendingTime < 0) extendingTime = 0;

        float delayedLength = (ChangeArmLength.CurrentValue - 1f) * extendingTime + 1f;
        GTPlayer.Instance.transform.localScale = new Vector3(delayedLength, delayedLength, delayedLength);
    }

    protected override void OnDisable() =>
            GTPlayer.Instance.transform.localScale = Vector3.one * VRRig.LocalRig.NativeScale;
}