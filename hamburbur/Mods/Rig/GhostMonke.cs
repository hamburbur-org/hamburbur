using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;

namespace hamburbur.Mods.Rig;

[hamburburmod(                "Ghost Monke", "Freeze your rig in place", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GhostMonke : hamburburmod
{
    protected override void Update()
    {
        if (!InputManager.Instance.RightPrimary.WasPressed)
            return;

        RigUtils.ToggleRig(!RigUtils.IsRigEnabled);
    }

    protected override void OnDisable() => RigUtils.ToggleRig(true);
}