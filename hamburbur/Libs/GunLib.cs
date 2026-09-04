using System.Collections.Generic;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Libs;

public enum GunType
{
    Rope,
    Static,
    Bezier,
    Straight,
    Wave,
    Helix,
    Zigzag,
    Lightning,
    Arc,
    Ribbon,
    Sawtooth,
    SquareWave,
    Pulse,
    Petals,
    Heartbeat,
}

public enum GunOrigin
{
    Hand,
    Head,
    BodyBottom,
}

public enum GunDirection
{
    Forward,
    Palm,
    Knuckles,
    ForwardSlightUp,
    ForwardUp,
    ForwardSteepUp,
    ForwardSlightDown,
    ForwardDown,
    ForwardSteepDown,
}

public enum GunColourPreset
{
    ThemePulse,
    ThemeGradient,
    Rainbow,
    Fire,
    Ocean,
    Neon,
    White,
    Red,
    Green,
    Blue,
    Purple,
    Pink,
}

public class GunLib
{
    private const int   NumPoints            = 50;
    private const int   ConstraintIterations = 20;
    private const float MaxRayDistance       = 1000f;

    public static GunType GunType = GunType.Straight;

    private static readonly float Gravity = Physics.gravity.magnitude;

    private static readonly Dictionary<LineRenderer, (Vector3[] previousPoints, Vector3[] currentPoints)> PointsDict =
            [];

    public VRRig ChosenRig;

    private LineRenderer gunLine;

    public  RaycastHit Hit;
    public  bool       IsShooting;
    private float      nextVibrationTime;
    public  bool       ShouldFollow;
    private GameObject targetMarker;
    private Renderer   targetMarkerRenderer;

    public void Start()
    {
        gunLine                   = new GameObject("GunLine").AddComponent<LineRenderer>();
        gunLine.positionCount     = NumPoints;
        gunLine.useWorldSpace     = true;
        gunLine.numCapVertices    = 4;
        gunLine.numCornerVertices = 4;
        gunLine.material          = new Material(Shaders.TextShader) { color = Color.white, };
        gunLine.gameObject.layer  = 2;
        gunLine.gameObject.SetActive(false);

        targetMarker       = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetMarker.name  = "GunTargetMarker";
        targetMarker.layer = 2;
        targetMarker.SetActive(false);

        Collider markerCollider = targetMarker.GetComponent<Collider>();
        if (markerCollider != null)
            Object.Destroy(markerCollider);

        targetMarkerRenderer = targetMarker.GetComponent<Renderer>();
        if (targetMarkerRenderer != null)
            targetMarkerRenderer.material = new Material(Shaders.UberShader);
    }

    public void OnDisable()
    {
        HideVisuals();
        IsShooting = false;
        ChosenRig  = null;
    }

    private static bool GetGunInput(bool isTriggerInput = false) =>
            LeftHandedGun.IsEnabled
                    ? isTriggerInput
                              ? InputManager.Instance.LeftTrigger.IsPressed
                              : InputManager.Instance.LeftGrip.IsPressed
                    : isTriggerInput
                            ? InputManager.Instance.RightTrigger.IsPressed
                            : InputManager.Instance.RightGrip.IsPressed;
    
    public static Transform GetGunHand() => LeftHandedGun.IsEnabled
                                                    ? Tools.Utils.RealLeftController
                                                    : Tools.Utils.RealRightController;
    
    public static Transform GetAlternateHand() => LeftHandedGun.IsEnabled
                                                    ? Tools.Utils.RealRightController
                                                    : Tools.Utils.RealLeftController;

    public void LateUpdate()
    {
        if (GetGunInput())
        {
            Transform originController = GetGunHand();

            Vector3 gunDirection = GetGunDirection(originController);
            Vector3 gunPosition  = GetGunOrigin(originController, gunDirection);

            HandleShooting(new Ray(gunPosition, gunDirection), GetGunInput(true), gunPosition, true);
        }
        else if (Mouse.current != null && Mouse.current.backButton.isPressed)
        {
            Camera cameraToUse = Tools.Utils.GetActiveCamera();
            Ray    ray         = cameraToUse.ScreenPointToRay(Mouse.current.position.ReadValue());

            HandleShooting(
                    ray,
                    Mouse.current.leftButton.isPressed,
                    GTPlayer.Instance.bodyCollider.transform.position,
                    false);
        }
        else
        {
            HideVisuals();
            IsShooting = false;
            ChosenRig  = null;
        }
    }

    private static Vector3 GetGunDirection(Transform controller)
    {
        bool isLeftHand = LeftHandedGun.IsEnabled;

        Vector3 direction = ChangeGunDirection.CurrentValue switch
                            {
                                    GunDirection.Palm              => isLeftHand ? controller.right : -controller.right,
                                    GunDirection.Knuckles          => isLeftHand ? -controller.right : controller.right,
                                    GunDirection.ForwardSlightUp   => GetAngledForward(controller, 10f),
                                    GunDirection.ForwardUp         => GetAngledForward(controller, 20f),
                                    GunDirection.ForwardSteepUp    => GetAngledForward(controller, 35f),
                                    GunDirection.ForwardSlightDown => GetAngledForward(controller, -10f),
                                    GunDirection.ForwardDown       => GetAngledForward(controller, -20f),
                                    GunDirection.ForwardSteepDown  => GetAngledForward(controller, -35f),
                                    var _                          => controller.forward,
                            };

        return direction.sqrMagnitude > 0f ? direction.normalized : controller.forward;
    }

    private static Vector3 GetAngledForward(Transform controller, float verticalAngle)
    {
        Vector3 verticalTarget = verticalAngle >= 0f ? controller.up : -controller.up;

        return Vector3.RotateTowards(
                controller.forward,
                verticalTarget,
                Mathf.Abs(verticalAngle) * Mathf.Deg2Rad,
                0f);
    }

    private static Vector3 GetGunOrigin(Transform controller, Vector3 direction)
    {
        Vector3 origin = ChangeGunOrigin.CurrentValue switch
                         {
                                 GunOrigin.Head       => GetAboveHeadOrigin(),
                                 GunOrigin.BodyBottom => GetBodyBottom(),
                                 var _                => controller.position,
                         };

        return origin + direction * (ChangeGunOriginOffset.CurrentValue * GTPlayer.Instance.scale);
    }

    private static Vector3 GetAboveHeadOrigin()
    {
        Collider headCollider = GTPlayer.Instance.headCollider;
        Bounds   headBounds   = headCollider.bounds;
        Vector3  headOrigin   = headBounds.center;

        headOrigin.y = headBounds.max.y + 0.03f * GTPlayer.Instance.scale;

        return headOrigin;
    }

    private static Vector3 GetBodyBottom()
    {
        Bounds bodyBounds = GTPlayer.Instance.bodyCollider.bounds;

        return new Vector3(bodyBounds.center.x, bodyBounds.min.y, bodyBounds.center.z);
    }

    private void HandleShooting(Ray ray, bool shooting, Vector3 visualOrigin, bool allowVibration)
    {
        IsShooting = shooting;

        if (allowVibration && shooting && GunVibrations.IsEnabled && Time.time >= nextVibrationTime)
        {
            GorillaTagger.Instance?.StartVibration(
                    LeftHandedGun.IsEnabled,
                    ChangeGunVibrationStrength.CurrentValue,
                    0.04f);

            nextVibrationTime = Time.time + 0.06f;
        }

        bool didHit = PhysicsRaycast(ray, VRRig.LocalRig, ref ChosenRig, out Hit, out ChosenRig);

        if (!shooting)
            ChosenRig = null;

        Vector3 lineEnd;
        if (didHit)
        {
            lineEnd = Hit.point;

            if (shooting && ShouldFollow && ChosenRig != null)
                lineEnd = ChosenRig.transform.position;
        }
        else
        {
            lineEnd = ChosenRig == null
                              ? ray.origin + ray.direction * MaxRayDistance
                              : ChosenRig.transform.position;
        }

        gunLine.gameObject.SetActive(true);
        ApplyGunAppearance(out Color _, out Color endColour);
        HandleShootingVisuals(visualOrigin, lineEnd, shooting || AlwaysAnimateGun.IsEnabled, gunLine);
        UpdateTargetMarker(lineEnd, endColour);
    }

    private void ApplyGunAppearance(out Color startColour, out Color endColour)
    {
        GetGunColours(out startColour, out endColour);

        gunLine.material.color = Color.white;
        gunLine.startColor     = startColour;
        gunLine.endColor       = endColour;

        float width = 0.0125f * ChangeGunLineThickness.CurrentValue * GTPlayer.Instance.scale;
        gunLine.startWidth = width;
        gunLine.endWidth   = width;
    }

    private static void GetGunColours(out Color startColour, out Color endColour)
    {
        float pulse = Mathf.PingPong(Time.time, 1f);

        switch (ChangeGunColour.CurrentValue)
        {
            case GunColourPreset.ThemeGradient:
                startColour = Plugin.Instance.MainColour;
                endColour   = Plugin.Instance.SecondaryColour;

                break;

            case GunColourPreset.Rainbow:
                startColour = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.15f,         1f), 1f, 1f);
                endColour   = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.15f + 0.25f, 1f), 1f, 1f);

                break;

            case GunColourPreset.Fire:
                startColour = new Color(1f, 0.75f, 0.05f);
                endColour   = new Color(1f, 0.05f, 0.01f);

                break;

            case GunColourPreset.Ocean:
                startColour = new Color(0.05f, 1f,    1f);
                endColour   = new Color(0.02f, 0.15f, 1f);

                break;

            case GunColourPreset.Neon:
                startColour = Color.magenta;
                endColour   = Color.cyan;

                break;

            case GunColourPreset.White:
                startColour = endColour = Color.white;

                break;

            case GunColourPreset.Red:
                startColour = endColour = Color.red;

                break;

            case GunColourPreset.Green:
                startColour = endColour = Color.green;

                break;

            case GunColourPreset.Blue:
                startColour = endColour = Color.blue;

                break;

            case GunColourPreset.Purple:
                startColour = endColour = new Color(0.55f, 0.1f, 1f);

                break;

            case GunColourPreset.Pink:
                startColour = endColour = new Color(1f, 0.15f, 0.65f);

                break;

            case GunColourPreset.ThemePulse:
            default:
                startColour = endColour = Color.Lerp(
                                      Plugin.Instance.MainColour,
                                      Plugin.Instance.SecondaryColour,
                                      pulse);

                break;
        }
    }

    private void UpdateTargetMarker(Vector3 lineEnd, Color colour)
    {
        if (!GunTargetMarker.IsEnabled)
        {
            targetMarker.SetActive(false);

            return;
        }

        targetMarker.SetActive(true);
        targetMarker.transform.position = lineEnd;
        targetMarker.transform.localScale = Vector3.one *
                                            (ChangeGunMarkerSize.CurrentValue * GTPlayer.Instance.scale);

        if (targetMarkerRenderer != null)
            targetMarkerRenderer.material.color = colour;
    }

    private void HideVisuals()
    {
        if (gunLine != null)
            gunLine.gameObject.SetActive(false);

        if (targetMarker != null)
            targetMarker.SetActive(false);
    }

    private static void HandleShootingVisuals(Vector3 origin, Vector3 end, bool doSpecial, LineRenderer lineToImpact)
    {
        EnsurePointBuffers(lineToImpact, origin, end);

        (Vector3[] previousPoints, Vector3[] currentPoints) = PointsDict[lineToImpact];

        if (!doSpecial)
        {
            SetStraightLine(previousPoints, currentPoints, origin, end);
        }
        else
        {
            float distance = Vector3.Distance(origin, end);
            float amplitude = Mathf.Clamp(
                    distance * 0.025f,
                    0.04f    * GTPlayer.Instance.scale,
                    0.35f    * GTPlayer.Instance.scale);

            GetPerpendicularBasis(origin, end, out Vector3 right, out Vector3 up);

            switch (GunType)
            {
                case GunType.Rope:
                    SetRopeLine(previousPoints, currentPoints, origin, end);

                    break;

                case GunType.Static:
                    SetStaticLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Bezier:
                    SetBezierLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Wave:
                    SetWaveLine(previousPoints, currentPoints, origin, end, up, amplitude);

                    break;

                case GunType.Helix:
                    SetHelixLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Zigzag:
                    SetZigzagLine(previousPoints, currentPoints, origin, end, right, amplitude);

                    break;

                case GunType.Lightning:
                    SetLightningLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Arc:
                    SetArcLine(previousPoints, currentPoints, origin, end, up, amplitude);

                    break;

                case GunType.Ribbon:
                    SetRibbonLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Sawtooth:
                    SetSawtoothLine(previousPoints, currentPoints, origin, end, up, amplitude);

                    break;

                case GunType.SquareWave:
                    SetSquareWaveLine(previousPoints, currentPoints, origin, end, right, amplitude);

                    break;

                case GunType.Pulse:
                    SetPulseLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Petals:
                    SetPetalsLine(previousPoints, currentPoints, origin, end, right, up, amplitude);

                    break;

                case GunType.Heartbeat:
                    SetHeartbeatLine(previousPoints, currentPoints, origin, end, up, amplitude);

                    break;

                case GunType.Straight:
                default:
                    SetStraightLine(previousPoints, currentPoints, origin, end);

                    break;
            }
        }

        currentPoints[0]             = origin;
        currentPoints[NumPoints - 1] = end;

        lineToImpact.SetPositions(currentPoints);
    }

    private static void EnsurePointBuffers(LineRenderer line, Vector3 origin, Vector3 end)
    {
        if (PointsDict.ContainsKey(line))
            return;

        Vector3[] previousPoints = new Vector3[NumPoints];
        Vector3[] currentPoints  = new Vector3[NumPoints];
        SetStraightLine(previousPoints, currentPoints, origin, end);
        PointsDict[line] = (previousPoints, currentPoints);
    }

    private static void SetStraightLine(Vector3[] previousPoints, Vector3[] currentPoints, Vector3 origin, Vector3 end)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, i / (float)(NumPoints - 1));
        }
    }

    private static void SetRopeLine(Vector3[] previousPoints, Vector3[] currentPoints, Vector3 origin, Vector3 end)
    {
        currentPoints[0]             = origin;
        currentPoints[NumPoints - 1] = end;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        for (int i = 1; i < NumPoints - 1; i++)
        {
            Vector3 velocity = (currentPoints[i] - previousPoints[i]) / deltaTime;

            velocity          += (end - origin).normalized * 2f;
            previousPoints[i] =  currentPoints[i];
            currentPoints[i]  += velocity     * deltaTime;
            currentPoints[i]  += Vector3.down * (Gravity * deltaTime * deltaTime);
        }

        float targetDistance = Vector3.Distance(origin, end) / (NumPoints - 1);
        for (int iteration = 0; iteration < ConstraintIterations; iteration++)
            for (int i = 0; i < NumPoints - 1; i++)
            {
                Vector3 delta  = currentPoints[i + 1] - currentPoints[i];
                float   length = delta.magnitude;

                if (length <= Mathf.Epsilon)
                    continue;

                Vector3 correction = delta / length * ((length - targetDistance) * 0.5f);
                if (i != 0)
                    currentPoints[i] += correction;

                if (i != NumPoints - 2)
                    currentPoints[i + 1] -= correction;
            }
    }

    private static void SetStaticLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        float frame = Mathf.Floor(Time.time * 24f);

        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float xNoise   = Mathf.PerlinNoise(i * 0.71f,       frame * 0.17f) * 2f - 1f;
            float yNoise   = Mathf.PerlinNoise(i * 0.43f + 20f, frame * 0.19f) * 2f - 1f;

            previousPoints[i] = currentPoints[i];
            currentPoints[i] = Vector3.Lerp(origin, end, t) +
                               (right * xNoise + up * yNoise) * (amplitude * envelope);
        }
    }

    private static void SetBezierLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        float angle = Time.time * 3f;
        Vector3 control = Vector3.Lerp(origin, end, 0.5f)               +
                          up    * (Mathf.Sin(angle)        * amplitude) +
                          right * (Mathf.Cos(angle * 1.3f) * amplitude);

        for (int i = 0; i < NumPoints; i++)
        {
            float t   = i / (float)(NumPoints - 1);
            float omt = 1f - t;

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = omt * omt * origin + 2f * omt * t * control + t * t * end;
        }
    }

    private static void SetWaveLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float wave     = Mathf.Sin(t * Mathf.PI * 8f - Time.time * 6f);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + up * (wave * amplitude * envelope);
        }
    }

    private static void SetHelixLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float   t        = i / (float)(NumPoints - 1);
            float   envelope = Mathf.Sin(Mathf.PI * t);
            float   angle    = t * Mathf.PI * 10f - Time.time * 5f;
            Vector3 offset   = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * (amplitude * envelope);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + offset;
        }
    }

    private static void SetZigzagLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float zigzag   = Mathf.PingPong(t * 12f, 1f) * 2f - 1f;

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + right * (zigzag * amplitude * envelope);
        }
    }

    private static void SetLightningLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        float frame = Mathf.Floor(Time.time * 30f);

        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float xJolt    = Mathf.PerlinNoise(i * 1.83f,       frame * 0.31f) * 2f - 1f;
            float yJolt    = Mathf.PerlinNoise(i * 2.27f + 30f, frame * 0.29f) * 2f - 1f;

            previousPoints[i] = currentPoints[i];
            currentPoints[i] = Vector3.Lerp(origin, end, t) +
                               (right * xJolt + up * yJolt) * (amplitude * envelope);
        }
    }

    private static void SetArcLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float t   = i / (float)(NumPoints - 1);
            float arc = Mathf.Sin(Mathf.PI * t);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + up * (arc * amplitude * 1.5f);
        }
    }

    private static void SetRibbonLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float   t        = i / (float)(NumPoints - 1);
            float   envelope = Mathf.Sin(Mathf.PI * t);
            float   angle    = t     * Mathf.PI * 8f    - Time.time * 4f;
            Vector3 offset   = right * Mathf.Sin(angle) + up        * (Mathf.Sin(angle * 2f) * 0.45f);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + offset * (amplitude * envelope);
        }
    }

    private static void SetSawtoothLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float phase    = Mathf.Repeat(t * 10f - Time.time * 0.75f, 1f);
            float tooth    = phase * 2f - 1f;

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + up * (tooth * amplitude * envelope);
        }
    }

    private static void SetSquareWaveLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float wave     = Mathf.Sin(t * Mathf.PI * 12f - Time.time * 5f) >= 0f ? 1f : -1f;

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + right * (wave * amplitude * envelope);
        }
    }

    private static void SetPulseLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float   t        = i / (float)(NumPoints - 1);
            float   envelope = Mathf.Sin(Mathf.PI * t);
            float   pulse    = Mathf.Pow(Mathf.Abs(Mathf.Sin(t * Mathf.PI * 5f - Time.time * 3f)), 8f);
            float   angle    = t     * Mathf.PI * 6f    + Time.time * 2f;
            Vector3 radial   = right * Mathf.Cos(angle) + up        * Mathf.Sin(angle);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + radial * (pulse * amplitude * envelope);
        }
    }

    private static void SetPetalsLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   right,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float   t        = i / (float)(NumPoints - 1);
            float   envelope = Mathf.Sin(Mathf.PI * t);
            float   angle    = t * Mathf.PI * 8f - Time.time * 2f;
            float   radius   = Mathf.Sin(t * Mathf.PI * 16f - Time.time * 4f);
            Vector3 radial   = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + radial * (radius * amplitude * envelope);
        }
    }

    private static void SetHeartbeatLine(
            Vector3[] previousPoints,
            Vector3[] currentPoints,
            Vector3   origin,
            Vector3   end,
            Vector3   up,
            float     amplitude)
    {
        for (int i = 0; i < NumPoints; i++)
        {
            float t        = i / (float)(NumPoints - 1);
            float envelope = Mathf.Sin(Mathf.PI * t);
            float phase    = Mathf.Repeat(t * 3f - Time.time * 0.6f, 1f);
            float beat     = GetHeartbeatValue(phase);

            previousPoints[i] = currentPoints[i];
            currentPoints[i]  = Vector3.Lerp(origin, end, t) + up * (beat * amplitude * envelope);
        }
    }

    private static float GetHeartbeatValue(float phase) =>
            phase switch
            {
                    < 0.3f  => 0f,
                    < 0.38f => Mathf.Lerp(0f,     -0.25f, Mathf.InverseLerp(0.3f,  0.38f, phase)),
                    < 0.46f => Mathf.Lerp(-0.25f, 1f,     Mathf.InverseLerp(0.38f, 0.46f, phase)),
                    < 0.54f => Mathf.Lerp(1f,     -0.65f, Mathf.InverseLerp(0.46f, 0.54f, phase)),
                    < 0.64f => Mathf.Lerp(-0.65f, 0.2f,   Mathf.InverseLerp(0.54f, 0.64f, phase)),
                    < 0.74f => Mathf.Lerp(0.2f,   0f,     Mathf.InverseLerp(0.64f, 0.74f, phase)),
                    var _   => 0f,
            };

    private static void GetPerpendicularBasis(Vector3 origin, Vector3 end, out Vector3 right, out Vector3 up)
    {
        Vector3 forward = (end - origin).normalized;
        if (forward.sqrMagnitude <= Mathf.Epsilon)
            forward = Vector3.forward;

        Vector3 referenceUp = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f
                                      ? Vector3.right
                                      : Vector3.up;

        right = Vector3.Cross(forward, referenceUp).normalized;
        up    = Vector3.Cross(right,   forward).normalized;
    }

    private static bool PhysicsRaycast(
            Ray                        ray,
            VRRig                      toIgnore,
            ref             VRRig      chosenRig,
            out             RaycastHit hit,
            [CanBeNull] out VRRig      rig)
    {
        // ReSharper disable once Unity.PreferNonAllocApi
        RaycastHit[] hits = Physics.RaycastAll(ray, MaxRayDistance);

        hit = default(RaycastHit);
        float minDistance = float.MaxValue;

        foreach (RaycastHit candidate in hits)
            if ((1 << candidate.collider.gameObject.layer & GTPlayer.Instance.locomotionEnabledLayers) != 0 ||
                candidate.collider.GetComponentInParent<VRRig>() != null &&
                candidate.collider.GetComponentInParent<VRRig>() != toIgnore)
                if (candidate.distance < minDistance)
                {
                    minDistance = candidate.distance;
                    hit         = candidate;
                }

        rig = chosenRig == null ? hit.collider?.GetComponentInParent<VRRig>() : chosenRig;

        return hit.collider != null;
    }
}
