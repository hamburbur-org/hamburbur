using System;
using System.Collections.Generic;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod("Glider Visualizer",
        "Turns the Gliders in SkyJungle into a equaliser for the soundboard. !! CURRENTLY DISABLED !!",
        ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GliderVisualizer : hamburburmod
{
    private const  int   SampleSize       = 1024;
    private const float HeightMultiplier = 10f;
    private const  float Radius           = 6f;
    private const  float CircleHeight     = -0.1f;
    private const  float OrbitSpeed       = 0.3f;

    private const float InfluenceLowerBound = 0f;
    private const float InfluenceUpperBound = 20f;

    public static bool IsEnabled;

    private GliderHoldable[] gliders;
    private int[]            shuffledIndices;
    private float[]          smoothedHeights;
    private float[]          spectrum;

    protected override void Start()
    {
        spectrum = new float[SampleSize];

        GliderHoldable[] all       = GetGliders();
        int              halfCount = all.Length;
        gliders = new GliderHoldable[halfCount];
        Array.Copy(all, gliders, halfCount);
        
        smoothedHeights = new float[gliders.Length];

        shuffledIndices = new int[gliders.Length];
        for (int i = 0; i < shuffledIndices.Length; i++)
            shuffledIndices[i] = i;

        Random rng = new();
        for (int i = shuffledIndices.Length - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            (shuffledIndices[i], shuffledIndices[swapIndex]) = (shuffledIndices[swapIndex], shuffledIndices[i]);
        }
    }

    protected override void Update()
    {
        if (gliders.Length == 0) 
            return;
        
        VoiceManager.Get().GetMixedOutput(spectrum);

        Vector3 center         = GorillaTagger.Instance.headCollider.transform.position;
        int     bandsPerGlider = Mathf.Max(1, spectrum.Length / gliders.Length);

        float timeOffset = -Time.time * OrbitSpeed;

        for (int i = 0; i < gliders.Length; i++)
        {
            GliderHoldable glider = gliders[i];
            if (!Equals(glider.GetView.Owner, PhotonNetwork.LocalPlayer))
            {
                glider.OnHover(null, null);

                continue;
            }

            int spectrumIndex = shuffledIndices[i];

            float   angle   = i / (float)gliders.Length * Mathf.PI * 2f + timeOffset;
            Vector3 outward = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            Vector3 pos       = center   + outward * Radius;
            float   baselineY = center.y + CircleHeight;
            pos.y = baselineY;

            float sum = 0f;
            for (int j = 0; j < bandsPerGlider; j++)
                sum += spectrum[spectrumIndex * bandsPerGlider + j];

            float avg = sum / bandsPerGlider;

            float       targetHeight = avg * HeightMultiplier;
            const float SmoothSpeed  = 8f;
            smoothedHeights[i] = Mathf.Lerp(
                    smoothedHeights[i],
                    targetHeight,
                    1f - Mathf.Exp(-SmoothSpeed * Time.deltaTime)
            );
            float influence = Mathf.Clamp(smoothedHeights[i], InfluenceLowerBound, InfluenceUpperBound);

            pos.y += influence;

            glider.transform.position = pos;
            glider.transform.rotation = Quaternion.LookRotation(outward, Vector3.up);
        }
    }

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;

    private GliderHoldable[] GetGliders() => Object.FindObjectsOfType<GliderHoldable>();
}

[HarmonyPatch(typeof(GliderHoldable), "Respawn")]
public static class GliderPatch
{
    private static bool Prefix() => !GliderVisualizer.IsEnabled;
}

[HarmonyPatch(typeof(RequestableOwnershipGuard), "OwnershipRequested")]
public static class OwnershipPatch
{
    private static readonly List<RequestableOwnershipGuard> blacklistedGuards = [];

    private static bool Prefix(RequestableOwnershipGuard __instance, string nonce, PhotonMessageInfo info) =>
            !GliderVisualizer.IsEnabled || __instance.photonView.IsMine && !blacklistedGuards.Contains(__instance);
}