using GorillaNetworking;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Room;

[hamburburmod("Join peakest", "Makes you join the og code us developers use to test stuff and play together", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinPeakest : hamburburmod
{
    protected override void Pressed() =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("PEAKEST", JoinType.Solo);
}