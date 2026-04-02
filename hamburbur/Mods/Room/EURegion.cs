using System;
using hamburbur.Mod_Backend;
using Photon.Pun;

namespace hamburbur.Mods.Room;

[hamburburmod(                "EU Region", "Makes you join the region for EU", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class EURegion : hamburburmod
{
    protected override void Pressed() => ConnectToRegion("eu");

    private void ConnectToRegion(string region)
    {
        string currentRegion = PhotonNetwork.CloudRegion;
        if (!string.IsNullOrEmpty(currentRegion))
            currentRegion = currentRegion.Replace("/*", "");

        if (currentRegion != region)
            PhotonNetwork.ConnectToRegion(region);

        NetworkSystem.Instance.currentRegionIndex = Array.IndexOf(NetworkSystem.Instance.regionNames, region);

        NetworkSystemPUN punNetwork = (NetworkSystemPUN)NetworkSystem.Instance;
        for (int i = 0; i < punNetwork.regionData.Length; i++)
        {
            NetworkRegionInfo regionInfo = punNetwork.regionData[i];
            regionInfo.pingToRegion = Array.IndexOf(NetworkSystem.Instance.regionNames, regionInfo) == i ? 0 : 9999;
        }
    }
}