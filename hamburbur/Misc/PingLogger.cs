using System;
using System.Collections.Generic;
using hamburbur.Components;
using HarmonyLib;
using Photon.Pun;

namespace hamburbur.Misc;

public class PingLogger : Singleton<PingLogger>
{
    public static readonly Dictionary<VRRig, int> PlayerPing = new();

    private void Start() => PlayerSerializePatch.OnPlayerSerialize += rig => { PlayerPing[rig] = GetPing(rig); };

    private static int GetPing(VRRig rig) =>
            (int)Math.Clamp(Math.Round(Math.Abs((rig.velocityHistoryList[0].time - PhotonNetwork.Time) * 1000)), 0,
                    int.MaxValue);

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.SerializeReadShared))]
    public static class PlayerSerializePatch
    {
        public static Action<VRRig> OnPlayerSerialize;
        public static bool          Prefix() => true;

        public static void Postfix(VRRig __instance, InputStruct data) =>
                OnPlayerSerialize?.Invoke(__instance);
    }
}