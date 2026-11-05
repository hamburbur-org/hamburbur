using System.Collections;
using System.Collections.Generic;
using GorillaTagScripts;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.Blocks;

public abstract class MasterBlockPatternGun : hamburburmod
{
    private readonly GunLib gun = new() { ShouldFollow = true, };
    private          int    buildGeneration;
    private          bool   building;
    private          bool   previousTrigger;

    protected override void Start() => gun.Start();

    protected override void LateUpdate()
    {
        gun.LateUpdate();
        bool shooting = gun.IsShooting;

        if (shooting && !previousTrigger && !building && gun.ChosenRig != null)
        {
            if (!PhotonNetwork.IsMasterClient || !BuilderBlockLib.TryGetTable(out BuilderTable _))
            {
                NotificationManager.SendNotification("<color=#58d3ff>Block Mods</color>",
                        "A ready Monke Blocks room and master client are required.", 4f, true, false);
            }
            else
            {
                Vector3 center = gun.ChosenRig.bodyTransform != null
                                         ? gun.ChosenRig.bodyTransform.position + Vector3.down * 0.8f
                                         : gun.ChosenRig.transform.position;

                building = true;
                CoroutineManager.Instance.StartCoroutine(Build(CreatePattern(center)));
            }
        }

        previousTrigger = shooting;
    }

    private IEnumerator Build(IEnumerable<BuilderBlockLib.BlockShot> shots)
    {
        Photon.Realtime.Room room       = PhotonNetwork.CurrentRoom;
        int                  generation = buildGeneration;

        yield return BuilderBlockLib.SpawnSequence(shots, 0.07f,
                () => Enabled && generation     == buildGeneration && PhotonNetwork.IsMasterClient &&
                      PhotonNetwork.CurrentRoom == room            &&
                      BuilderBlockLib.TryGetTable(out BuilderTable _));

        building = false;
    }

    protected abstract IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(Vector3 baseCenter);

    protected override void OnDisable()
    {
        buildGeneration++;
        building = false;
        gun.OnDisable();
        previousTrigger = false;
    }

    protected static BuilderBlockLib.BlockShot Shot(Vector3 position) =>
            new(position, Quaternion.identity, Vector3.zero, Vector3.zero);
}

[hamburburmod(                "[Master] Block Box Gun", "Enclose the targeted player in a block box",
        ButtonType.Togglable, AccessSetting.Public,     EnabledType.Disabled, 0)]
public sealed class MasterBlockBoxGun : MasterBlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(Vector3 baseCenter)
    {
        const float spacing   = 0.3f;
        const float halfWidth = 0.6f;

        for (int y = 0; y < 6; y++)
        {
            float height = 0.15f + y * spacing;
            for (int x = -2; x <= 2; x++)
            {
                float offset = x * spacing;

                yield return Shot(baseCenter + new Vector3(offset, height, -halfWidth));
                yield return Shot(baseCenter + new Vector3(offset, height, halfWidth));
            }

            for (int z = -1; z <= 1; z++)
            {
                float offset = z * spacing;

                yield return Shot(baseCenter + new Vector3(-halfWidth, height, offset));
                yield return Shot(baseCenter + new Vector3(halfWidth,  height, offset));
            }
        }

        for (int x = -2; x <= 2; x++)
            for (int z = -2; z <= 2; z++)
                yield return Shot(baseCenter + new Vector3(x * spacing, 1.95f, z * spacing));
    }
}

[hamburburmod(                "[Master] Block Hut Gun", "Build a roofed hut with a doorway around the targeted player",
        ButtonType.Togglable, AccessSetting.Public,     EnabledType.Disabled, 0)]
public sealed class MasterBlockHutGun : MasterBlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(Vector3 baseCenter)
    {
        const float spacing   = 0.3f;
        const float halfWidth = 0.6f;

        for (int y = 0; y < 6; y++)
        {
            float height = 0.15f + y * spacing;
            for (int x = -2; x <= 2; x++)
            {
                float offset = x * spacing;

                yield return Shot(baseCenter + new Vector3(offset, height, halfWidth));

                if (y >= 4 || x is <= -2 or >= 2)
                    yield return Shot(baseCenter + new Vector3(offset, height, -halfWidth));
            }

            for (int z = -1; z <= 1; z++)
            {
                float offset = z * spacing;

                yield return Shot(baseCenter + new Vector3(-halfWidth, height, offset));
                yield return Shot(baseCenter + new Vector3(halfWidth,  height, offset));
            }
        }

        for (int x = -2; x <= 2; x++)
            for (int z = -2; z <= 2; z++)
                yield return Shot(baseCenter + new Vector3(x * spacing, 1.95f, z * spacing));
    }
}

[hamburburmod(                "[Master] Block Meteor Gun", "Drop a short burst of blocks over the targeted player",
        ButtonType.Togglable, AccessSetting.Public,        EnabledType.Disabled, 0)]
public sealed class MasterBlockMeteorGun : MasterBlockPatternGun
{
    protected override IEnumerable<BuilderBlockLib.BlockShot> CreatePattern(Vector3 baseCenter)
    {
        for (int i = 0; i < 16; i++)
        {
            Vector3 position = baseCenter + BuilderBlockLib.RandomHorizontal(1.1f) +
                               Vector3.up * Random.Range(2.8f, 4.2f);

            yield return new BuilderBlockLib.BlockShot(position, Random.rotation,
                    Vector3.down * Random.Range(3f, 6f), Random.insideUnitSphere * 2f);
        }
    }
}