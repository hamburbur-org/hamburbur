using System;
using System.Collections;
using GorillaNetworking;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Lobby Hopper", "Good for player tracking with the telemetry", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.AlwaysDisabled, 0)]
public class LobbyHopper : hamburburmod
{
    private int       ensureNotDoOnGameStart;
    private Coroutine hopRoutine;

    private float punDelay = 1f;
    private float timeNotInRoom;
    private float timeNotInRoomForCache;

    protected override void Update()
    {
        if (NetworkSystem.Instance.InRoom)
        {
            timeNotInRoom         = 0f;
            timeNotInRoomForCache = 0f;

            return;
        }

        timeNotInRoom         += Time.deltaTime;
        timeNotInRoomForCache += Time.deltaTime;

        if (timeNotInRoomForCache > 60)
        {
            NotificationManager.SendNotification(
                    "<color=red>Error</color>",
                    "You seem to not be able to join rooms, your 'rig cache' may have failed",
                    5f,
                    true,
                    true);

            timeNotInRoomForCache = 0f;
        }

        if (timeNotInRoom < 10f)
            return;

        timeNotInRoom = 0f;
        JoinRandom();
    }

    protected override void OnEnable()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent += OnJoinedRoom;
        PUNErrors.OnPunError                     += HandlePunError;

        if (ensureNotDoOnGameStart != 0)
            JoinRandom();

        ensureNotDoOnGameStart = -1;
    }

    protected override void OnDisable()
    {
        NetworkSystem.Instance.OnJoinedRoomEvent -= OnJoinedRoom;
        PUNErrors.OnPunError                     -= HandlePunError;

        if (hopRoutine != null)
            CoroutineManager.Instance.StopCoroutine(hopRoutine);
    }

    private void OnJoinedRoom()
    {
        if (hopRoutine != null)
            CoroutineManager.Instance.StopCoroutine(hopRoutine);

        hopRoutine = CoroutineManager.Instance.StartCoroutine(RoomFlow());
    }

    private IEnumerator RoomFlow()
    {
        bool rigLoaded = false;

        RigUtils.OnRigCosmeticsLoaded += OnLoaded;

        while (!rigLoaded)
            yield return null;

        RigUtils.OnRigCosmeticsLoaded -= OnLoaded;

        yield return Countdown(4f);

        NetworkSystem.Instance.ReturnToSinglePlayer();

        yield return new WaitForSeconds(3f + punDelay);

        JoinRandom();

        yield break;

        void OnLoaded(VRRig rig) => rigLoaded = true;
    }

    private IEnumerator Countdown(float seconds)
    {
        float remaining = seconds;

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;

            yield return null;
        }

        NotificationManager.SendNotification(
                "<color=yellow>Room</color>",
                "Hopping now!",
                3f,
                false,
                true);
    }

    private void HandlePunError()
    {
        punDelay += 1f;

        if (hopRoutine != null)
            CoroutineManager.Instance.StopCoroutine(hopRoutine);

        hopRoutine = CoroutineManager.Instance.StartCoroutine(RateLimitDelay(punDelay));
    }

    private IEnumerator RateLimitDelay(float delay)
    {
        NotificationManager.SendNotification(
                "<color=yellow>Room</color>",
                "You're currently rate limited",
                delay,
                false,
                true);

        float remaining = delay;

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;

            yield return null;
        }
    }

    private async void JoinRandom()
    {
        Debug.Log("Trying to join random room");

        try
        {
            if (NetworkSystem.Instance.InRoom)
                await NetworkSystem.Instance.ReturnToSinglePlayer();

            else
                Debug.Log("Not connected to a room.");

            string networkZone = PhotonNetworkController.Instance.currentJoinTrigger == null ||
                                 PhotonNetworkController.Instance.currentJoinTrigger.name.ToLower()
                                                        .Contains("private")
                                         ? "forest"
                                         : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;

            PhotonNetworkController.Instance.AttemptToJoinPublicRoom(
                    GorillaComputer.instance.GetJoinTriggerForZone(networkZone));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}