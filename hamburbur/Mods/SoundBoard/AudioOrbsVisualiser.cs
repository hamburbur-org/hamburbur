using GorillaTag.Rendering;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod("Audio Orbs",
        "Dynamic orbs that react to sounds. Based on the VRChat world AudioOrbs",
        ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class AudioOrbsVisualizer : hamburburmod
{
    private const int   SampleSize         = 1024;
    private const int   OrbCount           = 32;
    private const float HistoryRate        = 90f;
    private const float HistoryStep        = 1f / HistoryRate;
    private const float BaseRadius         = 2.45f;
    private const float DisplacementHeight = 3.25f;
    private const float BaseOrbScale       = 0.075f;
    private const float MinimumTrailTime   = 0.1f;
    private const float MaximumTrailTime   = 0.42f;

    private static readonly Quaternion BaseRingRotation = Quaternion.Euler(18f, 0f, -8f);
    private static readonly Vector3 TumbleAxis = new Vector3(0.68f, 0.22f, 0.7f).normalized;

    private static readonly Color[] Palette =
    [
            new(0.55f, 0f,   1f),
            new(0.2f,  0.1f, 0.9f),
            new(0f,    0.5f, 1f),
            new(0f,    0.85f, 1f),
            new(0.8f,  0f,   1f),
            new(1f,    0f,   0.6f),
            new(0.6f,  0f,   0.8f),
            new(0.15f, 0f,   0.7f),
    ];

    private Vector3 anchorPosition;

    private float autoGainLevel;
    private float bassSlowEnvelope;
    private float bassTransient;
    private float beatCooldownTimer;
    private float buildupLevel;
    private float currentPeakEnvelope;
    private float historyAccumulator;
    private float impactPulse;
    private float previousBassTarget;
    private float previousBassTransient;
    private float previousPeakEnvelope;
    private float shapeContrast;
    private float tumbleAngle;
    private float volumeEnvelope;
    private float volumeFast;
    private float volumeSlow;

    private Color currentFogColor;

    private Vector4 currentBandEnvelope;
    private Vector4 previousBandEnvelope;

    private Quaternion ringRotation;

    private float[]           displacementTargets;
    private float[]           bassTransientHistory;
    private float[]           filteredDisplacementTargets;
    private float[]           fftImaginary;
    private float[]           fftReal;
    private float[]           hannWindow;
    private float[]           peakHistory;
    private float[]           samples;
    private float[]           smoothedDisplacements;
    private GameObject[]      orbs;
    private Material[]        orbMaterials;
    private ParticleSystem[]  orbParticles;
    private TrailRenderer[]   trails;
    private Vector4[]         bandHistory;

    private Material particleMaterial;
    private Material trailMaterial;

    protected override void OnEnable()
    {
        anchorPosition = GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f);

        samples             = new float[SampleSize];
        fftReal             = new float[SampleSize];
        fftImaginary        = new float[SampleSize];
        hannWindow          = new float[SampleSize];
        bandHistory         = new Vector4[OrbCount];
        peakHistory         = new float[OrbCount];
        bassTransientHistory = new float[OrbCount];
        displacementTargets = new float[OrbCount];
        filteredDisplacementTargets = new float[OrbCount];
        smoothedDisplacements = new float[OrbCount];

        for (int i = 0; i < SampleSize; i++)
            hannWindow[i] = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * i / (SampleSize - 1));

        currentFogColor         = new Color(0f, 0f, 0f, 0f);
        autoGainLevel           = 0.08f;
        bassSlowEnvelope        = 0f;
        bassTransient           = 0f;
        beatCooldownTimer       = 0f;
        buildupLevel            = 0f;
        currentBandEnvelope     = Vector4.zero;
        currentPeakEnvelope     = 0f;
        historyAccumulator      = 0f;
        impactPulse             = 0f;
        previousBassTarget      = 0f;
        previousBassTransient   = 0f;
        previousBandEnvelope    = Vector4.zero;
        previousPeakEnvelope    = 0f;
        shapeContrast           = 1f;
        tumbleAngle             = 0f;
        volumeEnvelope          = 0f;
        volumeFast              = 0f;
        volumeSlow              = 0f;
        ringRotation            = BaseRingRotation;

        orbs         = new GameObject[OrbCount];
        trails       = new TrailRenderer[OrbCount];
        orbMaterials = new Material[OrbCount];
        orbParticles = new ParticleSystem[OrbCount];

        trailMaterial = CreateMaterial(
                "Universal Render Pipeline/Particles/Unlit",
                "Legacy Shaders/Particles/Additive");
        trailMaterial.color = Color.white;

        particleMaterial = CreateMaterial(
                "Universal Render Pipeline/Particles/Unlit",
                "Legacy Shaders/Particles/Additive");
        particleMaterial.color = Color.white;

        for (int i = 0; i < OrbCount; i++)
            CreateOrb(i);

        PlaceOrbs(false, 0f);
        ApplyFog(currentFogColor);
    }

    private void CreateOrb(int i)
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = $"AudioOrb_{i}";

        if (orb.TryGetComponent(out Collider collider))
            collider.enabled = false;

        Renderer renderer = orb.GetComponent<Renderer>();
        Material orbMaterial = CreateMaterial("Universal Render Pipeline/Unlit", "Unlit/Color");
        Color orbColor = GetOrbColor(i);
        SetMaterialColor(orbMaterial, orbColor);
        if (orbMaterial.HasProperty("_EmissionColor"))
        {
            orbMaterial.EnableKeyword("_EMISSION");
            orbMaterial.SetColor("_EmissionColor", orbColor * 2.5f);
        }

        renderer.sharedMaterial = orbMaterial;
        orbMaterials[i]          = orbMaterial;

        TrailRenderer trail = orb.AddComponent<TrailRenderer>();
        trail.time                 = MinimumTrailTime;
        trail.startWidth           = 0.045f;
        trail.endWidth             = 0f;
        trail.minVertexDistance    = 0.015f;
        trail.numCapVertices       = 4;
        trail.numCornerVertices    = 4;
        trail.shadowCastingMode    = ShadowCastingMode.Off;
        trail.receiveShadows       = false;
        trail.generateLightingData = false;
        trail.emitting             = false;

        trail.sharedMaterial = trailMaterial;
        trails[i]            = trail;

        GameObject particleObject = new($"OrbParticles_{i}");
        particleObject.transform.SetParent(orb.transform, false);
        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.55f, 1.35f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.18f, 0.8f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.015f, 0.07f);
        main.startColor      = new ParticleSystem.MinMaxGradient(orbColor, Color.Lerp(orbColor, Color.white, 0.18f));
        main.maxParticles    = 120;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.09f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient particleGradient = new();
        particleGradient.SetKeys(
                new GradientColorKey[]
                {
                        new(Color.white, 0f), new(Color.white, 0.35f), new(Color.white, 1f),
                },
                new GradientAlphaKey[] { new(0.9f, 0f), new(0.55f, 0.4f), new(0f, 1f), });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(particleGradient);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.45f, 0.62f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sharedMaterial    = particleMaterial;
        particleRenderer.renderMode        = ParticleSystemRenderMode.Billboard;
        particleRenderer.shadowCastingMode = ShadowCastingMode.Off;

        orbParticles[i] = particles;
        orbs[i]         = orb;
    }

    private static Material CreateMaterial(string preferredShaderName, string fallbackShaderName)
    {
        Shader shader = Shader.Find(preferredShaderName);
        if (shader == null || shader.name == "Hidden/InternalErrorShader")
            shader = Shader.Find(fallbackShaderName);
        if (shader == null || shader.name == "Hidden/InternalErrorShader")
            shader = Shader.Find("Sprites/Default");

        return new Material(shader);
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        material.color = color;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
    }

    protected override void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.1f);
        if (dt <= 0f)
            return;

        AnalyzeAudio(dt);
        AdvanceAudioHistory(dt);
        UpdateRingRotation(dt);

        buildupLevel = Mathf.Lerp(
                buildupLevel,
                volumeEnvelope,
                1f - Mathf.Exp(-(volumeEnvelope > buildupLevel ? 1.8f : 0.55f) * dt));

        float fogColorMix = Mathf.Clamp01(
                currentBandEnvelope.z * 0.65f +
                currentBandEnvelope.w * 0.35f);
        Color fogBase = Color.Lerp(Palette[0], Palette[3], fogColorMix);
        float fogBrightness = 0.035f + volumeEnvelope * 0.105f + impactPulse * 0.05f;
        Color targetFogColor = new(
                fogBase.r * fogBrightness,
                fogBase.g * fogBrightness,
                fogBase.b * fogBrightness,
                0.55f + volumeEnvelope * 0.3f);

        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, 1f - Mathf.Exp(-7f * dt));
        ApplyFog(currentFogColor);

        PlaceOrbs(true, dt);
        impactPulse *= Mathf.Exp(-dt / 0.18f);
    }

    private void UpdateRingRotation(float dt)
    {
        const float tumbleSpeed = 18f;
        tumbleAngle = Mathf.Repeat(tumbleAngle + tumbleSpeed * dt, 360f);
        ringRotation = Quaternion.AngleAxis(tumbleAngle, TumbleAxis) * BaseRingRotation;
    }

    private void AnalyzeAudio(float dt)
    {
        VoiceManager.Get().GetMixedOutput(samples);

        float squareSum = 0f;
        float peak      = 0f;
        for (int i = 0; i < SampleSize; i++)
        {
            float sample = samples[i];
            squareSum += sample * sample;
            peak = Mathf.Max(peak, Mathf.Abs(sample));
            fftReal[i]      = sample * hannWindow[i];
            fftImaginary[i] = 0f;
        }

        float rms = Mathf.Sqrt(squareSum / SampleSize);
        PerformFft();

        int sampleRate = Mathf.Max(AudioSettings.outputSampleRate, 8000);
        Vector4 rawBands = new(
                GetBandEnergy(45f,   250f,   sampleRate),
                GetBandEnergy(250f,  700f,   sampleRate),
                GetBandEnergy(700f,  2500f,  sampleRate),
                GetBandEnergy(2500f, 12000f, sampleRate));

        float autoGainTarget = Mathf.Max(rms, 0.004f);
        float autoGainRate   = autoGainTarget > autoGainLevel ? 0.45f : 0.1f;
        autoGainLevel = Mathf.Lerp(autoGainLevel, autoGainTarget, 1f - Mathf.Exp(-autoGainRate * dt));

        float gainCorrection = Mathf.Clamp(0.075f / Mathf.Max(autoGainLevel, 0.004f), 0.5f, 4f);
        float loudnessGate   = Mathf.InverseLerp(0.0025f, 0.025f, rms);

        Vector4 bandTargets = new(
                Mathf.Clamp01(rawBands.x * gainCorrection / 0.05f) * loudnessGate,
                Mathf.Clamp01(rawBands.y * gainCorrection / 0.014f) * loudnessGate,
                Mathf.Clamp01(rawBands.z * gainCorrection / 0.0075f) * loudnessGate,
                Mathf.Clamp01(rawBands.w * gainCorrection / 0.0025f) * loudnessGate);

        float bassTarget      = bandTargets.x;
        float bassFlux        = Mathf.Max(0f, bassTarget - previousBassTarget);
        float bassAboveFloor  = Mathf.Max(0f, bassTarget - bassSlowEnvelope);
        float bassPunchTarget = Mathf.Clamp01(bassFlux * 2.5f + bassAboveFloor * 1.4f);
        bassTransient = SmoothEnvelope(bassTransient, bassPunchTarget, dt, 70f, 9f);
        bassSlowEnvelope = SmoothEnvelope(bassSlowEnvelope, bassTarget, dt, 3f, 3f);
        previousBassTarget = bassTarget;

        currentBandEnvelope.x = SmoothEnvelope(currentBandEnvelope.x, bandTargets.x, dt, 52f, 11f);
        currentBandEnvelope.y = SmoothEnvelope(currentBandEnvelope.y, bandTargets.y, dt, 48f, 10f);
        currentBandEnvelope.z = SmoothEnvelope(currentBandEnvelope.z, bandTargets.z, dt, 42f, 11f);
        currentBandEnvelope.w = SmoothEnvelope(currentBandEnvelope.w, bandTargets.w, dt, 38f, 12f);

        float peakTarget = Mathf.Clamp01(peak * gainCorrection / 0.42f) * loudnessGate;
        currentPeakEnvelope = SmoothEnvelope(currentPeakEnvelope, peakTarget, dt, 60f, 14f);

        float normalizedVolume = Mathf.Clamp01(rms * gainCorrection / 0.16f);
        float rawVolume        = Mathf.InverseLerp(0.008f, 0.3f, rms);
        float volumeTarget = Mathf.Clamp01(normalizedVolume * 0.65f + rawVolume * 0.35f) * loudnessGate;
        volumeEnvelope = SmoothEnvelope(volumeEnvelope, volumeTarget, dt, 32f, 7f);
        volumeFast     = SmoothEnvelope(volumeFast, volumeTarget, dt, 46f, 16f);
        volumeSlow     = SmoothEnvelope(volumeSlow, volumeTarget, dt, 2.8f, 2.8f);

        beatCooldownTimer -= dt;
        float volumeOnset = Mathf.Max(0f, volumeFast - volumeSlow * 1.04f);
        float bassOnset   = bassTransient * 0.28f;
        float onset       = Mathf.Max(volumeOnset, bassOnset);
        if (beatCooldownTimer <= 0f && volumeFast > 0.16f && onset > 0.045f)
        {
            beatCooldownTimer = 0.16f;
            float strength = Mathf.Clamp01(
                    onset * 4.2f +
                    currentPeakEnvelope * 0.18f +
                    currentBandEnvelope.x * 0.18f);
            impactPulse = Mathf.Max(impactPulse, strength);
        }
    }

    private void PerformFft()
    {
        int j = 0;
        for (int i = 1; i < SampleSize; i++)
        {
            int bit = SampleSize >> 1;
            while ((j & bit) != 0)
            {
                j ^= bit;
                bit >>= 1;
            }

            j ^= bit;
            if (i >= j)
                continue;

            (fftReal[i], fftReal[j])           = (fftReal[j], fftReal[i]);
            (fftImaginary[i], fftImaginary[j]) = (fftImaginary[j], fftImaginary[i]);
        }

        for (int length = 2; length <= SampleSize; length <<= 1)
        {
            float angleStep = -2f * Mathf.PI / length;
            int halfLength  = length >> 1;

            for (int start = 0; start < SampleSize; start += length)
            {
                for (int offset = 0; offset < halfLength; offset++)
                {
                    float angle = angleStep * offset;
                    float wr    = Mathf.Cos(angle);
                    float wi    = Mathf.Sin(angle);
                    int even    = start + offset;
                    int odd     = even + halfLength;

                    float oddReal = fftReal[odd] * wr - fftImaginary[odd] * wi;
                    float oddImag = fftReal[odd] * wi + fftImaginary[odd] * wr;
                    float evenReal = fftReal[even];
                    float evenImag = fftImaginary[even];

                    fftReal[even]      = evenReal + oddReal;
                    fftImaginary[even] = evenImag + oddImag;
                    fftReal[odd]       = evenReal - oddReal;
                    fftImaginary[odd]  = evenImag - oddImag;
                }
            }
        }
    }

    private float GetBandEnergy(float lowFrequency, float highFrequency, int sampleRate)
    {
        int firstBin = Mathf.Clamp(
                Mathf.CeilToInt(lowFrequency * SampleSize / sampleRate),
                1,
                SampleSize / 2);
        int lastBin = Mathf.Clamp(
                Mathf.FloorToInt(highFrequency * SampleSize / sampleRate),
                firstBin,
                SampleSize / 2);

        float power = 0f;
        int count   = 0;
        const float magnitudeScale = 2f / SampleSize;
        for (int i = firstBin; i <= lastBin; i++)
        {
            float real      = fftReal[i];
            float imaginary = fftImaginary[i];
            float magnitude = Mathf.Sqrt(real * real + imaginary * imaginary) * magnitudeScale;
            power += magnitude * magnitude;
            count++;
        }

        return count == 0 ? 0f : Mathf.Sqrt(power / count);
    }

    private static float SmoothEnvelope(
            float current,
            float target,
            float dt,
            float attackSpeed,
            float releaseSpeed)
    {
        float speed = target > current ? attackSpeed : releaseSpeed;
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * dt));
    }

    private void AdvanceAudioHistory(float dt)
    {
        historyAccumulator += dt;
        int safety = 0;
        while (historyAccumulator >= HistoryStep && safety++ < 16)
        {
            historyAccumulator -= HistoryStep;
            float alpha = Mathf.Clamp01(1f - historyAccumulator / Mathf.Max(dt, 0.0001f));

            Vector4 bands = Vector4.Lerp(previousBandEnvelope, currentBandEnvelope, alpha);
            float peak    = Mathf.Lerp(previousPeakEnvelope, currentPeakEnvelope, alpha);
            float bassPunch = Mathf.Lerp(previousBassTransient, bassTransient, alpha);

            for (int i = OrbCount - 1; i > 0; i--)
            {
                bandHistory[i]          = bandHistory[i - 1];
                peakHistory[i]          = peakHistory[i - 1];
                bassTransientHistory[i] = bassTransientHistory[i - 1];
            }

            bandHistory[0]          = bands;
            peakHistory[0]          = peak;
            bassTransientHistory[0] = bassPunch;
        }

        previousBandEnvelope = currentBandEnvelope;
        previousPeakEnvelope = currentPeakEnvelope;
        previousBassTransient = bassTransient;
    }

    private void PlaceOrbs(bool emitTrails, float dt)
    {
        float meanTransient = 0f;

        for (int i = 0; i < OrbCount; i++)
        {
            Vector4 bands = bandHistory[i];
            float weightedDrive =
                    bands.x * 0.52f +
                    bands.y * 0.27f +
                    bands.z * 0.15f +
                    bands.w * 0.06f;
            float drive = Mathf.Max(weightedDrive, bands.x * 0.9f);
            drive = Mathf.Clamp01(drive * 0.84f + peakHistory[i] * 0.16f);
            displacementTargets[i] = Mathf.Pow(drive, 0.78f);
            meanTransient += bassTransientHistory[i];
        }

        float meanDrive    = 0f;
        float minimumDrive = float.MaxValue;
        float maximumDrive = float.MinValue;
        for (int i = 0; i < OrbCount; i++)
        {
            int previous = Mathf.Max(i - 1, 0);
            int next     = Mathf.Min(i + 1, OrbCount - 1);
            float filteredDrive =
                    displacementTargets[previous] * 0.2f +
                    displacementTargets[i]        * 0.6f +
                    displacementTargets[next]     * 0.2f;
            filteredDisplacementTargets[i] = filteredDrive;
            meanDrive += filteredDrive;
            minimumDrive = Mathf.Min(minimumDrive, filteredDrive);
            maximumDrive = Mathf.Max(maximumDrive, filteredDrive);
        }

        meanDrive /= OrbCount;
        meanTransient /= OrbCount;

        float historyRange = maximumDrive - minimumDrive;
        float activity = Mathf.Clamp01(Mathf.Max(
                volumeEnvelope,
                currentBandEnvelope.x * 0.85f));
        float desiredRange = Mathf.Lerp(0.06f, 0.32f, Mathf.Pow(activity, 0.8f));
        float rawContrast = Mathf.Clamp(
                desiredRange / Mathf.Max(historyRange, 0.05f),
                1f,
                2.8f);
        float contrastGate =
                Mathf.InverseLerp(0.035f, 0.1f, historyRange) *
                Mathf.InverseLerp(0.05f, 0.3f, activity);
        float targetContrast = Mathf.Lerp(1f, rawContrast, contrastGate);
        if (emitTrails)
            shapeContrast = SmoothEnvelope(shapeContrast, targetContrast, dt, 4f, 2.5f);
        else
            shapeContrast = targetContrast;

        float bassShape    = Mathf.Pow(Mathf.Clamp01(currentBandEnvelope.x), 0.75f);
        float lowMidShape  = currentBandEnvelope.y;
        float highMidShape = currentBandEnvelope.z;

        for (int i = 0; i < OrbCount; i++)
        {
            float angle = i / (float)OrbCount * Mathf.PI * 2f;
            float centeredHistory = (filteredDisplacementTargets[i] - meanDrive) * shapeContrast;
            float transientShape = (bassTransientHistory[i] - meanTransient) * 0.2f;
            float harmonicShape =
                    Mathf.Sin(angle * 2f)        * 0.09f  * bassShape +
                    Mathf.Sin(angle * 3f + 0.8f) * 0.045f * lowMidShape +
                    Mathf.Cos(angle * 4f)        * 0.02f  * highMidShape;
            float targetDisplacement = Mathf.Clamp(
                    (centeredHistory + transientShape + harmonicShape) *
                    (1f + impactPulse * 0.1f),
                    -0.58f,
                    0.68f);

            if (emitTrails)
            {
                float currentDisplacement = smoothedDisplacements[i];
                float responseSpeed = targetDisplacement * currentDisplacement < 0f
                                              ? 12f
                                              : Mathf.Abs(targetDisplacement) > Mathf.Abs(currentDisplacement)
                                                      ? 14f
                                                      : 7f;
                smoothedDisplacements[i] = Mathf.Lerp(
                        currentDisplacement,
                        targetDisplacement,
                        1f - Mathf.Exp(-responseSpeed * dt));
            }
            else
            {
                smoothedDisplacements[i] = targetDisplacement;
            }

            float displacement = smoothedDisplacements[i];
            float radiusScale = 1f + bassShape * 0.035f + impactPulse * 0.025f;

            Vector3 localPosition = new(
                    Mathf.Cos(angle) * BaseRadius * radiusScale,
                    displacement * DisplacementHeight,
                    Mathf.Sin(angle) * BaseRadius * radiusScale);

            orbs[i].transform.position = anchorPosition + ringRotation * localPosition;

            float peak       = peakHistory[i];
            float localMotion = Mathf.Abs(displacement);
            float scaleDrive = Mathf.Clamp01(
                    meanDrive * 0.38f +
                    localMotion * 0.8f +
                    peak * 0.35f +
                    bassTransientHistory[i] * 0.4f);
            float scale = BaseOrbScale *
                          (1f + scaleDrive * 1.75f + impactPulse * 0.28f);
            orbs[i].transform.localScale = Vector3.one * Mathf.Clamp(scale, BaseOrbScale * 0.72f, 0.23f);

            Color baseColor = GetOrbColor(i);
            float globalWhite = Mathf.Pow(
                    Mathf.InverseLerp(0.62f, 0.95f, impactPulse),
                    0.7f);
            float localWhite = Mathf.Pow(
                    Mathf.InverseLerp(0.56f, 0.94f, bassTransientHistory[i]),
                    0.75f);
            float peakWhite = Mathf.InverseLerp(0.9f, 1f, peak) * 0.24f;
            float whiteMix = Mathf.Min(
                    0.96f,
                    Mathf.Clamp01(Mathf.Max(globalWhite * 0.9f, localWhite * 0.92f) + peakWhite));
            Color displayColor = Color.Lerp(baseColor, Color.white, whiteMix);
            displayColor.a = 1f;

            Color surfaceColor = displayColor * (1f + whiteMix * 0.65f);
            surfaceColor.a = 1f;
            SetMaterialColor(orbMaterials[i], surfaceColor);
            if (orbMaterials[i].HasProperty("_EmissionColor"))
                orbMaterials[i].SetColor(
                        "_EmissionColor",
                        displayColor * (2.5f + scaleDrive * 2.3f + whiteMix * 1.8f));

            Color trailEndColor = baseColor;
            trailEndColor.a = 0f;
            float trailDrive = Mathf.Clamp01(Mathf.Max(
                    scaleDrive,
                    localMotion * 1.15f + bassTransientHistory[i] * 0.35f));
            trails[i].startColor = surfaceColor;
            trails[i].endColor   = trailEndColor;
            trails[i].time       = Mathf.Lerp(
                    MinimumTrailTime,
                    MaximumTrailTime,
                    Mathf.Pow(trailDrive, 0.75f));
            trails[i].startWidth = Mathf.Lerp(0.018f, 0.075f, trailDrive);

            if (!emitTrails)
            {
                trails[i].Clear();
                trails[i].emitting = false;
            }
            else
            {
                trails[i].emitting = true;
            }

            float particleDrive = Mathf.Pow(Mathf.Clamp01((trailDrive - 0.14f) / 0.86f), 1.35f) *
                                  Mathf.Lerp(0.45f, 1f, buildupLevel);
            ParticleSystem.EmissionModule emission = orbParticles[i].emission;
            emission.rateOverTime = Mathf.Lerp(0f, 58f, particleDrive);

            ParticleSystem.MainModule particleMain = orbParticles[i].main;
            particleMain.startColor = new ParticleSystem.MinMaxGradient(
                    Color.Lerp(baseColor, Color.white, whiteMix * 0.7f),
                    Color.Lerp(baseColor, Color.white, 0.18f + whiteMix * 0.72f));
            particleMain.startSize = new ParticleSystem.MinMaxCurve(
                    0.01f,
                    0.025f + particleDrive * 0.055f);

            if (particleDrive > 0.015f && !orbParticles[i].isPlaying)
                orbParticles[i].Play();
        }
    }

    private static Color GetOrbColor(int i)
    {
        float scaled = i / (float)OrbCount * Palette.Length;
        int first    = Mathf.FloorToInt(scaled) % Palette.Length;
        int second   = (first + 1)              % Palette.Length;

        return Color.Lerp(Palette[first], Palette[second], scaled % 1f);
    }

    private static void ApplyFog(Color color) =>
            ZoneShaderSettings.activeInstance?.SetGroundFogValue(color, 0f, float.MaxValue, 0f);

    protected override void OnDisable()
    {
        ApplyFog(new Color(0f, 0f, 0f, 0f));

        if (orbs != null)
        {
            foreach (GameObject orb in orbs)
                if (orb != null)
                    Object.Destroy(orb);
        }

        if (orbMaterials != null)
        {
            foreach (Material material in orbMaterials)
                if (material != null)
                    Object.Destroy(material);
        }

        if (trailMaterial != null)
            Object.Destroy(trailMaterial);
        if (particleMaterial != null)
            Object.Destroy(particleMaterial);

        orbs               = null;
        trails             = null;
        orbMaterials       = null;
        orbParticles       = null;
        trailMaterial      = null;
        particleMaterial   = null;
        samples            = null;
        fftReal            = null;
        fftImaginary       = null;
        hannWindow         = null;
        bandHistory        = null;
        peakHistory        = null;
        bassTransientHistory = null;
        displacementTargets = null;
        filteredDisplacementTargets = null;
        smoothedDisplacements = null;
    }
}
