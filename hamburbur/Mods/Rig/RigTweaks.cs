using System;
using System.Linq;
using BepInEx;
using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Rig Tweaks", "Rig go weeeeeeeee", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RigTweaks : hamburburmod
{
    public static bool       IsEnabled;
    public static Quaternion RigRotation = Quaternion.identity;

    protected override void Update()
    {
        if (!IsEnabled)
            return;

        float speed = 90f;

        if (UnityInput.Current.GetKey(KeyCode.UpArrow))
            RigRotation *= Quaternion.Euler(speed * Time.deltaTime, 0, 0);

        if (UnityInput.Current.GetKey(KeyCode.DownArrow))
            RigRotation *= Quaternion.Euler(-speed * Time.deltaTime, 0, 0);

        if (UnityInput.Current.GetKey(KeyCode.LeftArrow))
            RigRotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);

        if (UnityInput.Current.GetKey(KeyCode.RightArrow))
            RigRotation *= Quaternion.Euler(0, -speed * Time.deltaTime, 0);

        if (UnityInput.Current.GetKey(KeyCode.RightControl))
            RigRotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime);

        if (UnityInput.Current.GetKey(KeyCode.RightShift))
            RigRotation *= Quaternion.Euler(0, 0, -speed * Time.deltaTime);

        if (UnityInput.Current.GetKeyDown(KeyCode.R))
            RigRotation = Quaternion.identity;
    }

    protected override void OnEnable()
    {
        IsEnabled = true;

        try
        {
            if (PhotonNetwork.LocalPlayer != null)
            {
                Hashtable props = new()
                {
                        { "Gorilla Track 2.3.1", true },
                };

                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to set Gorilla Track property on enable: {e}");
        }
    }

    protected override void OnDisable()
    {
        IsEnabled = false;

        try
        {
            if (PhotonNetwork.LocalPlayer != null)
            {
                Hashtable removeProps = new()
                {
                        { "Gorilla Track 2.3.1", null },
                };

                PhotonNetwork.LocalPlayer.SetCustomProperties(removeProps);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to remove Gorilla Track property on disable: {e}");
        }
    }
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SerializeWriteShared))]
public static class RigTweaks_SerializeWriteShared_Patch
{
    private static void Postfix(VRRig __instance)
    {
        try
        {
            if (!RigTweaks.IsEnabled)
                return;

            if (!__instance.isLocal)
                return;

            int packed = BitPackUtils.PackQuaternionForNetwork(__instance.transform.rotation);

            RaiseEventOptions opts = new()
            {
                    TargetActors = PhotonNetwork.PlayerList
                                                .Where(p => p.CustomProperties != null                            &&
                                                            p.CustomProperties.ContainsKey("Gorilla Track 2.3.1") &&
                                                            !p.IsLocal)
                                                .Select(p => p.ActorNumber)
                                                .ToArray(),
            };

            PhotonNetwork.RaiseEvent(189, packed, opts, SendOptions.SendReliable);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SerializeWriteShared postfix error: {e}");
        }
    }
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SerializeReadShared))]
public static class RigTweaks_SerializeReadShared_Prefix
{
    private static bool Prefix(VRRig __instance, InputStruct data)
    {
        try
        {
            if (!RigTweaks.IsEnabled)
                return true;

            Player player = __instance.creator?.GetPlayerRef();

            if (player == null)
                return true;

            if (!player.CustomProperties.ContainsKey("Gorilla Track 2.3.1"))
                return true;

            Quaternion headRot = BitPackUtils.UnpackQuaternionFromNetwork(data.headRotation);
            __instance.head.syncRotation = headRot;

            BitPackUtils.UnpackHandPosRotFromNetwork(data.rightHandLong, out __instance.tempVec,
                    out __instance.tempQuat);

            __instance.rightHand.syncPos      = __instance.tempVec;
            __instance.rightHand.syncRotation = __instance.tempQuat;

            BitPackUtils.UnpackHandPosRotFromNetwork(data.leftHandLong, out __instance.tempVec,
                    out __instance.tempQuat);

            __instance.leftHand.syncPos      = __instance.tempVec;
            __instance.leftHand.syncRotation = __instance.tempQuat;

            __instance.syncPos  = BitPackUtils.UnpackWorldPosFromNetwork(data.position);
            __instance.handSync = data.handPosition;

            __instance.taggedById = data.taggedById;

            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SerializeReadShared prefix error: {e}");

            return true;
        }
    }
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class RigTweaks_LateUpdatePatch
{
    private static void Postfix(VRRig __instance)
    {
        try
        {
            if (!RigTweaks.IsEnabled)
                return;

            if (__instance.isLocal)
            {
                __instance.transform.rotation =
                        GorillaTagger.Instance.headCollider.transform.rotation * RigTweaks.RigRotation;

                __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);

                __instance.head.rigTarget.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LateUpdate postfix error: {e}");
        }
    }
}

[HarmonyPatch(typeof(GorillaIKMgr), nameof(GorillaIKMgr.Awake))]
public static class RigTweaks_IKBlocker_Patch
{
    private static bool Prefix(GorillaIK __instance)
    {
        if (!RigTweaks.IsEnabled)
            return true;

        VRRig rig = __instance.GetComponent<VRRig>();

        if (rig == null || rig.isOfflineVRRig)
            return true;

        Hashtable props = rig.Creator.GetPlayerRef().CustomProperties;

        if (props.ContainsKey("Gorilla Track 2.3.1")     &&
            props["Gorilla Track 2.3.1"] is bool enabled &&
            enabled)
            return false;

        return true;
    }
}