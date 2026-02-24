using GorillaLocomotion;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Misc;

[hamburburmod("Hoverboards Everywhere", "Hoverboards can be used anywhere", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class HoverboardsAnywhere : hamburburmod
{
    protected override void OnEnable()  => GTPlayer.Instance.SetHoverAllowed(true);
    protected override void OnDisable() => GTPlayer.Instance.SetHoverAllowed(false);
}