using System.Collections.Generic;
using GorillaTagScripts;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Blocks;

[hamburburmod(                "Monke Blocks Anti Crash", "Locally hide nearby blocks after five seconds below 10 FPS",
        ButtonType.Togglable, AccessSetting.Public,      EnabledType.Disabled, 0)]
public sealed class BlockAntiCrash : hamburburmod
{
    private const float LowFps         = 10f;
    private const float RecoveredFps   = 15f;
    private const float LowFpsDuration = 5f;
    private const float SampleInterval = 0.5f;
    private const float HideInterval   = 0.25f;
    private const float RadiusSquared  = 12f * 12f;
    private const int   BlocksPerBatch = 8;

    private readonly List<BuilderPiece>            candidates = new(256);
    private readonly Dictionary<BuilderPiece, int> hidden     = new(256);
    private          int                           frames;
    private          float                         lowSince = -1f;
    private          int                           nextCandidate;
    private          float                         nextHide;
    private          bool                          recovering;
    private          float                         sampleStart;
    private          BuilderTable                  table;

    internal static BlockAntiCrash Active { get; private set; }

    protected override void OnEnable()
    {
        Active      = this;
        sampleStart = Time.realtimeSinceStartup;
    }

    protected override void LateUpdate()
    {
        float now = Time.realtimeSinceStartup;
        frames++;

        if (now - sampleStart < SampleInterval)
            return;

        float fps = frames / (now - sampleStart);
        frames      = 0;
        sampleStart = now;

        if (VRRig.LocalRig?.zoneEntity?.currentZone != GTZone.monkeBlocks                              ||
            !BuilderTable.TryGetBuilderTableForZone(GTZone.monkeBlocks, out BuilderTable currentTable) ||
            !currentTable                                                                              || currentTable.GetTableState() != BuilderTable.TableState.Ready ||
            !currentTable.builderRenderer)
        {
            ResetRecovery();

            return;
        }

        if (table != null && table != currentTable)
            ResetRecovery();

        table = currentTable;

        if (fps < LowFps)
        {
            if (lowSince < 0f)
                lowSince = now;

            if (!recovering && now - lowSince >= LowFpsDuration)
            {
                recovering = true;
                SnapshotNearbyBlocks();
                NotificationManager.SendNotification("<color=#58d3ff>Block Mods</color>",
                        "Low FPS detected: hiding nearby blocks locally.", 4f, true, false);
            }
        }
        else
        {
            lowSince = -1f;
            if (fps >= RecoveredFps)
                recovering = false;
        }

        if (recovering && now >= nextHide)
        {
            nextHide = now + HideInterval;
            HideBatch();
        }
    }

    private void SnapshotNearbyBlocks()
    {
        candidates.Clear();
        nextCandidate = 0;

        if (table == null || GorillaTagger.Instance?.bodyCollider == null)
            return;

        Vector3 center = GorillaTagger.Instance.bodyCollider.bounds.center;
        foreach (BuilderPiece piece in table.pieces)
            AddCandidate(piece, center);

        candidates.Sort((a, b) =>
                                (a.transform.position - center).sqrMagnitude.CompareTo(
                                        (b.transform.position - center).sqrMagnitude));
    }

    private void AddCandidate(BuilderPiece piece, Vector3 center)
    {
        if (piece                                            == null || !piece.gameObject.activeInHierarchy || piece.isBuiltIntoTable ||
            piece.heldByPlayerActorNumber                    > 0     || hidden.ContainsKey(piece)           ||
            (piece.transform.position - center).sqrMagnitude > RadiusSquared)
            return;

        candidates.Add(piece);
    }

    private void HideBatch()
    {
        if (GorillaTagger.Instance?.bodyCollider == null)
            return;

        Vector3 center          = GorillaTagger.Instance.bodyCollider.bounds.center;
        int     hiddenThisBatch = 0;
        while (nextCandidate < candidates.Count && hiddenThisBatch < BlocksPerBatch)
        {
            BuilderPiece piece = candidates[nextCandidate++];

            if (piece                                            == null || !piece.gameObject.activeInHierarchy || piece.isBuiltIntoTable ||
                piece.heldByPlayerActorNumber                    > 0     || hidden.ContainsKey(piece)           ||
                (piece.transform.position - center).sqrMagnitude > RadiusSquared)
                continue;

            hidden.Add(piece, piece.pieceId);
            table.builderRenderer.RemovePiece(piece);
            piece.gameObject.SetActive(false);
            hiddenThisBatch++;
        }
    }

    private void ResetRecovery()
    {
        recovering = false;
        lowSince   = -1f;
        candidates.Clear();
        nextCandidate = 0;

        foreach (KeyValuePair<BuilderPiece, int> entry in hidden)
            if (table             != null        && table.builderRenderer != null && entry.Key != null &&
                entry.Key.pieceId == entry.Value && entry.Key.GetTable()  == table)
            {
                entry.Key.gameObject.SetActive(true);
                table.builderRenderer.AddPiece(entry.Key);
                entry.Key.SetDirectRenderersVisible(table.IsInBuilderZone());
            }

        hidden.Clear();
        table = null;
    }

    protected override void OnDisable()
    {
        Active = null;
        ResetRecovery();
    }

    internal void OnPieceAdded(BuilderTable owner, BuilderPiece piece)
    {
        if (!recovering || owner != table || GorillaTagger.Instance?.bodyCollider == null)
            return;

        AddCandidate(piece, GorillaTagger.Instance.bodyCollider.bounds.center);
    }

    internal void OnPieceRemoving(BuilderTable owner, BuilderPiece piece)
    {
        if (owner != table || piece == null)
            return;

        hidden.Remove(piece);
    }
}

[HarmonyPatch(typeof(BuilderTable), nameof(BuilderTable.AddPiece))]
internal static class BlockAntiCrashAddPiecePatch
{
    private static void Postfix(BuilderTable __instance, BuilderPiece piece) =>
            BlockAntiCrash.Active?.OnPieceAdded(__instance, piece);
}

[HarmonyPatch(typeof(BuilderTable), nameof(BuilderTable.RemovePiece))]
internal static class BlockAntiCrashRemovePiecePatch
{
    private static void Prefix(BuilderTable __instance, BuilderPiece piece) =>
            BlockAntiCrash.Active?.OnPieceRemoving(__instance, piece);
}