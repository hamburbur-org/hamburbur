using GorillaNetworking;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Room;

[hamburburmod("Join hamburbur code", "Makes you join the hamburbur code", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinPeakest : hamburburmod
{
    protected override void Pressed() =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("PEAKEST", JoinType.Solo);
}