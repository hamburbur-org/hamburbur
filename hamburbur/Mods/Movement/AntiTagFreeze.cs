using GorillaLocomotion;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Movement;

[hamburburmod("Anti Tag Freeze", "Makes it so you can move after getting tagged", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class AntiTagFreeze : hamburburmod
{
    protected override void Update()
    {
        if (GTPlayer.Instance.disableMovement)
            GTPlayer.Instance.disableMovement = false;
    }
}