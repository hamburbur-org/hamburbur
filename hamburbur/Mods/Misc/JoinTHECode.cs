using GorillaNetworking;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Misc;

[hamburburmod("Join hamburbur code", "Makes you join the hamburbur code", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinTHECode : hamburburmod
{
    protected override void Pressed() =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("PEAKEST", JoinType. /*Han*/Solo /*1000Falcon*/);
}