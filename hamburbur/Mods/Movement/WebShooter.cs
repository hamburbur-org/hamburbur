using System.Linq;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Web Shooters",       "Gives you Spider Man style web shooters", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class WebShooter : hamburburmod
{
    private const float MaxWebDistance  = 45f;
    private const float WebTravelSpeed  = 100f;
    private const float MinimumShotTime = 0.04f;
    private const float MaximumShotTime = 0.2f;
    private const float MissVisibleTime = 0.08f;

    private const float MinimumRopeLength          = 0.75f;
    private const float InitialRopeTension         = 0.1f;
    private const float RopeSlack                  = 0.035f;
    private const float RopeCorrectionSpeed        = 16f;
    private const float MaximumRopeCorrectionSpeed = 10f;

    private const float PullDeadZone       = 0.12f;
    private const float MaximumPullSpeed   = 4f;
    private const float PullReelMultiplier = 0.6f;
    private const float PullAcceleration   = 10f;

    private const float SwingPumpStrength       = 3.5f;
    private const float SwingMaintainStrength   = 0.8f;
    private const float MaximumAssistedVelocity = 18f;

    private const float FlipRotationSpeed  = 150f;
    private const float RollRotationSpeed  = 90f;
    private const float UprightReturnSpeed = 5f;
    private const float MinimumFlipSpeed   = 1f;

    private const int WebSegments          = 20;
    private const int ConstraintIterations = 2;

    private static AudioClip[] shootSounds;

    internal static bool Active;

    private WebState leftWeb;
    private WebState rightWeb;

    protected override void Start()
    {
        AssetBundle bundle = Plugin.Instance.HamburburBundle;

        shootSounds = bundle.GetAllAssetNames()
                            .Where(assetName =>
                                   {
                                       string fileName = assetName.Split('/').Last().Split('.')[0];

                                       return fileName.StartsWith("web") &&
                                              int.TryParse(fileName[3..], out int _);
                                   })
                            .OrderBy(assetName =>
                                     {
                                         string fileName = assetName.Split('/').Last().Split('.')[0];

                                         return int.Parse(fileName[3..]);
                                     })
                            .Select(bundle.LoadAsset<AudioClip>)
                            .Where(clip => clip != null)
                            .ToArray();
    }

    protected override void OnEnable()
    {
        Active = true;

        TryCreateWebShooters();
    }

    protected override void FixedUpdate()
    {
        if (!TryCreateWebShooters())
            return;

        Rigidbody body = GorillaTagger.Instance.rigidbody;

        UpdateHandMotion(leftWeb);
        UpdateHandMotion(rightWeb);

        UpdatePull(leftWeb,  body);
        UpdatePull(rightWeb, body);

        for (int i = 0; i < ConstraintIterations; i++)
        {
            ApplyRopeConstraint(leftWeb,  body);
            ApplyRopeConstraint(rightWeb, body);
        }

        ApplySwingAssist(body);
    }

    protected override void LateUpdate()
    {
        if (!TryCreateWebShooters())
            return;

        UpdateInput(leftWeb,  InputManager.Instance.LeftGrip.IsPressed);
        UpdateInput(rightWeb, InputManager.Instance.RightGrip.IsPressed);

        UpdateWebVisual(leftWeb);
        UpdateWebVisual(rightWeb);

        UpdateSwingRotation();
    }

    protected override void OnDisable()
    {
        Active = false;

        ReleaseWeb(leftWeb);
        ReleaseWeb(rightWeb);

        DestroyWebState(leftWeb);
        DestroyWebState(rightWeb);

        leftWeb  = null;
        rightWeb = null;

        if (!SpiderWalk.Active)
            MakeUpright(true);
    }

    private bool TryCreateWebShooters()
    {
        if (leftWeb != null && rightWeb != null)
            return true;

        if (VRRig.LocalRig == null)
            return false;

        Transform leftController  = Tools.Utils.RealLeftController;
        Transform rightController = Tools.Utils.RealRightController;

        Transform leftRigHand  = VRRig.LocalRig.leftHand.rigTarget;
        Transform rightRigHand = VRRig.LocalRig.rightHand.rigTarget;

        if (leftController  == null ||
            rightController == null ||
            leftRigHand     == null ||
            rightRigHand    == null)
        {
            return false;
        }

        leftWeb = CreateWebState(
                leftController,
                leftRigHand,
                true);

        rightWeb = CreateWebState(
                rightController,
                rightRigHand,
                false);

        return true;
    }

    private WebState CreateWebState(Transform hand, Transform rigHand, bool isLeft)
    {
        Transform turnParent = GTPlayer.Instance.turnParent.transform;

        WebState state = new()
        {
                Hand                  = hand,
                IsLeft                = isLeft,
                LastLocalHandPosition = turnParent.InverseTransformPoint(hand.position),
        };

        GameObject shooter = new(isLeft
                                         ? "Left Web Shooter"
                                         : "Right Web Shooter");

        shooter.transform.SetParent(rigHand, false);

        shooter.transform.localPosition = new Vector3(
                isLeft ? -0.0518f : 0.0518f,
                0.0283f,
                -0.0041f);

        shooter.transform.localEulerAngles = isLeft ? new Vector3(280f, 270f, 180f) : new Vector3(270f, 270f, 0f);
        shooter.transform.localScale       = Vector3.one * 0.7f;

        state.Shooter = shooter.transform;

        CreateShooterPart(
                state.Shooter,
                PrimitiveType.Cube,
                "Base",
                Vector3.zero,
                new Vector3(0.065f, 0.018f, 0.08f),
                Quaternion.identity,
                Plugin.Instance.MainColour);

        CreateShooterPart(
                state.Shooter,
                PrimitiveType.Cube,
                "Plate",
                new Vector3(0f,     0.014f, 0.002f),
                new Vector3(0.044f, 0.012f, 0.05f),
                Quaternion.identity,
                Plugin.Instance.SecondaryColour);

        CreateShooterPart(
                state.Shooter,
                PrimitiveType.Cylinder,
                "Nozzle",
                new Vector3(0f,    0.004f, 0.045f),
                new Vector3(0.01f, 0.013f, 0.01f),
                Quaternion.Euler(90f, 0f, 0f),
                Plugin.Instance.SecondaryColour);

        GameObject muzzle = new("Muzzle");

        muzzle.transform.SetParent(state.Shooter, false);
        muzzle.transform.localPosition = new Vector3(0f, 0.004f, 0.064f);
        muzzle.transform.localRotation = Quaternion.identity;

        state.Muzzle = muzzle.transform;

        AudioSource audioSource = shooter.AddComponent<AudioSource>();

        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = 0.8f;
        audioSource.minDistance  = 0.25f;
        audioSource.maxDistance  = 6f;

        state.AudioSource = audioSource;

        GameObject webObject = new(isLeft
                                           ? "Left Web"
                                           : "Right Web");

        LineRenderer line = webObject.AddComponent<LineRenderer>();

        line.useWorldSpace     = true;
        line.positionCount     = WebSegments;
        line.numCapVertices    = 6;
        line.numCornerVertices = 3;

        line.startWidth = 0.012f;
        line.endWidth   = 0.006f;

        line.startColor = new Color(0.96f, 0.98f, 1f, 0.98f);
        line.endColor   = new Color(0.88f, 0.93f, 1f, 0.92f);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.enabled  = false;

        state.WebObject = webObject;
        state.Line      = line;

        return state;
    }

    private static void CreateShooterPart(
            Transform     parent,
            PrimitiveType primitiveType,
            string        name,
            Vector3       position,
            Vector3       scale,
            Quaternion    rotation,
            Color         colour)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);

        part.name = name;

        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localRotation = rotation;
        part.transform.localScale    = scale;

        if (part.TryGetComponent(out Collider collider))
            collider.Obliterate();

        if (!part.TryGetComponent(out Renderer renderer))
            return;

        renderer.material.shader = Shaders.UberShader;
        renderer.material.color  = colour;
    }

    private void UpdateInput(WebState state, bool pressed)
    {
        if (pressed && !state.WasPressed)
            FireWeb(state);

        if (!pressed && state.WasPressed)
            ReleaseWeb(state);

        state.WasPressed = pressed;

        if (!state.IsFired)
            return;

        if (state.HasHit      &&
            !state.IsAttached &&
            Time.time >= state.AttachTime)
        {
            AttachWeb(state);
        }

        if (!state.HasHit &&
            Time.time >= state.HideTime)
        {
            state.IsFired      = false;
            state.Line.enabled = false;
        }
    }

    private void FireWeb(WebState state)
    {
        Vector3 origin    = state.Muzzle.position;
        Vector3 direction = state.Hand.forward;

        state.IsFired         = true;
        state.IsAttached      = false;
        state.HasHit          = false;
        state.AnchorTransform = null;
        state.PullSpeed       = 0f;
        state.WobbleStrength  = 0.07f;
        state.ShotStartedAt   = Time.time;

        if (shootSounds is { Length: > 0, } &&
            state.AudioSource != null)
        {
            AudioClip clip = shootSounds[Random.Range(0, shootSounds.Length)];

            state.AudioSource.PlayOneShot(clip);
        }

        bool didHit = Physics.SphereCast(
                origin,
                0.02f,
                direction,
                out RaycastHit hit,
                MaxWebDistance,
                GTPlayer.Instance.locomotionEnabledLayers,
                QueryTriggerInteraction.Ignore);

        if (didHit)
        {
            state.HasHit       = true;
            state.StaticAnchor = hit.point;

            if (hit.collider != null)
            {
                state.AnchorTransform  = hit.collider.transform;
                state.AnchorLocalPoint = state.AnchorTransform.InverseTransformPoint(hit.point);
            }

            float distance = Vector3.Distance(origin, hit.point);

            state.ShotDuration = Mathf.Clamp(
                    distance / WebTravelSpeed,
                    MinimumShotTime,
                    MaximumShotTime);

            state.AttachTime = Time.time + state.ShotDuration;
        }
        else
        {
            state.MissPoint = origin + direction * MaxWebDistance;

            state.ShotDuration = Mathf.Clamp(
                    MaxWebDistance / WebTravelSpeed,
                    MinimumShotTime,
                    MaximumShotTime);

            state.HideTime =
                    Time.time          +
                    state.ShotDuration +
                    MissVisibleTime;
        }

        state.Line.enabled = true;
    }

    private void AttachWeb(WebState state)
    {
        state.IsAttached = true;

        Vector3 anchor = GetAnchorPoint(state);

        float distance = Vector3.Distance(
                state.Hand.position,
                anchor);

        state.RopeLength = Mathf.Max(
                MinimumRopeLength,
                distance - InitialRopeTension);

        Transform turnParent = GTPlayer.Instance.turnParent.transform;

        state.LastLocalHandPosition =
                turnParent.InverseTransformPoint(state.Hand.position);

        state.HandVelocity   = Vector3.zero;
        state.PullSpeed      = 0f;
        state.WobbleStrength = 0.055f;
    }

    private static void ReleaseWeb(WebState state)
    {
        if (state == null)
            return;

        state.IsFired         = false;
        state.HasHit          = false;
        state.IsAttached      = false;
        state.AnchorTransform = null;
        state.PullSpeed       = 0f;
        state.WobbleStrength  = 0f;

        if (state.Line != null)
            state.Line.enabled = false;
    }

    private static void UpdateHandMotion(WebState state)
    {
        if (state == null || state.Hand == null)
            return;

        Transform turnParent = GTPlayer.Instance.turnParent.transform;

        Vector3 localPosition =
                turnParent.InverseTransformPoint(state.Hand.position);

        float deltaTime = Mathf.Max(
                Time.fixedDeltaTime,
                0.001f);

        Vector3 localVelocity =
                (localPosition - state.LastLocalHandPosition) /
                deltaTime;

        state.LastLocalHandPosition = localPosition;

        Vector3 worldVelocity =
                turnParent.TransformVector(localVelocity);

        state.HandVelocity = Vector3.Lerp(
                state.HandVelocity,
                worldVelocity,
                0.65f);
    }

    private static void UpdatePull(WebState state, Rigidbody body)
    {
        if (state?.IsAttached != true)
            return;

        Vector3 anchor = GetAnchorPoint(state);

        Vector3 toAnchor =
                anchor - state.Hand.position;

        float distance = toAnchor.magnitude;

        if (distance < 0.001f)
            return;

        Vector3 ropeDirection =
                toAnchor / distance;

        float pullSpeed =
                -Vector3.Dot(
                        state.HandVelocity,
                        ropeDirection);

        pullSpeed = Mathf.Clamp(
                pullSpeed,
                0f,
                MaximumPullSpeed);

        float smoothing =
                1f -
                Mathf.Exp(
                        -18f *
                        Time.fixedDeltaTime);

        state.PullSpeed = Mathf.Lerp(
                state.PullSpeed,
                pullSpeed,
                smoothing);

        float activePull =
                state.PullSpeed -
                PullDeadZone;

        if (activePull <= 0f)
            return;

        state.RopeLength = Mathf.Max(
                MinimumRopeLength,
                state.RopeLength -
                activePull         *
                PullReelMultiplier *
                Time.fixedDeltaTime);

        body.AddForce(
                ropeDirection *
                activePull    *
                PullAcceleration,
                ForceMode.Acceleration);
    }

    private static void ApplyRopeConstraint(WebState state, Rigidbody body)
    {
        if (state?.IsAttached != true)
            return;

        Vector3 anchor = GetAnchorPoint(state);

        Vector3 toAnchor =
                anchor - state.Hand.position;

        float distance = toAnchor.magnitude;

        if (distance < 0.001f)
            return;

        Vector3 ropeDirection =
                toAnchor / distance;

        float stretch =
                distance -
                state.RopeLength;

        if (stretch < -RopeSlack)
            return;

        float tautness = Mathf.InverseLerp(
                -RopeSlack,
                0f,
                stretch);

        Vector3 endVelocity =
                body.linearVelocity +
                state.HandVelocity;

        float radialVelocity =
                Vector3.Dot(
                        endVelocity,
                        ropeDirection);

        if (radialVelocity < 0f)
        {
            body.linearVelocity +=
                    ropeDirection   *
                    -radialVelocity *
                    tautness;
        }

        if (stretch <= 0f)
            return;

        float correctionSpeed = Mathf.Min(
                stretch * RopeCorrectionSpeed,
                MaximumRopeCorrectionSpeed);

        correctionSpeed /= ConstraintIterations;

        body.linearVelocity +=
                ropeDirection *
                correctionSpeed;
    }

    private void ApplySwingAssist(Rigidbody body)
    {
        Vector3 ropeDirection = Vector3.zero;
        Vector3 handForward   = Vector3.zero;

        float pullAmount = 0f;

        int count = 0;

        AddSwingData(
                leftWeb,
                ref ropeDirection,
                ref handForward,
                ref pullAmount,
                ref count);

        AddSwingData(
                rightWeb,
                ref ropeDirection,
                ref handForward,
                ref pullAmount,
                ref count);

        if (count == 0)
            return;

        ropeDirection.Normalize();
        handForward.Normalize();

        Vector3 tangentVelocity =
                Vector3.ProjectOnPlane(
                        body.linearVelocity,
                        ropeDirection);

        if (tangentVelocity.sqrMagnitude  > 0.25f &&
            body.linearVelocity.magnitude < MaximumAssistedVelocity)
        {
            body.AddForce(
                    tangentVelocity.normalized *
                    SwingMaintainStrength,
                    ForceMode.Acceleration);
        }

        pullAmount /= count;

        if (pullAmount <= PullDeadZone)
            return;

        Transform head = GTPlayer.Instance.headCollider.transform;

        Vector3 wantedDirection =
                head.forward * 0.75f +
                handForward  * 0.25f;

        Vector3 swingDirection =
                Vector3.ProjectOnPlane(
                        wantedDirection,
                        ropeDirection);

        if (swingDirection.sqrMagnitude < 0.001f)
            return;

        swingDirection.Normalize();

        float pump =
                Mathf.Clamp(
                        pullAmount - PullDeadZone,
                        0f,
                        MaximumPullSpeed);

        float speedScale =
                Mathf.InverseLerp(
                        MaximumAssistedVelocity,
                        MaximumAssistedVelocity - 5f,
                        body.linearVelocity.magnitude);

        body.AddForce(
                swingDirection    *
                pump              *
                SwingPumpStrength *
                speedScale,
                ForceMode.Acceleration);
    }

    private static void AddSwingData(
            WebState    state,
            ref Vector3 ropeDirection,
            ref Vector3 handForward,
            ref float   pullAmount,
            ref int     count)
    {
        if (state?.IsAttached != true)
            return;

        Vector3 direction =
                GetAnchorPoint(state) -
                state.Hand.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        ropeDirection += direction.normalized;
        handForward   += state.Hand.forward;
        pullAmount    += state.PullSpeed;

        count++;
    }

    private void UpdateWebVisual(WebState state)
    {
        if (state == null || state.Line == null)
            return;

        if (!state.IsFired)
        {
            state.Line.enabled = false;

            return;
        }

        state.Line.enabled = true;

        Vector3 start = state.Muzzle.position;

        Vector3 target = state.HasHit
                                 ? GetAnchorPoint(state)
                                 : state.MissPoint;

        float progress = Mathf.Clamp01(
                (Time.time - state.ShotStartedAt) /
                Mathf.Max(state.ShotDuration, 0.001f));

        if (state.IsAttached)
            progress = 1f;

        float easedProgress =
                1f -
                Mathf.Pow(
                        1f - progress,
                        3f);

        Vector3 end =
                Vector3.Lerp(
                        start,
                        target,
                        easedProgress);

        Vector3 direction =
                end - start;

        float distance =
                direction.magnitude;

        if (distance < 0.001f)
            return;

        direction /= distance;

        Vector3 side =
                Vector3.Cross(
                        direction,
                        Vector3.up);

        if (side.sqrMagnitude < 0.001f)
            side = Vector3.Cross(direction, Vector3.right);

        side.Normalize();

        Vector3 up =
                Vector3.Cross(
                        direction,
                        side).normalized;

        float ropeDistance =
                state.IsAttached
                        ? Vector3.Distance(
                                state.Hand.position,
                                target)
                        : distance;

        float slack =
                state.IsAttached
                        ? Mathf.Max(
                                0f,
                                state.RopeLength - ropeDistance)
                        : 0f;

        float sag = Mathf.Clamp(
                distance * 0.012f +
                slack    * 0.35f,
                0f,
                0.28f);

        if (!state.IsAttached)
            sag *= progress;

        float speed =
                GorillaTagger.Instance.rigidbody.linearVelocity.magnitude;

        float targetWobble =
                state.IsAttached
                        ? 0.007f + Mathf.Clamp(speed * 0.002f, 0f, 0.03f)
                        : 0.045f;

        state.WobbleStrength = Mathf.Lerp(
                state.WobbleStrength,
                targetWobble,
                Time.deltaTime * 8f);

        Vector3 gravityDirection =
                Physics.gravity.sqrMagnitude > 0.001f
                        ? Physics.gravity.normalized
                        : Vector3.down;

        for (int i = 0; i < WebSegments; i++)
        {
            float t =
                    i /
                    (float)(WebSegments - 1);

            Vector3 point =
                    Vector3.Lerp(
                            start,
                            end,
                            t);

            float envelope =
                    Mathf.Sin(
                            t *
                            Mathf.PI);

            float wave =
                    Mathf.Sin(
                            t         * 17f -
                            Time.time * 13f +
                            (state.IsLeft ? 0f : 1.7f));

            float secondWave =
                    Mathf.Sin(
                            t         * 10f +
                            Time.time * 8f  +
                            (state.IsLeft ? 2f : 0f));

            point +=
                    side                 *
                    wave                 *
                    state.WobbleStrength *
                    envelope;

            point +=
                    up                   *
                    secondWave           *
                    state.WobbleStrength *
                    0.4f                 *
                    envelope;

            point +=
                    gravityDirection *
                    sag              *
                    envelope;

            state.Line.SetPosition(i, point);
        }
    }

    private void UpdateSwingRotation()
    {
        if (SpiderWalk.Active)
            return;

        bool leftAttached  = leftWeb?.IsAttached  == true;
        bool rightAttached = rightWeb?.IsAttached == true;

        if (!leftAttached && !rightAttached)
        {
            MakeUpright(false);

            return;
        }

        Rigidbody body = GorillaTagger.Instance.rigidbody;

        if (body.linearVelocity.magnitude < MinimumFlipSpeed)
            return;

        Transform turnParent =
                GTPlayer.Instance.turnParent.transform;

        Transform head =
                GTPlayer.Instance.headCollider.transform;

        Vector3 averageHand = Vector3.zero;

        int count = 0;

        if (leftAttached)
        {
            averageHand += leftWeb.Hand.position;
            count++;
        }

        if (rightAttached)
        {
            averageHand += rightWeb.Hand.position;
            count++;
        }

        if (count == 0)
            return;

        averageHand /= count;

        Vector3 handsFromHead =
                averageHand -
                head.position;

        if (handsFromHead.sqrMagnitude < 0.001f)
            return;

        handsFromHead.Normalize();

        Vector3 localLook =
                turnParent.InverseTransformDirection(
                        head.forward);

        Vector3 localHands =
                turnParent.InverseTransformDirection(
                        handsFromHead);

        float pitchInput = Mathf.Clamp(
                localLook.y  * 0.75f +
                localHands.y * 0.45f,
                -1f,
                1f);

        float rollInput = Mathf.Clamp(
                localHands.x * 0.65f,
                -1f,
                1f);

        float speedScale = Mathf.InverseLerp(
                MinimumFlipSpeed,
                8f,
                body.linearVelocity.magnitude);

        float pitchAngle =
                -pitchInput       *
                FlipRotationSpeed *
                speedScale        *
                Time.deltaTime;

        float rollAngle =
                -rollInput        *
                RollRotationSpeed *
                speedScale        *
                Time.deltaTime;

        Quaternion pitch =
                Quaternion.AngleAxis(
                        pitchAngle,
                        head.right);

        Quaternion roll =
                Quaternion.AngleAxis(
                        rollAngle,
                        head.forward);

        turnParent.rotation =
                roll  *
                pitch *
                turnParent.rotation;
    }

    private static void MakeUpright(bool immediate)
    {
        if (!GTPlayer.hasInstance ||
            GTPlayer.Instance.turnParent == null)
        {
            return;
        }

        Transform turnParent =
                GTPlayer.Instance.turnParent.transform;

        Transform head =
                GTPlayer.Instance.headCollider.transform;

        Vector3 forward =
                Vector3.ProjectOnPlane(
                        turnParent.forward,
                        Vector3.up);

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(
                    head.forward,
                    Vector3.up);
        }

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        Quaternion upright =
                Quaternion.LookRotation(
                        forward.normalized,
                        Vector3.up);

        if (immediate)
        {
            turnParent.rotation = upright;

            return;
        }

        float amount =
                1f -
                Mathf.Exp(
                        -UprightReturnSpeed *
                        Time.deltaTime);

        turnParent.rotation =
                Quaternion.Slerp(
                        turnParent.rotation,
                        upright,
                        amount);
    }

    private static Vector3 GetAnchorPoint(WebState state)
    {
        if (state.AnchorTransform != null)
        {
            return state.AnchorTransform.TransformPoint(
                    state.AnchorLocalPoint);
        }

        return state.StaticAnchor;
    }

    private static void DestroyWebState(WebState state)
    {
        if (state == null)
            return;

        if (state.Line?.material != null)
            Object.Destroy(state.Line.material);

        if (state.WebObject != null)
            state.WebObject.Obliterate();

        if (state.Shooter != null)
            state.Shooter.gameObject.Obliterate();
    }

    private sealed class WebState
    {
        public Vector3   AnchorLocalPoint;
        public Transform AnchorTransform;

        public float       AttachTime;
        public AudioSource AudioSource;

        public Transform Hand;
        public Vector3   HandVelocity;
        public bool      HasHit;
        public float     HideTime;
        public bool      IsAttached;
        public bool      IsFired;
        public bool      IsLeft;
        public Vector3   LastLocalHandPosition;

        public LineRenderer Line;
        public Vector3      MissPoint;
        public Transform    Muzzle;
        public float        PullSpeed;
        public float        RopeLength;
        public Transform    Shooter;
        public float        ShotDuration;
        public float        ShotStartedAt;

        public Vector3 StaticAnchor;
        public bool    WasPressed;

        public GameObject WebObject;
        public float      WobbleStrength;
    }
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class WebShooterRigPatch
{
    private static void Postfix(VRRig __instance)
    {
        if (!WebShooter.Active       ||
            SpiderWalk.Active        ||
            !__instance.IsLocalRig() ||
            !GTPlayer.hasInstance)
        {
            return;
        }

        Transform turnParent =
                GTPlayer.Instance.turnParent.transform;

        Vector3 up =
                turnParent.up;

        if (Vector3.Dot(up, Vector3.up) > 0.9999f)
            return;

        Quaternion tilt =
                Quaternion.FromToRotation(
                        Vector3.up,
                        up);

        __instance.transform.rotation =
                tilt *
                __instance.transform.rotation;

        __instance.leftHand.MapMine(
                __instance.scaleFactor,
                __instance.playerOffsetTransform);

        __instance.rightHand.MapMine(
                __instance.scaleFactor,
                __instance.playerOffsetTransform);

        __instance.head.MapMine(
                __instance.scaleFactor,
                __instance.playerOffsetTransform);

        __instance.head.rigTarget.rotation =
                GTPlayer.Instance.headCollider.transform.rotation;
    }
}