using System.Collections.Generic;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
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
}

public class GunLib
{
    private const           int     ConstraintIterations = 5;
    private const           int     NumPoints            = 50;
    public static           GunType GunType              = GunType.Straight;
    private static readonly float   Gravity              = Physics.gravity.magnitude;

    private static readonly Dictionary<LineRenderer, (Vector3[] previousPoints, Vector3[] currentPoints)> PointsDict =
            [];

    public VRRig ChosenRig;

    private LineRenderer gunLine;

    public RaycastHit Hit;
    public bool       IsShooting;
    public bool       ShouldFollow;

    public void Start()
    {
        gunLine               = new GameObject("GunLine").AddComponent<LineRenderer>();
        gunLine.positionCount = NumPoints;
        gunLine.useWorldSpace = true;
        gunLine.material      = new Material(Shader.Find("GUI/Text Shader"));
        gunLine.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        if (gunLine != null)
            gunLine.gameObject.SetActive(false);
    }

    public void LateUpdate()
    {
        if (InputManager.Instance.RightGrip.IsPressed)
        {
            Transform realRightController = Tools.Utils.RealRightController;

            Vector3 gunPosition  = realRightController.position;
            Vector3 gunDirection = realRightController.forward;

            HandleShooting(new Ray(gunPosition, gunDirection), InputManager.Instance.RightTrigger.IsPressed,
                    gunPosition);
        }
        else if (Mouse.current.backButton.isPressed)
        {
            Camera cameraToUse = Tools.Utils.GetActiveCamera();
            Ray    ray         = cameraToUse.ScreenPointToRay(Mouse.current.position.ReadValue());

            HandleShooting(ray, Mouse.current.leftButton.isPressed, GTPlayer.Instance.bodyCollider.transform.position);
        }
        else
        {
            gunLine.gameObject.SetActive(false);
            ChosenRig = null;
        }
    }

    private void HandleShooting(Ray ray, bool shooting, Vector3 fakeOrigin)
    {
        IsShooting = shooting;

        if (PhysicsRaycast(ray, VRRig.LocalRig, ref ChosenRig, out Hit, out ChosenRig))
        {
            gunLine.gameObject.SetActive(true);

            float time = Mathf.PingPong(Time.time, 1f);
            gunLine.material.color = Color.Lerp(Plugin.Instance.MainColour, Plugin.Instance.SecondaryColour, time);

            float scale = 0.0125f * GTPlayer.Instance.scale;
            gunLine.startWidth = scale;
            gunLine.endWidth   = scale;

            Vector3 targetEndPos = Hit.point;

            switch (IsShooting)
            {
                case true when ShouldFollow && ChosenRig != null:
                    targetEndPos = ChosenRig.transform.position;

                    break;

                case false:
                    ChosenRig = null;

                    break;
            }

            HandleShootingVisuals(fakeOrigin, targetEndPos, IsShooting || AlwaysAnimateGun.IsEnabled, gunLine);
        }
        else
        {
            gunLine.gameObject.SetActive(false);
        }
    }

    private static void HandleShootingVisuals(Vector3 origin, Vector3 end, bool doSpecial, LineRenderer lineToImpact)
    {
        if (!PointsDict.ContainsKey(lineToImpact))
        {
            PointsDict[lineToImpact] = (previousPoints: new Vector3[NumPoints], currentPoints: new Vector3[NumPoints]);
            for (int i = 0; i < NumPoints; i++)
                PointsDict[lineToImpact].previousPoints[i] = PointsDict[lineToImpact].currentPoints[i] = Vector3.zero;
        }

        if (!doSpecial)
            for (int i = 0; i < NumPoints; i++)
            {
                float t = i / (float)(NumPoints - 1);
                PointsDict[lineToImpact].currentPoints[i]  = Vector3.Lerp(origin, end, t);
                PointsDict[lineToImpact].previousPoints[i] = PointsDict[lineToImpact].currentPoints[i];
            }
        else
            switch (GunType)
            {
                case GunType.Rope:
                {
                    PointsDict[lineToImpact].currentPoints[0]             = origin;
                    PointsDict[lineToImpact].currentPoints[NumPoints - 1] = end;

                    for (int i = 1; i < NumPoints - 1; i++)
                    {
                        Vector3 velocity = (PointsDict[lineToImpact].currentPoints[i] -
                                            PointsDict[lineToImpact].previousPoints[i]) / Time.deltaTime;

                        PointsDict[lineToImpact].previousPoints[i] = PointsDict[lineToImpact].currentPoints[i];

                        PointsDict[lineToImpact].currentPoints[i] += velocity;
                        PointsDict[lineToImpact].currentPoints[i] +=
                                Vector3.down * (Gravity * Time.deltaTime * Time.deltaTime);
                    }

                    for (int iter = 0; iter < ConstraintIterations; iter++)
                    {
                        for (int i = 0; i < NumPoints - 1; i++)
                        {
                            Vector3 delta = PointsDict[lineToImpact].currentPoints[i + 1] -
                                            PointsDict[lineToImpact].currentPoints[i];

                            float   dist       = delta.magnitude;
                            float   targetDist = Vector3.Distance(origin, end) / (NumPoints - 1);
                            Vector3 correction = delta.normalized              * ((dist - targetDist) * 0.5f);

                            if (i != 0) PointsDict[lineToImpact].currentPoints[i]                 += correction;
                            if (i != NumPoints - 2) PointsDict[lineToImpact].currentPoints[i + 1] -= correction;
                        }
                    }

                    break;
                }

                case GunType.Static:
                {
                    PointsDict[lineToImpact].currentPoints[0]             = origin;
                    PointsDict[lineToImpact].currentPoints[NumPoints - 1] = end;

                    float   distance   = Vector3.Distance(origin, end);
                    Vector3 lastStatic = origin;

                    for (int i = 0; i < NumPoints; i++)
                    {
                        PointsDict[lineToImpact].previousPoints[i] = PointsDict[lineToImpact].currentPoints[i];
                        PointsDict[lineToImpact].currentPoints[i] =
                                Vector3.Lerp(origin, end, i / (float)(NumPoints - 1));

                        if (Vector3.Distance(lastStatic, PointsDict[lineToImpact].currentPoints[i]) > distance * 0.05f)
                            PointsDict[lineToImpact].currentPoints[i] += new Vector3(
                                    Random.Range(-1f * distance * 0.05f, distance * 0.05f),
                                    Random.Range(-1f * distance * 0.05f, distance * 0.05f),
                                    Random.Range(-1f * distance * 0.05f, distance * 0.05f));
                    }

                    break;
                }

                case GunType.Bezier:
                {
                    Vector3 midPosition = Vector3.zero;
                    Vector3 midVelocity = Vector3.zero;

                    Vector3 baseMid = Vector3.Lerp(origin, end, 0.5f);

                    Transform controller = Tools.Utils.RealRightController;
                    Vector3   up         = controller.up;
                    Vector3   right      = controller.right;

                    float angle = Time.time * 3f;
                    Vector3 wobbleOffset =
                            up    * (Mathf.Sin(angle)        * 0.15f) +
                            right * (Mathf.Cos(angle * 1.3f) * 0.15f);

                    Vector3 targetMid = baseMid + wobbleOffset;

                    if (midPosition == Vector3.zero)
                        midPosition = targetMid;

                    Vector3 force = (targetMid - midPosition) * 40f;
                    midVelocity += force * Time.deltaTime;
                    midVelocity *= Mathf.Exp(-6f * Time.deltaTime);
                    midPosition += midVelocity * Time.deltaTime;

                    for (int i = 0; i < NumPoints; i++)
                    {
                        float t   = i / (float)(NumPoints - 1);
                        float omt = 1f - t;

                        PointsDict[lineToImpact].previousPoints[i] = PointsDict[lineToImpact].currentPoints[i];
                        PointsDict[lineToImpact].currentPoints[i] =
                                omt * omt * origin          +
                                2f  * omt * t * midPosition +
                                t   * t   * end;
                    }

                    break;
                }

                case GunType.Straight:
                default:
                {
                    for (int i = 0; i < NumPoints; i++)
                    {
                        PointsDict[lineToImpact].previousPoints[i] = PointsDict[lineToImpact].currentPoints[i];
                        PointsDict[lineToImpact].currentPoints[i] =
                                Vector3.Lerp(origin, end, i / (float)(NumPoints - 1));
                    }

                    break;
                }
            }

        lineToImpact.SetPositions(PointsDict[lineToImpact].currentPoints);
    }

    private static bool PhysicsRaycast(Ray ray, VRRig toIgnore, ref VRRig chosenRig, out RaycastHit hit,
                                       [CanBeNull] out VRRig rig)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        hit = default(RaycastHit);
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit2 in hits)
            if ((1 << hit2.collider.gameObject.layer & GTPlayer.Instance.locomotionEnabledLayers) != 0
             || hit2.collider.GetComponentInParent<VRRig>() != null &&
                hit2.collider.GetComponentInParent<VRRig>() != toIgnore)
                if (hit2.distance < minDistance)
                {
                    minDistance = hit2.distance;
                    hit         = hit2;
                }

        rig = chosenRig == null ? hit.collider?.GetComponentInParent<VRRig>() : chosenRig;

        return hit.collider != null;
    }
}