using GorillaNetworking;
using hamburbur.Mod_Backend;

namespace hamburbur.Misc;

[hamburburmod(                "Anti AFK", "Disable the AFK kick thangylang", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class AntiAFK : hamburburmod
{
    protected override void OnEnable()  => PhotonNetworkController.Instance.disableAFKKick = true;
    protected override void OnDisable() => PhotonNetworkController.Instance.disableAFKKick = false;
}