using System.Collections;
using System.Threading.Tasks;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Fun;

[hamburburmod("Nuke", "Falling nuke", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled, 0)]
public class Nuke : hamburburmod
{
    private Coroutine fallRoutine;
    private int       nukeAssetId;

    protected override void OnEnable()
    {
        nukeAssetId = Components.Console.GetFreeAssetID();

        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All,
                "consolehamburburassets", "nuke", nukeAssetId);

        Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, nukeAssetId,
                Vector3.one * 25);

        Vector3 spawnPos = GTPlayer.Instance.headCollider.transform.position + Vector3.up * 30f + Vector3.forward * 2f;

        fallRoutine = CoroutineManager.Instance.StartCoroutine(FallNuke(spawnPos));
    }

    private IEnumerator FallNuke(Vector3 pos)
    {
        yield return new WaitForSeconds(1f);

        float speed = 10f;

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All,
                nukeAssetId, pos);

        while (true)
        {
            Vector3 nextPos = pos + Vector3.down * speed * Time.deltaTime;

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, speed * Time.deltaTime + 0.1f,
                        Tools.Utils.NoInvisLayerMask()))
            {
                Explode(hit.point);

                Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All,
                        nukeAssetId, hit.point);

                break;
            }

            pos = nextPos;

            Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All,
                    nukeAssetId, pos);

            yield return null;
        }

        fallRoutine = null;
    }

    private void Explode(Vector3 position)
    {
        float radius = 25f;
        float force  = 80f;

        foreach (VRRig rig in VRRigCache.m_activeRigs)
        {
            if (rig == null)
                continue;

            float dist = Vector3.Distance(position, rig.transform.position);

            if (dist > radius)
                continue;

            Vector3 dir     = (rig.transform.position - position).normalized;
            float   falloff = 1f - dist / radius;

            Vector3 velocity = dir * force * falloff + Vector3.up * 10f;

            Components.Console.ExecuteCommand("vel",
                    rig.creator.ActorNumber, velocity);
        }

        int explosionId = Components.Console.GetFreeAssetID();
        Components.Console.ExecuteCommand("asset-spawn",       ReceiverGroup.All, "btools", "Explosion", explosionId);
        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, explosionId, position);
        
        Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, explosionId, "Sound", "Explode");

        Task.Run(async () =>
                 {
                     await Task.Delay(1000);
                     Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, explosionId);
                 });
    }

    protected override void OnDisable()
    {
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, nukeAssetId);

        if (fallRoutine != null)
            CoroutineManager.Instance.StopCoroutine(fallRoutine);

        fallRoutine = null;
    }
}