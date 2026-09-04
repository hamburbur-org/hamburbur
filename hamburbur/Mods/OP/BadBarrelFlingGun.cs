using System.Collections;
using ExitGames.Client.Photon;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.OP;

// R.I.P Barrel Mods 21/08/2026 - They removed the tiny collider :sob:

[hamburburmod("Barrel Fling Gun", "Fling people with the barrel cosmetic", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class BadBarrelFlingGun : hamburburmod
{
    private const int BarrelIndex = 618;

    private const float BurstCount = 1f;
    private const float BurstDelay = 0.01f;
    private const float FireRate   = 0.02f;

    private const float FlingForce    = 5382.4f;
    private const float ForceVariance = 0f;

    private const float OffsetX      = 0f;
    private const float OffsetY      = -3.47f;
    private const float OffsetZ      = 0f;

    private const float UpwardsBias = 0f;

    private readonly GunLib    gunLib = new() { ShouldFollow = true, };
    private          Coroutine cleanupCoroutine;
    private          Coroutine flingCoroutine;
    
    private object activeNotification;

    protected override void Start() => gunLib.Start();

    protected override void OnEnable() =>
            NotificationManager.SendNotification("<color=red>Op</color>",
                    "Make sure you own and have the barrel cosmetic equipped", 4f, false, false);

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null)
        {
            StopFling();

            return;
        }

        flingCoroutine ??= CoroutineManager.Instance.StartCoroutine(FlingLoop());
    }
    
    private void UpdateDistanceNotification(VRRig rig)
    {
        if (rig == null || VRRig.LocalRig == null) return;

        float distance = Vector3.Distance(
                GorillaTagger.Instance.bodyCollider.transform.position,
                rig.transform.position
        );

        if (activeNotification == null)
        {
            activeNotification = NotificationManager.SendNotification(
                    "<color=red>Barrel Fling</color>",
                    $"Target distance: {distance.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}m",
                    9999f,
                    false,
                    false
            );
        }
        else
        {
            NotificationManager.UpdateNotificationEntry(activeNotification, "<color=red>Barrel Fling</color>",
                    $"Target distance: {distance.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}m",
                    9999f);
        }
    }

    private IEnumerator FlingLoop()
    {
        while (gunLib.IsShooting && gunLib.ChosenRig != null)
        {
            yield return CoroutineManager.Instance.StartCoroutine(FireBurst(gunLib.ChosenRig));
            yield return new WaitForSeconds(FireRate);
        }

        flingCoroutine = null;
    }

    private IEnumerator FireBurst(VRRig rig)
    {
        int shots = Mathf.RoundToInt(BurstCount);
        for (int i = 0; i < shots; i++)
        {
            if (rig == null) yield break;
            
            UpdateDistanceNotification(rig);

            Vector3 targetPos = rig.transform.position;
            Vector3 spawnPos  = targetPos + new Vector3(OffsetX, OffsetY, OffsetZ);

            Vector3 direction = (targetPos - spawnPos).normalized + Vector3.up * UpwardsBias;
            direction = direction.normalized;

            float   actualForce = FlingForce + Random.Range(-ForceVariance, ForceVariance);
            Vector3 velocity    = direction * actualForce;

            FireBarrel(spawnPos, velocity, Quaternion.identity);

            if (i < shots - 1)
                yield return new WaitForSeconds(BurstDelay);
        }
    }

    private void FireBarrel(Vector3 pos, Vector3 vel, Quaternion rot)
    {
        TransferrableObject barrel = VRRig.LocalRig.myBodyDockPositions.allObjects[BarrelIndex];

        if (!barrel.gameObject.activeSelf)
        {
            VRRig.LocalRig.SetActiveTransferrableObjectIndex(1, BarrelIndex);
            barrel.gameObject.SetActive(true);
        }

        barrel.storedZone   = BodyDockPositions.DropPositions.RightArm;
        barrel.currentState = TransferrableObject.PositionState.InRightHand;

        DeployableObject deployable = barrel.GetComponent<DeployableObject>();

        deployable._child.Deploy(deployable, pos, rot, vel);
        deployable.DeployChild();
        
        Tools.Utils.RPCProtection();

        if (cleanupCoroutine != null)
            CoroutineManager.Instance.StopCoroutine(cleanupCoroutine);

        cleanupCoroutine = CoroutineManager.Instance.StartCoroutine(CleanupAfterThrow());
    }

    private IEnumerator CleanupAfterThrow()
    {
        yield return new WaitForSeconds(0.3f);

        TransferrableObject barrel = VRRig.LocalRig.myBodyDockPositions.allObjects[BarrelIndex];
        barrel.gameObject.SetActive(true);
        barrel.storedZone   = BodyDockPositions.DropPositions.RightArm;
        barrel.currentState = TransferrableObject.PositionState.OnRightArm;

        cleanupCoroutine = null;
    }

    private void StopFling()
    {
        if (flingCoroutine == null) 
            return;
        
        CoroutineManager.Instance.StopCoroutine(flingCoroutine);
        flingCoroutine = null;
        
        NotificationManager.RemoveNotificationEntry(activeNotification);
        activeNotification = null;
    }

    protected override void OnDisable()
    {
        StopFling();

        if (cleanupCoroutine != null)
        {
            CoroutineManager.Instance.StopCoroutine(cleanupCoroutine);
            cleanupCoroutine = null;
        }

        gunLib.OnDisable();
    }
}