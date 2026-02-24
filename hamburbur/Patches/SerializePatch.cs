using System;
using HarmonyLib;
using Photon.Pun;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.RunViewUpdate))]
public class SerializePatch
{
    public static Func<bool>   OverrideSerialization;
    public static event Action OnSerialize;

    public static bool Prefix()
    {
        if (!PhotonNetwork.InRoom)
            return true;

        try
        {
            OnSerialize?.Invoke();
        }
        catch
        {
            //ignored
        }

        if (OverrideSerialization == null)
            return true;

        try
        {
            return OverrideSerialization();
        }
        catch
        {
            return false;
        }
    }
}