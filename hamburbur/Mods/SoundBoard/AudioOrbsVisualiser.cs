using System;
using GorillaTag.Rendering;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod("Audio Orbs",
        "Dynamic orbs that react to sounds. Based on the VRChat world AudioOrbs",
        ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class AudioOrbsVisualizer : hamburburmod
{
    private const int   SampleSize     = 512;
    private const int   OrbCount       = 48;
    private const int   BassBinCount   = 10;
    private const int   MidBinStart    = 10;
    private const int   MidBinEnd      = 60;
    private const int   HiBinStart     = 60;
    private const int   HiBinEnd       = 180;
    private const float BaseRadius     = 2.3f;
    private const float GlobalSpinBase = 0.1f;
    private const float TiltLerpSpeed  = 1.5f;
    private const float ShapeLerpSpeed = 1.1f;
    private const float BeatCooldown   = 0.18f;
    private const float BeatThreshold  = 1.55f;
    private const float BuildupDecay   = 0.7f;
    private const int   TornadoCount   = 280;
    private const int   TotalShapes    = 10;

    private static readonly Color[] Palette =
    [
            new(0.55f, 0.0f, 1.0f),
            new(0.2f, 0.1f, 0.9f),
            new(0.0f, 0.5f, 1.0f),
            new(0.0f, 0.85f, 1.0f),
            new(0.8f, 0.0f, 1.0f),
            new(1.0f, 0.0f, 0.6f),
            new(0.6f, 0.0f, 0.8f),
            new(0.15f, 0.0f, 0.7f),
    ];

    private Vector3 anchorPosition;
    private float   bassNorm;

    private float bassSmoothedFast;
    private float bassSmoothedSlow;
    private float beatCooldownTimer;
    private float buildupLevel;
    private float buildupRaw;

    private Color currentFogColor;

    private int              currentShape;
    private float            fogSnapTimer;
    private float            globalAngle;
    private float            globalBassImpact;
    private float            globalSpinSpeed;
    private float            hiSmoothed;
    private float            midSmoothed;
    private int              nextShape;
    private Material[]       orbMats;
    private ParticleSystem[] orbParticles;
    private float[]          orbPhaseOffsets;
    private Renderer[]       orbRends;

    private GameObject[] orbs;
    private float[]      orbVerticalPhase;
    private bool         rippleActive;
    private float        ripplePhase;
    private float        rippleSpeed;

    private float[] rippleWave;

    private Random  rng;
    private float   shapeBlend;
    private float[] smoothedAmplitudes;

    private float[] spectrum;
    private Color   targetFogColor;
    private float   targetTiltX;
    private float   targetTiltZ;

    private float      tiltX;
    private float      tiltZ;
    private float      tornadoAlpha;
    private GameObject tornadoRoot;

    private ParticleSystem  tornadoSystem;
    private TrailRenderer[] trails;

    protected override void OnEnable()
    {
        rng            = new Random();
        anchorPosition = GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f);

        spectrum           = new float[SampleSize];
        smoothedAmplitudes = new float[OrbCount];
        orbPhaseOffsets    = new float[OrbCount];
        orbVerticalPhase   = new float[OrbCount];
        rippleWave         = new float[OrbCount];

        for (int i = 0; i < OrbCount; i++)
        {
            orbPhaseOffsets[i]  = (float)(rng.NextDouble() * Math.PI * 2.0);
            orbVerticalPhase[i] = (float)(rng.NextDouble() * Math.PI * 2.0);
        }

        currentShape    = 0;
        nextShape       = 0;
        shapeBlend      = 1f;
        globalSpinSpeed = GlobalSpinBase;

        orbs         = new GameObject[OrbCount];
        trails       = new TrailRenderer[OrbCount];
        orbRends     = new Renderer[OrbCount];
        orbMats      = new Material[OrbCount];
        orbParticles = new ParticleSystem[OrbCount];

        for (int i = 0; i < OrbCount; i++)
            CreateOrb(i);

        CreateTornado();

        currentFogColor = new Color(0f, 0f, 0f, 0f);
        targetFogColor  = currentFogColor;
        ApplyFog(currentFogColor);
    }

    private void CreateOrb(int i)
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name                 = $"AudioOrb_{i}";
        orb.transform.localScale = Vector3.one * 0.055f;

        if (orb.TryGetComponent(out Collider c)) c.enabled = false;

        Renderer rend = orb.GetComponent<Renderer>();
        Material mat  = new(Shader.Find("Universal Render Pipeline/Unlit"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Unlit/Color"));

        Color orbCol = GetOrbColor(i);
        mat.color = orbCol;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", orbCol * 2.8f);
        }

        rend.material = mat;
        orbRends[i]   = rend;
        orbMats[i]    = mat;

        TrailRenderer trail = orb.AddComponent<TrailRenderer>();
        trail.time                 = 0.45f;
        trail.startWidth           = 0.035f;
        trail.endWidth             = 0f;
        trail.numCapVertices       = 3;
        trail.numCornerVertices    = 3;
        trail.shadowCastingMode    = ShadowCastingMode.Off;
        trail.receiveShadows       = false;
        trail.generateLightingData = false;

        Material tMat = new(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (tMat.shader.name == "Hidden/InternalErrorShader")
            tMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        tMat.color     = orbCol;
        trail.material = tMat;

        Gradient g = new();
        g.SetKeys(
                new GradientColorKey[]
                        { new(orbCol, 0f), new(Color.Lerp(orbCol, Color.white, 0.35f), 0.12f), new(orbCol, 1f), },
                new GradientAlphaKey[] { new(1f, 0f), new(0.5f, 0.35f), new(0f, 1f), }
        );

        trail.colorGradient = g;
        trails[i]           = trail;

        GameObject psGo = new($"OrbParticles_{i}");
        psGo.transform.SetParent(orb.transform, false);
        ParticleSystem ps = psGo.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = ps.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f,  1f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startColor      = new ParticleSystem.MinMaxGradient(orbCol, Color.Lerp(orbCol, Color.white, 0.4f));
        main.maxParticles    = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.04f, 0.04f);

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.1f;

        ParticleSystem.ColorOverLifetimeModule colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient pg = new();
        pg.SetKeys(
                new GradientColorKey[]
                        { new(Color.Lerp(orbCol, Color.white, 0.3f), 0f), new(orbCol, 0.4f), new(orbCol, 1f), },
                new GradientAlphaKey[] { new(1f, 0f), new(0.75f, 0.3f), new(0f, 1f), }
        );

        colorOverLife.color = new ParticleSystem.MinMaxGradient(pg);

        ParticleSystem.SizeOverLifetimeModule sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sc = new();
        sc.AddKey(0f,    1f);
        sc.AddKey(0.35f, 0.6f);
        sc.AddKey(1f,    0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sc);

        ParticleSystemRenderer psRend = ps.GetComponent<ParticleSystemRenderer>();
        psRend.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (psRend.material.shader.name == "Hidden/InternalErrorShader")
            psRend.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        psRend.renderMode        = ParticleSystemRenderMode.Billboard;
        psRend.shadowCastingMode = ShadowCastingMode.Off;

        orbParticles[i] = ps;
        orbs[i]         = orb;
    }

    private void CreateTornado()
    {
        tornadoRoot   = new GameObject("AudioOrbs_Tornado");
        tornadoSystem = tornadoRoot.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = tornadoSystem.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(1.2f,   2.8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.4f,   2.2f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.007f, 0.025f);
        main.startColor      = new ParticleSystem.MinMaxGradient(Palette[0], Palette[4]);
        main.maxParticles    = TornadoCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.18f, -0.04f);

        ParticleSystem.EmissionModule emission = tornadoSystem.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = tornadoSystem.shape;
        shape.enabled         = true;
        shape.shapeType       = ParticleSystemShapeType.Circle;
        shape.radius          = BaseRadius * 0.9f;
        shape.radiusThickness = 0.4f;

        ParticleSystem.VelocityOverLifetimeModule vel = tornadoSystem.velocityOverLifetime;
        vel.enabled  = true;
        vel.space    = ParticleSystemSimulationSpace.Local;
        vel.orbitalY = new ParticleSystem.MinMaxCurve(2.5f,  4.5f);
        vel.radial   = new ParticleSystem.MinMaxCurve(-0.3f, 0.1f);

        ParticleSystem.ColorOverLifetimeModule colorOverLife = tornadoSystem.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient cg = new();
        cg.SetKeys(
                new GradientColorKey[] { new(Palette[0], 0f), new(Palette[3], 0.5f), new(Palette[4], 1f), },
                new GradientAlphaKey[] { new(0f, 0f), new(0.7f, 0.3f), new(0.7f, 0.7f), new(0f, 1f), }
        );

        colorOverLife.color = new ParticleSystem.MinMaxGradient(cg);

        ParticleSystem.SizeOverLifetimeModule sizeOverLife = tornadoSystem.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sc = new();
        sc.AddKey(0f,   0.2f);
        sc.AddKey(0.3f, 1f);
        sc.AddKey(0.8f, 0.7f);
        sc.AddKey(1f,   0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sc);

        ParticleSystemRenderer rend = tornadoSystem.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (rend.material.shader.name == "Hidden/InternalErrorShader")
            rend.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        rend.renderMode        = ParticleSystemRenderMode.Billboard;
        rend.shadowCastingMode = ShadowCastingMode.Off;
        tornadoAlpha           = 0f;
    }

    protected override void Update()
    {
        VoiceManager.Get().GetMixedOutput(spectrum);

        float bassRaw                                  = 0f;
        for (int b = 0; b < BassBinCount; b++) bassRaw += spectrum[b];
        bassRaw /= BassBinCount;

        float midRaw                                         = 0f;
        for (int b = MidBinStart; b < MidBinEnd; b++) midRaw += spectrum[b];
        midRaw /= MidBinEnd - MidBinStart;

        float hiRaw                                       = 0f;
        for (int b = HiBinStart; b < HiBinEnd; b++) hiRaw += spectrum[b];
        hiRaw /= HiBinEnd - HiBinStart;

        float dtFast = 1f - Mathf.Exp(-22f  * Time.deltaTime);
        float dtMed  = 1f - Mathf.Exp(-8f   * Time.deltaTime);
        float dtSlow = 1f - Mathf.Exp(-2.5f * Time.deltaTime);

        bassSmoothedFast = Mathf.Lerp(bassSmoothedFast, bassRaw, dtFast);
        bassSmoothedSlow = Mathf.Lerp(bassSmoothedSlow, bassRaw, dtSlow);
        midSmoothed      = Mathf.Lerp(midSmoothed,      midRaw,  dtMed);
        hiSmoothed       = Mathf.Lerp(hiSmoothed,       hiRaw,   dtMed);

        bassNorm         = Mathf.Clamp01(bassSmoothedFast * 18f);
        globalBassImpact = Mathf.Lerp(globalBassImpact, bassNorm, dtFast);

        float totalEnergy = bassSmoothedFast + midSmoothed * 0.5f;
        buildupRaw   += totalEnergy  * Time.deltaTime * 3f;
        buildupRaw   -= BuildupDecay * Time.deltaTime;
        buildupRaw   =  Mathf.Max(0f, buildupRaw);
        buildupLevel =  Mathf.Clamp01(buildupRaw / 4f);

        beatCooldownTimer -= Time.deltaTime;
        bool isBeat = bassSmoothedFast  > bassSmoothedSlow * BeatThreshold
                   && bassSmoothedFast  > 0.0015f
                   && beatCooldownTimer <= 0f;

        if (isBeat)
        {
            beatCooldownTimer = BeatCooldown;
            OnBeat(bassNorm);
        }

        tiltX      = Mathf.Lerp(tiltX, targetTiltX, Time.deltaTime * TiltLerpSpeed);
        tiltZ      = Mathf.Lerp(tiltZ, targetTiltZ, Time.deltaTime * TiltLerpSpeed);
        shapeBlend = Mathf.Clamp01(shapeBlend + Time.deltaTime * ShapeLerpSpeed);

        globalAngle += Time.deltaTime * (0.25f + globalSpinSpeed + midSmoothed * 0.6f + globalBassImpact * 0.35f);

        targetTiltX += Mathf.Sin(Time.time * 0.25f) * 0.002f;
        targetTiltZ += Mathf.Cos(Time.time * 0.2f)  * 0.002f;
        targetTiltX =  Mathf.Clamp(targetTiltX, -1.2f, 1.2f);
        targetTiltZ =  Mathf.Clamp(targetTiltZ, -1.2f, 1.2f);

        if (rippleActive)
        {
            ripplePhase += Time.deltaTime * rippleSpeed;
            for (int i = 0; i < OrbCount; i++)
            {
                float orbAngle  = i / (float)OrbCount * Mathf.PI * 2f;
                float waveInput = orbAngle - ripplePhase;
                rippleWave[i] = Mathf.Max(0f,
                        Mathf.Sin(waveInput) * Mathf.Exp(-Mathf.Abs(Mathf.Sin(waveInput * 0.5f)) * 1.5f));
            }

            if (ripplePhase > Mathf.PI * 6f) rippleActive = false;
        }
        else
        {
            for (int i = 0; i < OrbCount; i++)
                rippleWave[i] = Mathf.Lerp(rippleWave[i], 0f, Time.deltaTime * 5f);
        }

        fogSnapTimer -= Time.deltaTime;
        float fogLerpSpeed = fogSnapTimer > 0f ? 18f : 4f;
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, Time.deltaTime * fogLerpSpeed);
        ApplyFog(currentFogColor);

        UpdateTornado();
        UpdateOrbs();
    }

    private void UpdateOrbs()
    {
        int bandsPerOrb = Mathf.Max(1, SampleSize / OrbCount);

        for (int i = 0; i < OrbCount; i++)
        {
            float sum = 0f;
            for (int j = 0; j < bandsPerOrb; j++)
            {
                int idx                        = i * bandsPerOrb + j;
                if (idx < spectrum.Length) sum += spectrum[idx];
            }

            smoothedAmplitudes[i] = Mathf.Lerp(
                    smoothedAmplitudes[i], sum / bandsPerOrb,
                    1f - Mathf.Exp(-10f * Time.deltaTime)
            );

            float amp  = smoothedAmplitudes[i];
            float ampN = Mathf.Clamp01(amp * 20f);

            GetShapePos(i, currentShape, amp, out float rFrom, out float yFrom, out float aoFrom);
            GetShapePos(i, nextShape,    amp, out float rTo,   out float yTo,   out float aoTo);

            float r           = Mathf.Lerp(rFrom,  rTo,  shapeBlend);
            float y           = Mathf.Lerp(yFrom,  yTo,  shapeBlend);
            float angleOffset = Mathf.Lerp(aoFrom, aoTo, shapeBlend);

            r += rippleWave[i] * (0.3f + bassNorm * 0.5f);

            float baseAngle = i / (float)OrbCount * Mathf.PI * 2f + globalAngle + angleOffset;

            float x       = Mathf.Cos(baseAngle) * r;
            float yCircle = Mathf.Sin(baseAngle) * r;
            float tiltedY = yCircle + x * Mathf.Sin(tiltX) + y * Mathf.Sin(tiltZ);

            float vertWobble = Mathf.Sin(Time.time * 0.85f + orbVerticalPhase[i]) *
                               (0.04f + amp * 0.25f + bassNorm * 0.08f);

            Vector3 pos = anchorPosition + new Vector3(x, tiltedY + vertWobble, y);
            orbs[i].transform.position = pos;

            float scale = 0.06f + ampN * 0.06f + globalBassImpact * 0.07f + rippleWave[i] * 0.05f;
            orbs[i].transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.04f, 0.2f);

            Color orbCol     = GetOrbColor(i);
            float brighten   = Mathf.Clamp01(globalBassImpact * 0.55f + ampN * 0.25f);
            Color displayCol = Color.Lerp(orbCol, Color.Lerp(orbCol, Color.white, 0.45f), brighten);
            orbMats[i].color = displayCol;
            if (orbMats[i].HasProperty("_EmissionColor"))
                orbMats[i].SetColor("_EmissionColor", displayCol * (2.5f + brighten * 3f));

            Color    trailCol = Color.Lerp(orbCol, Color.Lerp(orbCol, Color.white, 0.3f), brighten * 0.5f);
            Gradient tg       = new();
            tg.SetKeys(
                    new GradientColorKey[]
                    {
                            new(trailCol, 0f), new(Color.Lerp(trailCol, Color.white, 0.25f), 0.1f), new(trailCol, 1f),
                    },
                    new GradientAlphaKey[] { new(1f, 0f), new(0.5f, 0.4f), new(0f, 1f), }
            );

            trails[i].colorGradient = tg;
            trails[i].time          = Mathf.Lerp(0.18f,  0.65f,  ampN + globalBassImpact * 0.35f);
            trails[i].startWidth    = Mathf.Lerp(0.015f, 0.065f, ampN + globalBassImpact * 0.3f);

            float particleDrive = Mathf.Max(
                    Mathf.Pow(Mathf.Max(buildupLevel - 0.3f, 0f) / 0.7f, 1.5f),
                    Mathf.Clamp01((globalBassImpact - 0.5f) * 2f) * ampN
            );

            ParticleSystem.EmissionModule emission = orbParticles[i].emission;
            if (particleDrive > 0.02f)
            {
                emission.rateOverTime = Mathf.Lerp(0f, 35f, particleDrive);
                if (!orbParticles[i].isPlaying) orbParticles[i].Play();
            }
            else
            {
                emission.rateOverTime = 0f;
            }

            ParticleSystem.MainModule psMain = orbParticles[i].main;
            psMain.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.015f + particleDrive * 0.018f);
        }
    }

    private void UpdateTornado()
    {
        float targetAlpha = Mathf.Pow(buildupLevel, 1.8f);
        tornadoAlpha = Mathf.Lerp(tornadoAlpha, targetAlpha, Time.deltaTime * 2.5f);

        tornadoRoot.transform.position = anchorPosition;
        tornadoRoot.transform.Rotate(Vector3.up, Time.deltaTime * (60f + buildupLevel * 120f + bassNorm * 80f));

        ParticleSystem.EmissionModule emission = tornadoSystem.emission;
        emission.rateOverTime = Mathf.Lerp(0f, TornadoCount * 0.8f, tornadoAlpha);

        switch (tornadoAlpha)
        {
            case > 0.02f when !tornadoSystem.isPlaying:
                tornadoSystem.Play();

                break;

            case <= 0.02f when tornadoSystem.isPlaying:
                tornadoSystem.Stop();

                break;
        }

        Color dominantColor = buildupLevel > 0.5f ? Palette[4] : Palette[0];
        Color tornadoStart  = Color.Lerp(dominantColor, Color.white, tornadoAlpha * 0.3f);
        tornadoStart.a = tornadoAlpha;

        ParticleSystem.MainModule main = tornadoSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(tornadoStart, Palette[3]);

        ParticleSystem.ShapeModule shape = tornadoSystem.shape;
        shape.radius = BaseRadius * (0.7f + buildupLevel * 0.4f + bassNorm * 0.2f);

        ParticleSystem.ColorOverLifetimeModule colorOverLife = tornadoSystem.colorOverLifetime;
        Gradient                               cg            = new();
        cg.SetKeys(
                new GradientColorKey[]
                {
                        new(Color.Lerp(dominantColor, Color.white, 0.3f), 0f), new(dominantColor, 0.5f),
                        new(dominantColor, 1f),
                },
                new GradientAlphaKey[]
                        { new(0f, 0f), new(tornadoAlpha * 0.8f, 0.25f), new(tornadoAlpha * 0.6f, 0.75f), new(0f, 1f), }
        );

        colorOverLife.color = new ParticleSystem.MinMaxGradient(cg);
    }

    private void OnBeat(float bass)
    {
        bool changeShape = rng.NextDouble() < 0.42f || buildupLevel > 0.75f;
        if (changeShape)
        {
            currentShape = nextShape;
            shapeBlend   = 0f;

            double sr = rng.NextDouble();
            int candidate = sr   < 0.30 ? 0
                            : sr < 0.44 ? 1
                            : sr < 0.54 ? 2
                            : sr < 0.62 ? 3
                            : sr < 0.70 ? 4
                            : sr < 0.77 ? 5
                            : sr < 0.83 ? 6
                            : sr < 0.88 ? 7
                            : sr < 0.93 ? 8
                                          : 9;

            nextShape = candidate == currentShape ? (candidate + 1) % TotalShapes : candidate;
        }

        bool changeTilt = rng.NextDouble() < 0.60f;
        if (changeTilt)
        {
            double tr = rng.NextDouble();
            switch (tr)
            {
                case < 0.32:
                    targetTiltX = 0f;
                    targetTiltZ = 0f;

                    break;

                case < 0.50:
                    targetTiltX = (float)(rng.NextDouble() * 0.75 - 0.375);
                    targetTiltZ = 0f;

                    break;

                case < 0.68:
                    targetTiltX = 0f;
                    targetTiltZ = (float)(rng.NextDouble() * 0.75 - 0.375);

                    break;

                case < 0.82:
                    targetTiltX = (float)(rng.NextDouble() * 0.5 - 0.25);
                    targetTiltZ = (float)(rng.NextDouble() * 0.5 - 0.25);

                    break;

                default:
                    targetTiltX = (float)(rng.NextDouble() > 0.5
                                                  ? 1.45  + rng.NextDouble() * 0.2
                                                  : -1.45 - rng.NextDouble() * 0.2);

                    targetTiltZ = 0f;

                    break;
            }
        }

        if (bass > 0.45f && !rippleActive && (currentShape == 0 || currentShape == 1))
        {
            rippleActive = true;
            ripplePhase  = 0f;
            rippleSpeed  = 3f + bass * 5f;
        }

        int   fogIdx        = (int)(rng.NextDouble() * Palette.Length);
        Color fogBase       = Palette[fogIdx];
        float fogBrightness = 0.12f + bass * 0.12f;
        targetFogColor = new Color(
                fogBase.r * fogBrightness,
                fogBase.g * fogBrightness,
                fogBase.b * fogBrightness,
                0.6f + bass * 0.35f
        );

        fogSnapTimer = 0.08f;
    }

    private void GetShapePos(int i, int shape, float amp, out float r, out float y, out float angleOffset)
    {
        float       t   = i / (float)OrbCount;
        const float PI2 = Mathf.PI * 2f;
        angleOffset = 0f;

        switch (shape % TotalShapes)
        {
            case 0:
                r = BaseRadius + amp * 0.4f;
                y = Mathf.Sin(t * PI2 + Time.time * 0.4f) * (0.07f + amp * 0.3f + bassNorm * 0.12f);

                break;

            case 1:
                float wt = t * PI2;
                r = BaseRadius * (0.88f + 0.14f * Mathf.Sin(wt * 4f + Time.time * 1.3f)) + amp * 0.4f;
                y = Mathf.Sin(t * PI2 * 3f + Time.time * 0.75f) * (0.55f + bassNorm * 0.25f) + amp * 0.2f;

                break;

            case 2:
                float spiralR = BaseRadius * (0.4f + t * 0.7f);
                r = spiralR         + amp                     * 0.35f;
                y = t * 2.2f - 1.1f + Mathf.Sin(t * PI2 * 2f) * 0.14f + amp * 0.4f;

                break;

            case 3:
                float ft = t * PI2;
                r = BaseRadius * (1f + 0.3f * Mathf.Cos(ft * 3f)) + amp * 0.4f;
                y = Mathf.Sin(ft * 3f + Time.time * 0.5f) * (0.65f + amp * 0.35f);

                break;

            case 4:
                float lt2 = t * PI2;
                r           = BaseRadius * (0.6f + 0.4f * Mathf.Abs(Mathf.Cos(lt2 * 1.5f))) + amp * 0.35f;
                y           = Mathf.Sin(lt2 * 2f + Time.time * 0.4f) * (0.9f + amp * 0.35f);
                angleOffset = Mathf.Sin(t * PI2 * 2f)                * 0.12f;

                break;

            case 5:
                float ct = t          * PI2;
                float cR = BaseRadius * (0.8f + 0.22f * Mathf.Sin(Time.time * 0.65f));
                r = cR + amp * 0.5f;
                y = Mathf.Cos(ct) * (1.4f + amp * 0.5f + bassNorm * 0.3f);

                break;

            case 6:
                float ht = t          * PI2;
                float hR = BaseRadius * (1f + 0.35f * Mathf.Cos(ht * 2f + Time.time * 0.55f));
                r = hR + amp                              * 0.4f;
                y = Mathf.Sin(ht * 4f + Time.time * 0.7f) * (0.8f + amp * 0.4f) + Mathf.Cos(ht * 2f) * 0.4f;

                break;

            case 7:
                float st = t * PI2 * 2.5f;
                r           = BaseRadius * (0.55f + 0.5f * Mathf.Abs(Mathf.Sin(st * 0.5f))) + amp * 0.4f;
                y           = t * 2.5f - 1.25f + Mathf.Sin(st) * 0.5f + amp * 0.35f;
                angleOffset = Mathf.Cos(t * PI2 * 3f) * 0.18f;

                break;

            case 8:
                float et = t * PI2;
                r = BaseRadius * (0.75f + 0.3f * Mathf.Cos(et * 5f)) + amp * 0.4f;
                y = Mathf.Sin(et * 5f + Time.time * 0.9f) * (0.7f + amp * 0.4f) +
                    Mathf.Cos(et * 2f + Time.time * 0.3f) * 0.45f;

                break;

            default:
                float vt = t          * PI2;
                float vr = BaseRadius * (0.9f + 0.2f * Mathf.Sin(vt * 3f + Time.time * 0.8f));
                r = vr + amp           * 0.45f;
                y = Mathf.Sin(vt * 2f) * Mathf.Cos(vt) * 2f * (0.6f + bassNorm * 0.25f) + amp * 0.3f;

                break;
        }
    }

    private static Color GetOrbColor(int i)
    {
        float t      = i                        / (float)OrbCount;
        float scaled = t                        * Palette.Length;
        int   pa     = Mathf.FloorToInt(scaled) % Palette.Length;
        int   pb     = (pa + 1)                 % Palette.Length;

        return Color.Lerp(Palette[pa], Palette[pb], scaled % 1f);
    }

    private void ApplyFog(Color c) =>
            ZoneShaderSettings.activeInstance?.SetGroundFogValue(c, 0f, float.MaxValue, 0f);

    protected override void OnDisable()
    {
        ApplyFog(new Color(0f, 0f, 0f, 0f));

        if (tornadoRoot != null)
            Object.Destroy(tornadoRoot);

        tornadoRoot   = null;
        tornadoSystem = null;

        if (orbs == null)
            return;

        foreach (GameObject t in orbs)
            if (t != null)
                Object.Destroy(t);

        orbs         = null;
        trails       = null;
        orbRends     = null;
        orbMats      = null;
        orbParticles = null;
    }
}