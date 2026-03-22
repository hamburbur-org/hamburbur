using System.Collections.Generic;
using System.Linq;
using hamburbur.Components;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Cosmetic ESP", "Most cosmetics on people will be visible through walls", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class CosmeticEsp : hamburburmod
{
    private static readonly Dictionary<VRRig, Dictionary<Renderer, Material[]>> rigRenderers = new();

    protected override void OnEnable()
    {
        if (NetworkSystem.Instance.InRoom)
            foreach (VRRig rig in VRRigCache.m_activeRigs.Where(r => !r.isLocal))
                InitRig(rig);

        RigUtils.OnRigCosmeticsLoaded += InitRig;
        RigUtils.OnRigUnloaded        += RestoreRig;
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigCosmeticsLoaded -= InitRig;
        RigUtils.OnRigUnloaded        -= RestoreRig;

        foreach (VRRig rig in rigRenderers.Keys.ToList())
            RestoreRig(rig);

        rigRenderers.Clear();
    }

    protected override void Update()
    {
        foreach (KeyValuePair<VRRig, Dictionary<Renderer, Material[]>> pair in rigRenderers)
        {
            VRRig rig = pair.Key;

            if (rig == null)
                continue;

            foreach (GameObject cosmetic in rig.cosmetics)
                ScanRecursive(rig, cosmetic);
        }
    }

    private static void InitRig(VRRig rig)
    {
        if (rig == null || rig.isLocal)
            return;

        if (!rigRenderers.ContainsKey(rig))
            rigRenderers[rig] = new Dictionary<Renderer, Material[]>();

        foreach (GameObject cosmetic in rig.cosmetics)
            ScanRecursive(rig, cosmetic);
    }

    private static void ScanRecursive(VRRig rig, GameObject obj)
    {
        if (obj == null)
            return;

        if (obj.TryGetComponent(out Renderer renderer))
        {
            Dictionary<Renderer, Material[]> dict = rigRenderers[rig];

            if (!dict.ContainsKey(renderer))
            {
                Material[] original = renderer.materials;
                Material[] copies   = new Material[original.Length];

                for (int i = 0; i < original.Length; i++)
                    copies[i] = new Material(original[i]);

                dict[renderer] = copies;
                foreach (Material mat in renderer.materials)
                {
                    mat.shader = Shader.Find("GUI/Text Shader");
                    mat.color  = Plugin.Instance.MainColour;
                }

                if (!obj.TryGetComponent(out ColourChanger changer))
                {
                    changer       = obj.AddComponent<ColourChanger>();
                    changer.alpha = 0.4f;
                }
            }
        }

        foreach (Transform child in obj.transform)
            ScanRecursive(rig, child.gameObject);
    }

    private static void RestoreRig(VRRig rig)
    {
        if (rig == null)
            return;

        if (!rigRenderers.TryGetValue(rig, out Dictionary<Renderer, Material[]> dict))
            return;

        foreach (KeyValuePair<Renderer, Material[]> pair in dict.ToList())
        {
            Renderer renderer = pair.Key;

            if (renderer == null)
                continue;

            renderer.materials = pair.Value;

            if (renderer.TryGetComponent(out ColourChanger changer))
                Object.Destroy(changer);
        }

        rigRenderers.Remove(rig);
    }
}