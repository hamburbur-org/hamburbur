using GorillaNetworking;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Room;


// cannot wait for this
[hamburburmod("Join zellix1", "Joins code Zellix1 because this is where a lot of cool people meet to mess around.", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinPeakest : hamburburmod
{
    protected override void Pressed() =>
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("ZELLIX1", JoinType.Solo);
}
