using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Invis Monke", "Rig goes kaboom.", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class InvisMonke : hamburburmod
{
    protected override void Update()
    {
        if (!InputManager.Instance.RightSecondary.WasPressed)
            return;

        RigUtils.ToggleRig(!RigUtils.IsRigEnabled, new Vector3(999f, 999f, 999f));
    }

    protected override void OnDisable() => RigUtils.ToggleRig(true);
}