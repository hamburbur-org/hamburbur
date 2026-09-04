using System.Collections.Generic;
using GorillaLocomotion;
using GorillaTagScripts;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Blocks;

[hamburburmod(                "Select Block Gun",   "Aim at a Monke Block and press trigger to select its type",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class SelectBuilderBlockGun : hamburburmod
{
    private readonly GunLib gun = new();
    private          bool   previousTrigger;

    protected override void Start() => gun.Start();

    protected override void LateUpdate()
    {
        gun.LateUpdate();

        if (gun.IsShooting && !previousTrigger)
        {
            BuilderPiece piece = gun.Hit.collider?.GetComponentInParent<BuilderPiece>();
            if (piece != null)
                BuilderBlockLib.TrySelectPiece(piece);
        }

        previousTrigger = gun.IsShooting;
    }

    protected override void OnDisable()
    {
        gun.OnDisable();
        previousTrigger = false;
    }
}

[hamburburmod(                "Place Block Gun",    "Aim and press trigger to place the selected block on a surface",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class PlaceBuilderBlockGun : hamburburmod
{
    private readonly GunLib gun = new();
    private          float  nextPlacement;

    protected override void Start() => gun.Start();

    protected override void LateUpdate()
    {
        gun.LateUpdate();

        if (!gun.IsShooting || gun.Hit.collider == null || Time.time < nextPlacement)
            return;

        nextPlacement = Time.time + 0.16f;
        Vector3 normal = gun.Hit.normal.sqrMagnitude > 0f ? gun.Hit.normal.normalized : Vector3.up;

        if (!BuilderBlockLib.TrySpawn(
                    gun.Hit.point + normal * 0.055f,
                    Quaternion.FromToRotation(Vector3.up, normal)))
            BuilderBlockLib.NotifyUnavailable();
    }

    protected override void OnDisable() => gun.OnDisable();
}

[hamburburmod(                "Random Block Type",  "Randomise the block type for every creation", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class RandomBuilderBlockType : hamburburmod
{
    protected override void OnEnable()  => BuilderBlockLib.UseRandomPieceType = true;
    protected override void OnDisable() => BuilderBlockLib.UseRandomPieceType = false;
}

public abstract class BlockPatternGun : hamburburmod
{
    private readonly GunLib gun = new();
    private          bool   previousTrigger;

    protected override void Start() => gun.Start();

    protected override void LateUpdate()
    {
        gun.LateUpdate();

        if (gun.IsShooting && !previousTrigger && gun.Hit.collider != null)
        {
            if (!BuilderBlockLib.TryGetTable(out BuilderTable _))
                BuilderBlockLib.NotifyUnavailable();
            else
                CoroutineManager.Instance.StartCoroutine(
                        BuilderBlockLib.SpawnSequence(CreatePattern(gun.Hit), 0.065f));
        }

        previousTrigger = gun.IsShooting;
    }

    protected abstract IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(RaycastHit hit);

    protected override void OnDisable()
    {
        gun.OnDisable();
        previousTrigger = false;
    }

    protected static void GetSurfaceBasis(RaycastHit hit, out Vector3 right, out Vector3 up, out Vector3 normal)
    {
        normal = hit.normal.sqrMagnitude > 0f ? hit.normal.normalized : Vector3.up;
        Transform hand = GunLib.GetGunHand();

        right = Vector3.ProjectOnPlane(hand != null ? hand.right : Vector3.right, normal).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(normal, Vector3.forward).normalized;

        if (right.sqrMagnitude < 0.01f)
            right = Vector3.right;

        up = Vector3.Cross(normal, right).normalized;
    }
}

[hamburburmod(                "Block Wall Gun",     "Press trigger to build a 5 by 4 wall at the target",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockWallGun : BlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(RaycastHit hit)
    {
        GetSurfaceBasis(hit, out Vector3 right, out Vector3 up, out Vector3 normal);
        Quaternion rotation = Quaternion.LookRotation(normal, up);

        for (int y = 0; y < 4; y++)
            for (int x = -2; x <= 2; x++)
                yield return new BuilderBlockLib.BlockShot(
                        hit.point + normal * 0.06f + right * (x * 0.18f) + up * (y * 0.18f),
                        rotation,
                        Vector3.zero,
                        Vector3.zero);
    }
}

[hamburburmod(                "Block Bridge Gun",   "Press trigger to lay a block bridge toward the target",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockBridgeGun : BlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(RaycastHit hit)
    {
        Vector3 start = GTPlayer.Instance.bodyCollider.bounds.min + Vector3.up * 0.04f;
        Vector3 delta = hit.point                                 - start;
        int     count = Mathf.Clamp(Mathf.CeilToInt(delta.magnitude / 0.22f), 1, 28);
        Quaternion rotation = delta.sqrMagnitude > 0f
                                      ? Quaternion.LookRotation(delta.normalized, Vector3.up)
                                      : Quaternion.identity;

        for (int i = 1; i <= count; i++)
            yield return new BuilderBlockLib.BlockShot(
                    Vector3.Lerp(start, hit.point, i / (float)count),
                    rotation,
                    Vector3.zero,
                    Vector3.zero);
    }
}

[hamburburmod(                "Block Stairs Gun",   "Press trigger to build a rising staircase toward the target",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockStairsGun : BlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(RaycastHit hit)
    {
        Vector3 start         = GTPlayer.Instance.bodyCollider.bounds.min + Vector3.up * 0.04f;
        Vector3 flatDirection = Vector3.ProjectOnPlane(hit.point - start, Vector3.up).normalized;
        if (flatDirection.sqrMagnitude < 0.01f)
            flatDirection = GunLib.GetGunHand().forward;

        Quaternion rotation = Quaternion.LookRotation(flatDirection, Vector3.up);

        for (int i = 1; i <= 16; i++)
            yield return new BuilderBlockLib.BlockShot(
                    start + flatDirection * (i * 0.20f) + Vector3.up * (i * 0.09f),
                    rotation,
                    Vector3.zero,
                    Vector3.zero);
    }
}

[hamburburmod(                "Block Wave Gun",     "Press trigger to draw a sine wave of blocks on the target surface",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockWaveGun : BlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(RaycastHit hit)
    {
        GetSurfaceBasis(hit, out Vector3 right, out Vector3 up, out Vector3 normal);
        Quaternion rotation = Quaternion.LookRotation(normal, up);

        for (int i = -10; i <= 10; i++)
        {
            float wave = Mathf.Sin(i * 0.8f) * 0.32f;

            yield return new BuilderBlockLib.BlockShot(
                    hit.point + normal * 0.06f + right * (i * 0.15f) + up * wave,
                    rotation,
                    Vector3.zero,
                    Vector3.zero);
        }
    }
}

public abstract class TimedBlockEffect : hamburburmod
{
    private float nextEmission;

    protected abstract float                     Interval { get; }
    protected abstract BuilderBlockLib.BlockShot CreateShot();

    protected override void LateUpdate()
    {
        if (Time.time < nextEmission)
            return;

        nextEmission = Time.time + Interval;
        BuilderBlockLib.BlockShot shot = CreateShot();
        if (!BuilderBlockLib.TrySpawn(shot.Position, shot.Rotation, shot.Velocity, shot.AngularVelocity))
            BuilderBlockLib.NotifyUnavailable();
    }
}

[hamburburmod(                "Block Rain",         "Rains selected blocks around you", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockRain : TimedBlockEffect
{
    protected override float Interval => 0.22f;

    protected override BuilderBlockLib.BlockShot CreateShot()
    {
        Vector3 center   = GorillaTagger.Instance.headCollider.transform.position;
        Vector3 position = center + BuilderBlockLib.RandomHorizontal(2.4f) + Vector3.up * 3.4f;

        return new BuilderBlockLib.BlockShot(position, Random.rotation, Vector3.down * 3f,
                Random.insideUnitSphere                                              * 3f);
    }
}

[hamburburmod(                "Block Fountain",     "Launches a kinetic fountain of selected blocks",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockFountain : TimedBlockEffect
{
    protected override float Interval => 0.16f;

    protected override BuilderBlockLib.BlockShot CreateShot()
    {
        Vector3 position = GTPlayer.Instance.bodyCollider.bounds.min + Vector3.up * 0.15f;
        Vector3 velocity = Vector3.up                                             * Random.Range(7f, 11f) + BuilderBlockLib.RandomHorizontal(3.5f);

        return new BuilderBlockLib.BlockShot(position, Random.rotation, velocity,
                Random.insideUnitSphere * 8f);
    }
}

[hamburburmod(                "Block Aura",         "Continuously pops blocks outward around your body", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockAura : TimedBlockEffect
{
    protected override float Interval => 0.20f;

    protected override BuilderBlockLib.BlockShot CreateShot()
    {
        Vector3 center    = GTPlayer.Instance.bodyCollider.bounds.center;
        Vector3 direction = Random.onUnitSphere;
        direction.y = Mathf.Abs(direction.y) * 0.4f;
        direction.Normalize();

        return new BuilderBlockLib.BlockShot(
                center + direction * 0.65f,
                Random.rotation,
                direction               * 4f,
                Random.insideUnitSphere * 5f);
    }
}

[hamburburmod(                "Block Trail",        "Leaves a trail of selected blocks as you move", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockTrail : hamburburmod
{
    private Vector3 lastPosition;
    private float   nextBlock;

    protected override void OnEnable() => lastPosition = GTPlayer.Instance.transform.position;

    protected override void LateUpdate()
    {
        Vector3 current = GTPlayer.Instance.transform.position;

        if (Time.time < nextBlock || Vector3.Distance(current, lastPosition) < 0.18f)
            return;

        nextBlock    = Time.time + 0.14f;
        lastPosition = current;

        Vector3 position = GTPlayer.Instance.bodyCollider.bounds.min + Vector3.up * 0.03f;
        if (!BuilderBlockLib.TrySpawn(position, Quaternion.identity))
            BuilderBlockLib.NotifyUnavailable();
    }
}

[hamburburmod(                "Block Firework",     "Launch a spiral burst of blocks from your hand", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BlockFirework : hamburburmod
{
    protected override void Pressed()
    {
        if (!BuilderBlockLib.TryGetTable(out BuilderTable _))
        {
            BuilderBlockLib.NotifyUnavailable();

            return;
        }

        Transform                       hand  = GunLib.GetGunHand();
        List<BuilderBlockLib.BlockShot> shots = [];
        for (int i = 0; i < 14; i++)
        {
            float angle = i / 14f * Mathf.PI * 2f;
            Vector3 velocity = Vector3.up                                          * Random.Range(8f, 12f) +
                               new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 3.5f;

            shots.Add(new BuilderBlockLib.BlockShot(hand.position, Random.rotation, velocity,
                    Random.insideUnitSphere * 7f));
        }

        CoroutineManager.Instance.StartCoroutine(BuilderBlockLib.SpawnSequence(shots, 0.065f));
    }
}

[hamburburmod(                "Build Block Ring",   "Build a ring of blocks around you", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BuildBlockRing : hamburburmod
{
    protected override void Pressed()
    {
        if (!BuilderBlockLib.TryGetTable(out BuilderTable _))
        {
            BuilderBlockLib.NotifyUnavailable();

            return;
        }

        Vector3                         center = GTPlayer.Instance.bodyCollider.bounds.center;
        List<BuilderBlockLib.BlockShot> shots  = [];
        for (int i = 0; i < 20; i++)
        {
            float   angle  = i / 20f * Mathf.PI * 2f;
            Vector3 radial = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            shots.Add(new BuilderBlockLib.BlockShot(
                    center + radial * 1.4f,
                    Quaternion.LookRotation(radial, Vector3.up),
                    Vector3.zero,
                    Vector3.zero));
        }

        CoroutineManager.Instance.StartCoroutine(BuilderBlockLib.SpawnSequence(shots, 0.065f));
    }
}

[hamburburmod(                "Build Block Spiral", "Build a rising spiral of blocks around you", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public sealed class BuildBlockSpiral : hamburburmod
{
    protected override void Pressed()
    {
        if (!BuilderBlockLib.TryGetTable(out BuilderTable _))
        {
            BuilderBlockLib.NotifyUnavailable();

            return;
        }

        Vector3                         center = GTPlayer.Instance.bodyCollider.bounds.min;
        List<BuilderBlockLib.BlockShot> shots  = [];
        for (int i = 0; i < 26; i++)
        {
            float   angle  = i * 0.65f;
            Vector3 radial = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            shots.Add(new BuilderBlockLib.BlockShot(
                    center + radial * 1.1f + Vector3.up * (i * 0.09f),
                    Quaternion.LookRotation(radial, Vector3.up),
                    Vector3.zero,
                    Vector3.zero));
        }

        CoroutineManager.Instance.StartCoroutine(BuilderBlockLib.SpawnSequence(shots, 0.065f));
    }
}

[hamburburmod(                "Destroy Nearby Blocks", "Continuously recycle or remove all eligible blocks around you",
        ButtonType.Togglable, AccessSetting.Public,    EnabledType.Disabled, 0)]
public sealed class CleanupNearbyBuilderBlocks : hamburburmod
{
    private float nextAttempt;

    protected override void LateUpdate()
    {
        if (Time.time < nextAttempt)
            return;

        nextAttempt = Time.time + 0.08f;
        BuilderBlockLib.TryRecycleNearbyBlock();
    }
}