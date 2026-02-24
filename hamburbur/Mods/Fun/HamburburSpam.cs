using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod(                "Hamburbur Spam", "Spam spawns in hamburburs", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class HamburburSpam : hamburburmod
{
    private readonly GunLib           gunLib            = new();
    private readonly List<GameObject> SpawnedHamburburs = [];
    private          GameObject       HamburburPrefab;
    private          float            lastTime;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || !(Time.time - lastTime > 0.2f))
            return;

        lastTime = Time.time;

        GameObject dummyCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dummyCube.GetComponent<Renderer>().Obliterate();
        dummyCube.transform.position   = gunLib.Hit.point + Vector3.up * 0.25f;
        dummyCube.transform.localScale = new Vector3(1f, 0.8f, 1f);

        Rigidbody rb = dummyCube.AddComponent<Rigidbody>();
        dummyCube.SetLayer(UnityLayer.Default);

        GameObject hamburburObject = GameObject.Instantiate(HamburburPrefab, dummyCube.transform, false);
        hamburburObject.GetComponent<MeshCollider>().Obliterate();
        hamburburObject.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        hamburburObject.transform.localRotation = Quaternion.identity;
        hamburburObject.transform.localScale    = new Vector3(1f, 1.2f, 1f);

        SpawnedHamburburs.Add(dummyCube);
    }

    protected override void OnEnable()
    {
        if (HamburburPrefab != null)
            return;

        HamburburPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("burger");
    }

    protected override void OnDisable()
    {
        gunLib.OnDisable();

        if (SpawnedHamburburs == null)
            return;

        foreach (GameObject hamburburObjects in SpawnedHamburburs)
            hamburburObjects.Obliterate();

        SpawnedHamburburs.Clear();
    }
}