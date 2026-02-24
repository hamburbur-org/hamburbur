using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Scoreboard;

[hamburburmod(                "Teleport To", "Teleport to the selected player", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class TeleportTo : hamburburmod
{
    private void OnPress() => Tools.Utils.TeleportPlayer(PlayerLine.CurrentRig.transform.position);
}