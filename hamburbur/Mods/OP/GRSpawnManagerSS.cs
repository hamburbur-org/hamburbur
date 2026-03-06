using System.Collections;
using GorillaLocomotion;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.OP;

public static class GRSpawnManagerSS
{
    public static void SpawnObjectGlobal(Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.InRoom)
            return;

        GameEntityManager gameEntityManager = GameEntityManager.GetManagerForZone(GhostReactor.instance.zone);
        gameEntityManager.photonView.RPC("BroadcastHandprint", RpcTarget.All, position, rotation);
    }

    public static void SpawnObject(Player targetPlayer, Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.InRoom)
            return;

        GameEntityManager gameEntityManager = GameEntityManager.GetManagerForZone(GhostReactor.instance.zone);
        gameEntityManager.photonView.RPC("BroadcastHandprint", targetPlayer, position, rotation);
    }
}

[hamburburmod("GR Trail", "Gives you a server sided trail in ghost reactor", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GRTrail : hamburburmod
{
    protected override void Update()
    {
        Vector3 position = GTPlayer.Instance.bodyCollider.transform.position;
        Quaternion rotation = Quaternion.LookRotation(
                Vector3.forward,
                GTPlayer.Instance.bodyCollider.transform.rotation * Vector3.forward
        );

        GRSpawnManagerSS.SpawnObjectGlobal(position, rotation);
    }
}

[hamburburmod(                "GR Draw Gun", "Gives you a server sided draw gun in ghost reactor", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GRDrawGun : hamburburmod
{
    private readonly GunLib gunLib = new();

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting)
            GRSpawnManagerSS.SpawnObjectGlobal(
                    gunLib.Hit.point + gunLib.Hit.normal * 0.05f,
                    Quaternion.LookRotation(Vector3.forward, gunLib.Hit.normal)
            );
    }

    protected override void OnDisable() => gunLib.OnDisable();
}

[hamburburmod("GR Blind All", "Blinds everyone in ghost reactor", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GRBlindAll : hamburburmod
{
    protected override void Update()
    {
        foreach (VRRig rig in VRRigCache.m_activeRigs)
        {
            if (rig.isLocal)
                continue;

            Vector3    position = rig.headMesh.transform.position;
            Quaternion rotation = rig.headMesh.transform.rotation;

            GRSpawnManagerSS.SpawnObject(
                    rig.Creator.GetPlayerRef(),
                    position,
                    rotation
            );
        }
    }
}

[hamburburmod("GR Hamburbur Text", "SS hamburbur in ghost reactor", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class GRHamburburText : hamburburmod
{
    private Coroutine spawnTextRoutine;

    protected override void OnEnable()
        => spawnTextRoutine = CoroutineManager.Instance.StartCoroutine(SpawnTextRoutine());

    protected override void OnDisable()
    {
        if (spawnTextRoutine != null)
            CoroutineManager.Instance.StopCoroutine(spawnTextRoutine);

        spawnTextRoutine = null;
    }

    private IEnumerator SpawnTextRoutine()
    {
        while (true)
        {
            DrawWord(
                    "hamburbur",
                    new Vector3(-35f - 25f, -70f),
                    Quaternion.Euler(0f, 0f, 0f),
                    1f,
                    0.5f
            );

            yield return new WaitForSeconds(1f);
        }
    }

    private void DrawWord(string text, Vector3 origin, Quaternion rotation, float scale, float spacing)
    {
        Vector3 cursor = Vector3.zero;

        foreach (char c in text.ToUpper())
        {
            if (!Tools.Utils.BITFont.TryGetValue(c, out Vector2Int[] points))
            {
                cursor += Vector3.right * spacing;

                continue;
            }

            foreach (Vector2Int point in points)
            {
                Vector3 localOffset   = new(point.x * scale, point.y * scale, 0f);
                Vector3 rotatedOffset = rotation * (cursor + localOffset);
                Vector3 finalPos      = origin + rotatedOffset;

                GRSpawnManagerSS.SpawnObjectGlobal(finalPos, rotation);
            }

            cursor += Vector3.right * spacing;
        }
    }
}