using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using GorillaTagScripts;
using hamburbur.Managers;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Libs;

public static class BuilderBlockLib
{
    private const float MinimumSpawnInterval = 0.055f;

    private static float nextSpawnTime;
    private static float nextRecycleTime;
    private static float nextUnavailableNotification;
    private static int   nextCommandId = 1;

    public static int  SelectedPieceType  { get; private set; } = -1;
    public static bool UseRandomPieceType { get; set; }

    public static bool TryGetTable(out BuilderTable table)
    {
        table = null;

        VRRig localRig = VRRig.LocalRig;

        if (localRig?.zoneEntity == null)
            return false;

        return BuilderTable.TryGetBuilderTableForZone(localRig.zoneEntity.currentZone, out table) &&
               table != null                                                                      && table.isTableMutable && table.GetTableState() == BuilderTable.TableState.Ready;
    }

    public static bool TrySelectPiece(BuilderPiece piece)
    {
        if (piece == null || piece.pieceType < 0)
            return false;

        SelectedPieceType = piece.pieceType;

        NotificationManager.SendNotification(
                "<color=#58d3ff>Block Mods</color>",
                $"Selected {GetPieceName(piece.pieceType)}",
                3f,
                true,
                false);

        return true;
    }

    public static bool TrySpawn(
            Vector3    position,
            Quaternion rotation,
            Vector3    velocity        = default,
            Vector3    angularVelocity = default,
            int?       pieceType       = null)
    {
        if (Time.time < nextSpawnTime || !PhotonNetwork.InRoom || !TryGetTable(out BuilderTable table))
            return false;

        int resolvedPieceType = pieceType ?? ResolvePieceType(table);

        if (resolvedPieceType < 0)
            return false;

        velocity        = Vector3.ClampMagnitude(velocity,        BuilderTable.MAX_DROP_VELOCITY);
        angularVelocity = Vector3.ClampMagnitude(angularVelocity, BuilderTable.MAX_DROP_ANG_VELOCITY);

        if (!PhotonNetwork.IsMasterClient)
            return TryReuseNearbyPiece(table, resolvedPieceType, position, rotation, velocity, angularVelocity);

        if (!TryGetShelf(table, out BuilderPiece.State shelfState, out int shelfId))
            return false;

        int                    pieceId    = table.CreatePieceId();
        int                    commandId  = nextCommandId++;
        BuilderTableNetworking networking = table.builderNetworking;

        networking.photonView.RPC(
                "PieceCreatedByShelfRPC",
                RpcTarget.All,
                resolvedPieceType,
                pieceId,
                BitPackUtils.PackWorldPosForNetwork(position),
                BitPackUtils.PackQuaternionForNetwork(rotation),
                0,
                (byte)shelfState,
                shelfId,
                PhotonNetwork.LocalPlayer);

        networking.photonView.RPC(
                "PieceGrabbedRPC",
                RpcTarget.All,
                commandId,
                pieceId,
                true,
                BitPackUtils.PackHandPosRotForNetwork(Vector3.zero, Quaternion.identity),
                PhotonNetwork.LocalPlayer);

        networking.photonView.RPC(
                "PieceDroppedRPC",
                RpcTarget.All,
                commandId,
                pieceId,
                position,
                rotation,
                velocity,
                angularVelocity,
                PhotonNetwork.LocalPlayer);

        nextSpawnTime = Time.time + MinimumSpawnInterval;
        PhotonNetwork.SendAllOutgoingCommands();

        return true;
    }

    public static IEnumerator SpawnSequence(IEnumerable<BlockShot> shots, float interval = 0.07f)
    {
        foreach (BlockShot shot in shots)
        {
            while (Time.time < nextSpawnTime)
                yield return null;

            TrySpawn(shot.Position, shot.Rotation, shot.Velocity, shot.AngularVelocity);

            if (interval > 0f)
                yield return new WaitForSeconds(interval);
        }
    }

    public static bool TryRecycleNearbyBlock(float radius = BuilderTable.MAX_DISTANCE_FROM_HAND)
    {
        if (Time.time < nextRecycleTime || !PhotonNetwork.InRoom || !TryGetTable(out BuilderTable table))
            return false;

        Vector3 searchPosition = PhotonNetwork.IsMasterClient
                                         ? GTPlayer.Instance.bodyCollider.bounds.center
                                         : GetServerLeftHandPosition();

        BuilderPiece piece = table.pieces
                                  .Where(candidate => candidate != null && candidate.gameObject.activeInHierarchy)
                                  .Where(candidate => !candidate.isBuiltIntoTable)
                                  .Where(candidate => candidate.heldByPlayerActorNumber !=
                                                      PhotonNetwork.LocalPlayer.ActorNumber)
                                  .Where(candidate => Vector3.Distance(candidate.transform.position, searchPosition) <
                                                      radius - 0.05f)
                                  .Where(candidate => PhotonNetwork.IsMasterClient ||
                                                      candidate.CanPlayerGrabPiece(
                                                              PhotonNetwork.LocalPlayer.ActorNumber,
                                                              candidate.transform.position))
                                  .OrderBy(candidate => Vector3.Distance(candidate.transform.position, searchPosition))
                                  .FirstOrDefault();

        if (piece == null)
            return false;

        if (PhotonNetwork.IsMasterClient)
        {
            table.builderNetworking.RequestRecyclePiece(
                    piece.pieceId,
                    piece.transform.position,
                    piece.transform.rotation,
                    true,
                    -1);
        }
        else
        {
            Vector3 handPosition = GetServerLeftHandPosition();
            BuilderDropZone dropZone = table.dropZones?
                                            .Where(zone => zone != null && (int)zone.dropType >= 1)
                                            .Where(zone => Vector3.Distance(zone.transform.position,
                                                                   handPosition) < BuilderTable.MAX_DISTANCE_FROM_HAND)
                                            .OrderBy(zone => Vector3.Distance(zone.transform.position,
                                                             handPosition))
                                            .FirstOrDefault();

            Vector3 dropPosition = dropZone != null
                                           ? dropZone.transform.position
                                           : handPosition + Vector3.down *
                                             (BuilderTable.MAX_DISTANCE_FROM_HAND - 0.05f);

            table.builderNetworking.RequestGrabPiece(piece, true, Vector3.zero, Quaternion.identity);
            table.builderNetworking.RequestDropPiece(
                    piece,
                    dropPosition,
                    Random.rotation,
                    Vector3.down * BuilderTable.MAX_DROP_VELOCITY,
                    Vector3.zero);
        }

        nextRecycleTime = Time.time + (PhotonNetwork.IsMasterClient ? 0.065f : 0.12f);
        PhotonNetwork.SendAllOutgoingCommands();

        return true;
    }

    public static void NotifyUnavailable()
    {
        if (Time.time < nextUnavailableNotification)
            return;

        nextUnavailableNotification = Time.time + 3f;

        string reason = !PhotonNetwork.InRoom
                                ? "Join a room first."
                                : "Enter a ready Monke Blocks build zone with a grabbable block nearby.";

        NotificationManager.SendNotification(
                "<color=#58d3ff>Block Mods</color>",
                reason,
                4f,
                true,
                false);
    }

    public static Vector3 RandomHorizontal(float radius)
    {
        Vector2 point = Random.insideUnitCircle * radius;

        return new Vector3(point.x, 0f, point.y);
    }

    private static int ResolvePieceType(BuilderTable table)
    {
        int[] availableTypes = GetAvailablePieceTypes(table);

        if (availableTypes.Length == 0)
            return -1;

        if (UseRandomPieceType)
            return availableTypes[Random.Range(0, availableTypes.Length)];

        if (SelectedPieceType >= 0 && availableTypes.Contains(SelectedPieceType))
            return SelectedPieceType;

        SelectedPieceType = availableTypes[0];

        return SelectedPieceType;
    }

    private static int[] GetAvailablePieceTypes(BuilderTable table)
    {
        if (table?.builderPool?.piecePoolLookup == null || table.builderPool.piecePools == null)
            return [];

        return table.builderPool.piecePoolLookup
                    .Where(pair => pair.Value >= 0 && pair.Value < table.builderPool.piecePools.Count)
                    .Where(pair => table.builderPool.piecePools[pair.Value].Count > 0)
                    .Select(pair => pair.Key)
                    .ToArray();
    }

    private static bool TryGetShelf(BuilderTable table, out BuilderPiece.State state, out int shelfId)
    {
        shelfId = table.dispenserShelves?.FindIndex(shelf => shelf != null) ?? -1;
        if (shelfId >= 0)
        {
            state = BuilderPiece.State.OnShelf;

            return true;
        }

        shelfId = table.conveyors?.FindIndex(conveyor => conveyor != null) ?? -1;
        state   = BuilderPiece.State.OnConveyor;

        return shelfId >= 0;
    }

    private static bool TryReuseNearbyPiece(
            BuilderTable table,
            int          pieceType,
            Vector3      position,
            Quaternion   rotation,
            Vector3      velocity,
            Vector3      angularVelocity)
    {
        Vector3 handPosition = GetServerLeftHandPosition();

        IEnumerable<BuilderPiece> candidates = table.pieces
                                                    .Where(piece => piece != null &&
                                                                    piece.gameObject.activeInHierarchy)
                                                    .Where(piece => !piece.isBuiltIntoTable)
                                                    .Where(piece => piece.heldByPlayerActorNumber !=
                                                                    PhotonNetwork.LocalPlayer.ActorNumber)
                                                    .Where(piece => piece.CanPlayerGrabPiece(
                                                                   PhotonNetwork.LocalPlayer.ActorNumber,
                                                                   piece.transform.position))
                                                    .Where(piece => Vector3.Distance(piece.transform.position,
                                                                            handPosition) < BuilderTable.MAX_DISTANCE_FROM_HAND);

        BuilderPiece reusablePiece = candidates
                                    .Where(piece => piece.pieceType == pieceType)
                                    .OrderBy(piece => Vector3.Distance(piece.transform.position, handPosition))
                                    .FirstOrDefault() ?? candidates
                                                        .OrderBy(piece => Vector3.Distance(piece.transform.position, handPosition))
                                                        .FirstOrDefault();

        if (reusablePiece == null)
            return false;

        float maxDistance = BuilderTable.MAX_DISTANCE_FROM_HAND - 0.05f;
        if (Vector3.Distance(handPosition, position) > maxDistance)
            position = handPosition + (position - handPosition).normalized * maxDistance;

        table.builderNetworking.RequestGrabPiece(reusablePiece, true, Vector3.zero, Quaternion.identity);
        table.builderNetworking.RequestDropPiece(
                reusablePiece,
                position,
                rotation,
                velocity,
                angularVelocity);

        nextSpawnTime = Time.time + 0.1f;
        PhotonNetwork.SendAllOutgoingCommands();

        return true;
    }

    private static Vector3 GetServerLeftHandPosition()
    {
        VRRig localRig = VRRig.LocalRig;

        return localRig?.leftHand?.rigTarget != null
                       ? localRig.leftHand.rigTarget.position
                       : GorillaTagger.Instance.leftHandTransform.position;
    }

    private static string GetPieceName(int pieceType)
    {
        if (!BuilderSetManager.hasInstance)
            return $"block {pieceType}";

        BuilderPiece prefab = BuilderSetManager.instance.GetPiecePrefab(pieceType);

        return prefab == null ? $"block {pieceType}" : prefab.displayName ?? prefab.name.Replace("(Clone)", "");
    }

    public readonly struct BlockShot(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        public readonly Vector3    Position        = position;
        public readonly Quaternion Rotation        = rotation;
        public readonly Vector3    Velocity        = velocity;
        public readonly Vector3    AngularVelocity = angularVelocity;
    }
}