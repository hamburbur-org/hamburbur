using GorillaNetworking;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Room;

[hamburburmod("Join hamburbur code", "Makes you join the hamburbur code", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinHamburburCode : hamburburmod
{
    protected override void Pressed() =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("[hamburbur_menu]", JoinType.Solo);
}