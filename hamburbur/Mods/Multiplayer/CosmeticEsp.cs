using System.Linq;
using hamburbur.Components;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Rare Cosmetic ESP", "Most cosmetics on people will be visible through walls", ButtonType.Togglable,
        AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class CosmeticEsp : hamburburmod
{
    protected override void OnEnable()
    {
        if (NetworkSystem.Instance.InRoom)
            foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
                ApplyEsp(rig);

        RigUtils.OnRigCosmeticsLoaded += ApplyEsp;
        RigUtils.OnRigUnloaded        += RestoreCosmetics;
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigCosmeticsLoaded -= ApplyEsp;
        RigUtils.OnRigUnloaded        -= RestoreCosmetics;

        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            ApplyEsp(rig);
    }

    private static void ApplyEsp(VRRig rig)
    {
        foreach (GameObject cosmeticObject in rig.cosmetics)
        {
            if (!cosmeticObject.TryGetComponent(out MeshRenderer meshRenderer))
                continue;

            meshRenderer.material.shader = Shader.Find("GUI/Text Shader");
            meshRenderer.material.color  = Plugin.Instance.MainColour;

            cosmeticObject.AddComponent<ColourChanger>().alpha = 0.4f;
        }
    }

    private void RestoreCosmetics(VRRig rig)
    {
        foreach (GameObject cosmeticObject in rig.cosmetics)
        {
            if (!cosmeticObject.TryGetComponent(out MeshRenderer meshRenderer))
                continue;

            // ReSharper disable once ShaderLabShaderReferenceNotResolved
            meshRenderer.material.shader = Shader.Find("hatlas");
            meshRenderer.material.color  = new Color(1, 1, 1, 1);
        }
    }
}