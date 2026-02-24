using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;

namespace hamburbur.Mods.Movement;

[hamburburmod("Dash", "Press your right primary to dash forward", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Dash : hamburburmod
{
    protected override void Update()
    {
        if (InputManager.Instance.RightPrimary.WasPressed)
            GorillaTagger.Instance.rigidbody.linearVelocity +=
                    GTPlayer.Instance.headCollider.transform.forward * ChangeFlySpeed.Instance.IncrementalValue;
    }
}