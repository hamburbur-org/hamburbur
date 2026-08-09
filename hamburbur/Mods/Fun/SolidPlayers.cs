using System;
using System.Collections.Generic;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hamburbur.Mods.Fun;

[hamburburmod(
        "Solid Players",
        "Makes everyone solid",
        ButtonType.Togglable,
        AccessSetting.Public,
        EnabledType.Disabled,
        0)]
public class SolidPlayers : hamburburmod
{
    protected override void OnEnable()
    {
        RigUtils.OnRigLoaded += MakeSolid;
        RigUtils.OnRigUnloaded += RevertSolidity;

        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (VRRig rig in NetworkSystem.Instance.Rigs())
            MakeSolid(rig);
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded   -= MakeSolid;
        RigUtils.OnRigUnloaded -= RevertSolidity;

        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (VRRig rig in NetworkSystem.Instance.Rigs())
            RevertSolidity(rig);
    }

    private static void MakeSolid(VRRig rig)
    {
        if (rig == null || rig.IsLocalRig())
            return;

        if (rig.GetComponent<SolidPlayerCollider>() != null)
            return;

        rig.gameObject.AddComponent<SolidPlayerCollider>();
    }

    private static void RevertSolidity(VRRig rig)
    {
        if (rig == null)
            return;

        SolidPlayerCollider solidCollider =
                rig.GetComponent<SolidPlayerCollider>();

        if (solidCollider != null)
            Object.Destroy(solidCollider);
    }
}

internal class SolidPlayerCollider : MonoBehaviour
{
    private const int CollisionLayer = 0;

    private const float HeadRadius      = 0.16f;
    private const float MinimumRadius   = 0.045f;
    private const float MaximumRadius   = 0.085f;
    private const float RadiusScale     = 0.22f;
    private const float MinimumBoneSize = 0.01f;

    private static readonly int[] Bones =
    [
            4, 3,
            5, 4,

            19, 18,
            20, 19,

            3, 18,

            21, 20,
            22, 21,

            25, 21,
            29, 21,

            31, 29,
            27, 25,
            24, 22,

            6, 5,
            7, 6,

            10, 6,
            14, 6,

            16, 14,
            12, 10,

            9, 7,
    ];

    private readonly List<CapsuleLink> capsuleLinks = new();
    private          Rigidbody         colliderRigidbody;

    private GameObject colliderRoot;

    private SphereCollider headCollider;

    private PhysicsMaterial physicsMaterial;

    private VRRig rig;

    private void Awake()
    {
        rig = GetComponent<VRRig>();

        if (rig == null)
        {
            Destroy(this);

            return;
        }

        CreatePhysicsMaterial();
        CreateColliderRoot();
        CreateHeadCollider();
        CreateBoneColliders();

        UpdateColliders();
    }

    private void FixedUpdate()
    {
        if (rig                == null ||
            rig.mainSkin       == null ||
            rig.mainSkin.bones == null)
            return;

        UpdateColliders();
    }

    private void OnDestroy()
    {
        capsuleLinks.Clear();

        if (colliderRoot != null)
            Destroy(colliderRoot);

        if (physicsMaterial != null)
            Destroy(physicsMaterial);
    }

    private void CreatePhysicsMaterial()
    {
        physicsMaterial = new PhysicsMaterial("HamburburSolidPlayer")
        {
                dynamicFriction = 0f,
                staticFriction  = 0f,
                bounciness      = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine   = PhysicsMaterialCombine.Minimum,
        };
    }

    private void CreateColliderRoot()
    {
        colliderRoot = new GameObject("HamburburSolidPlayerColliders")
        {
                layer     = CollisionLayer,
                transform =
                {
                        position = rig.transform.position,
                },
        };

        colliderRigidbody = colliderRoot.AddComponent<Rigidbody>();

        colliderRigidbody.useGravity       = false;
        colliderRigidbody.isKinematic      = true;
        colliderRigidbody.detectCollisions = true;
        colliderRigidbody.interpolation    = RigidbodyInterpolation.Interpolate;
        colliderRigidbody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;
    }

    private void CreateHeadCollider()
    {
        GameObject headObject = new("HeadCollider")
        {
                layer = CollisionLayer,
        };

        headObject.transform.SetParent(colliderRoot.transform);

        headCollider = headObject.AddComponent<SphereCollider>();

        headCollider.radius         = HeadRadius;
        headCollider.isTrigger      = false;
        headCollider.sharedMaterial = physicsMaterial;
    }

    private void CreateBoneColliders()
    {
        Transform[] rigBones = rig.mainSkin.bones;

        if (rigBones == null)
            return;

        for (int i = 0; i < Bones.Length; i += 2)
        {
            int startIndex = Bones[i];
            int endIndex   = Bones[i + 1];

            if (startIndex < 0                ||
                endIndex   < 0                ||
                startIndex >= rigBones.Length ||
                endIndex   >= rigBones.Length)
                continue;

            Transform startBone = rigBones[startIndex];
            Transform endBone   = rigBones[endIndex];

            if (startBone == null || endBone == null)
                continue;

            GameObject capsuleObject = new($"BoneCollider_{startIndex}_{endIndex}")
            {
                    layer = CollisionLayer,
            };

            capsuleObject.transform.SetParent(colliderRoot.transform);

            CapsuleCollider capsule =
                    capsuleObject.AddComponent<CapsuleCollider>();

            capsule.direction      = 1;
            capsule.isTrigger      = false;
            capsule.sharedMaterial = physicsMaterial;

            capsuleLinks.Add(new CapsuleLink
            {
                    Start     = startBone,
                    End       = endBone,
                    Transform = capsuleObject.transform,
                    Collider  = capsule,
            });
        }
    }

    private void UpdateColliders()
    {
        if (colliderRoot == null)
            return;

        colliderRoot.transform.position = rig.transform.position;
        colliderRoot.transform.rotation = Quaternion.identity;

        UpdateHeadCollider();

        foreach (CapsuleLink link in capsuleLinks)
            UpdateCapsule(link);
    }

    private void UpdateHeadCollider()
    {
        if (headCollider       == null ||
            rig.head           == null ||
            rig.head.rigTarget == null)
            return;

        Transform headTransform = headCollider.transform;

        headTransform.position = rig.head.rigTarget.position;
        headTransform.rotation = Quaternion.identity;
    }

    private static void UpdateCapsule(CapsuleLink link)
    {
        if (link.Start    == null ||
            link.End      == null ||
            link.Collider == null)
            return;

        Vector3 start = link.Start.position;
        Vector3 end   = link.End.position;

        Vector3 difference = end - start;
        float   distance   = difference.magnitude;

        if (distance < MinimumBoneSize)
        {
            link.Collider.enabled = false;

            return;
        }

        link.Collider.enabled = true;

        Vector3 midpoint = (start + end) * 0.5f;

        link.Transform.position = midpoint;
        link.Transform.rotation =
                Quaternion.FromToRotation(Vector3.up, difference.normalized);

        float radius = Mathf.Clamp(
                distance * RadiusScale,
                MinimumRadius,
                MaximumRadius);

        link.Collider.radius = radius;

        link.Collider.height = Mathf.Max(
                distance + radius * 2f,
                radius * 2f);
    }

    private class CapsuleLink
    {
        public CapsuleCollider Collider;
        public Transform       End;
        public Transform       Start;

        public Transform Transform;
    }
}