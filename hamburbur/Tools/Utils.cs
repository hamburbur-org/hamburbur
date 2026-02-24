using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using hamburbur.Managers;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace hamburbur.Tools;

public class Utils : MonoBehaviour
{
    public static Transform RealRightController;
    public static Transform RealLeftController;
    public static Action    OnFixedUpdate;
    public static Action    OnUpdate;
    public static Action    OnLateUpdate;
    public static Action    OnOnGUI;
    public static bool      HasRemovedThisFrame;

    public static readonly int TransparentFX    = LayerMask.NameToLayer("TransparentFX");
    public static readonly int IgnoreRaycast    = LayerMask.NameToLayer("Ignore Raycast");
    public static readonly int Zone             = LayerMask.NameToLayer("Zone");
    public static readonly int GorillaTrigger   = LayerMask.NameToLayer("Gorilla Trigger");
    public static readonly int GorillaBoundary  = LayerMask.NameToLayer("Gorilla Boundary");
    public static readonly int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");
    public static readonly int GorillaParticle  = LayerMask.NameToLayer("GorillaParticle");

    public static readonly Dictionary<char, Vector2Int[]> BITFont = new()
    {
            ['A'] = [V(1, 4), V(0, 3), V(2, 3), V(0, 2), V(1, 2), V(2, 2), V(0, 1), V(2, 1), V(0, 0), V(2, 0),],
            ['B'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 4), V(2, 3), V(1, 2), V(2, 1), V(1, 0),],
            ['C'] = [V(1, 4), V(2, 4), V(0, 3), V(0, 2), V(0, 1), V(1, 0), V(2, 0),],
            ['D'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 4), V(2, 3), V(2, 2), V(2, 1), V(1, 0),],
            ['E'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 4), V(2, 4), V(1, 2), V(2, 0), V(1, 0),],
            ['F'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 4), V(2, 4), V(1, 2),],
            ['G'] = [V(1, 4), V(2, 4), V(0, 3), V(0, 2), V(2, 2), V(2, 1), V(1, 0), V(2, 0),],
            ['H'] =
            [
                    V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(2, 0), V(2, 1), V(2, 2), V(2, 3), V(2, 4), V(1, 2),
            ],
            ['I'] = [V(0, 4), V(1, 4), V(2, 4), V(1, 3), V(1, 2), V(1, 1), V(0, 0), V(1, 0), V(2, 0),],
            ['J'] = [V(2, 4), V(2, 3), V(2, 2), V(2, 1), V(1, 0), V(0, 1),],
            ['K'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 2), V(2, 3), V(2, 1),],
            ['L'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 0), V(2, 0),],
            ['M'] =
            [
                    V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 3), V(2, 4), V(2, 3), V(2, 2), V(2, 1), V(2, 0),
            ],
            ['N'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 3), V(2, 2), V(2, 1), V(2, 0),],
            ['O'] = [V(1, 4), V(2, 4), V(0, 3), V(2, 3), V(0, 2), V(2, 2), V(0, 1), V(2, 1), V(1, 0), V(2, 0),],
            ['P'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 4), V(2, 3), V(1, 2),],
            ['Q'] =
            [
                    V(1, 4), V(2, 4), V(0, 3), V(2, 3), V(0, 2), V(2, 2), V(0, 1), V(2, 1), V(1, 0), V(2, 0),
                    V(2, -1),
            ],
            ['R'] = [V(0, 0), V(0, 1), V(0, 2), V(0, 3), V(0, 4), V(1, 4), V(2, 3), V(1, 2), V(2, 1),],
            ['S'] = [V(1, 4), V(2, 4), V(0, 3), V(1, 2), V(2, 1), V(0, 0), V(1, 0),],
            ['T'] = [V(0, 4), V(1, 4), V(2, 4), V(1, 3), V(1, 2), V(1, 1), V(1, 0),],
            ['U'] = [V(0, 4), V(0, 3), V(0, 2), V(0, 1), V(1, 0), V(2, 1), V(2, 2), V(2, 3), V(2, 4),],
            ['V'] = [V(0, 4), V(0, 3), V(0, 2), V(1, 1), V(2, 2), V(2, 3), V(2, 4),],
            ['W'] =
            [
                    V(0, 4), V(0, 3), V(0, 2), V(0, 1), V(0, 0), V(1, 1), V(2, 0), V(2, 1), V(2, 2), V(2, 3), V(2, 4),
            ],
            ['X'] = [V(0, 4), V(1, 3), V(2, 2), V(1, 1), V(0, 0), V(2, 4), V(1, 1),],
            ['Y'] = [V(0, 4), V(1, 3), V(2, 4), V(1, 2), V(1, 1), V(1, 0),],
            ['Z'] = [V(0, 4), V(1, 4), V(2, 4), V(1, 3), V(1, 2), V(1, 1), V(0, 0), V(1, 0), V(2, 0),],
    };

    public static bool IsMasterClient => PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;

    public static bool IsModdedRoom =>
            NetworkSystem.Instance.InRoom && NetworkSystem.Instance.GameModeString.Contains("MODDED");

    public static bool InVR     => XRSettings.isDeviceActive;
    private       void Update() => OnUpdate?.Invoke();

    private void FixedUpdate() => OnFixedUpdate?.Invoke();
    private void LateUpdate()  => OnLateUpdate?.Invoke();
    private void OnGUI()       => OnOnGUI?.Invoke();

    private static Vector2Int V(int x, int y) => new(x, y);

    public static void RPCProtection()
    {
        try
        {
            if (HasRemovedThisFrame)
                return;

            HasRemovedThisFrame = true;

            MonkeAgent.instance.rpcErrorMax  = int.MaxValue;
            MonkeAgent.instance.rpcCallLimit = int.MaxValue;
            MonkeAgent.instance.logErrorMax  = int.MaxValue;

            PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
            PhotonNetwork.QuickResends               = int.MaxValue;

            PhotonNetwork.SendAllOutgoingCommands();

            CoroutineManager.Instance.StartCoroutine(ResetHasRemovedFlag());
        }
        catch (Exception ex)
        {
            Debug.Log($"RPC protection failed: {ex.Message}");
        }
    }

    public static Texture2D LoadEmbeddedImage(string name)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("hamburbur.Resources." + name);

        if (stream == null) return null;
        byte[] imageData = new byte[stream.Length];
        stream.Read(imageData, 0, imageData.Length);
        Texture2D texture = new(2, 2);
        texture.LoadImage(imageData);

        return texture;
    }

    //Yes I skidded who cares???
    public static void SendSerialize(PhotonView pv, RaiseEventOptions options = null, int timeOffset = 0)
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (pv == null)
        {
            Debug.LogError("PhotonView is null. Cannot serialize.");

            return;
        }

        List<object> serializedData = PhotonNetwork.OnSerializeWrite(pv);

        PhotonNetwork.RaiseEventBatch raiseEventBatch = new();

        bool mixedReliable = pv.mixedModeIsReliable;
        raiseEventBatch.Reliable = pv.Synchronization == ViewSynchronization.ReliableDeltaCompressed || mixedReliable;
        raiseEventBatch.Group    = pv.Group;

        IDictionary dictionary = PhotonNetwork.serializeViewBatches;

        PhotonNetwork.SerializeViewBatch serializeViewBatch = new(raiseEventBatch, 2);

        if (!dictionary.Contains(raiseEventBatch))
            dictionary[raiseEventBatch] = serializeViewBatch;

        serializeViewBatch.Add(serializedData);

        RaiseEventOptions sendOptions = PhotonNetwork.serializeRaiseEvOptions;
        RaiseEventOptions finalOptions = options != null
                                                 ? new RaiseEventOptions
                                                 {
                                                         CachingOption = sendOptions.CachingOption,
                                                         Flags         = sendOptions.Flags,
                                                         InterestGroup = sendOptions.InterestGroup,
                                                         TargetActors  = options.TargetActors,
                                                         Receivers     = options.Receivers,
                                                 }
                                                 : sendOptions;

        bool         reliable           = serializeViewBatch.Batch.Reliable;
        List<object> objectUpdate       = serializeViewBatch.ObjectUpdates;
        byte         currentLevelPrefix = PhotonNetwork.currentLevelPrefix;

        objectUpdate[0] = PhotonNetwork.ServerTimestamp + timeOffset;
        objectUpdate[1] = currentLevelPrefix != 0 ? currentLevelPrefix : null;

        PhotonNetwork.NetworkingClient.OpRaiseEvent((byte)(reliable ? 206 : 201), objectUpdate, finalOptions,
                reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable);

        serializeViewBatch.Clear();
    }

    private static IEnumerator ResetHasRemovedFlag()
    {
        yield return new WaitForEndOfFrame();
        HasRemovedThisFrame = false;
    }

    public static Camera GetActiveCamera() => Plugin.Instance.ThirdPersonCamera.gameObject.activeInHierarchy
                                                      ? Plugin.Instance.ThirdPersonCamera
                                                      : Plugin.Instance.FirstPersonCamera; // foo

    public static void TeleportPlayer(Vector3 destinationPosition)
    {
        GTPlayer.Instance.TeleportTo(FormatTeleportPosition(destinationPosition), GTPlayer.Instance.transform.rotation);
        VRRig.LocalRig.transform.position = destinationPosition;
    }

    public static Vector3 FormatTeleportPosition(Vector3 teleportPosition) =>
            teleportPosition - GorillaTagger.Instance.bodyCollider.transform.position +
            GorillaTagger.Instance.transform.position;

    public static int NoInvisLayerMask() =>
            ~(1 << TransparentFX    | 1 << IgnoreRaycast | 1 << Zone | 1 << GorillaTrigger | 1 << GorillaBoundary |
              1 << GorillaCosmetics | 1 << GorillaParticle);

    public static Vector3 RandomVector3(float range = 1f) =>
            new(Random.Range(-range,     range),
                    Random.Range(-range, range),
                    Random.Range(-range, range));

    public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(Random.Range(0f, range),
                    Random.Range(0f,          range),
                    Random.Range(0f,          range));

    public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)Random.Range(0, range),
                    (byte)Random.Range(0,     range),
                    (byte)Random.Range(0,     range),
                    alpha);
}