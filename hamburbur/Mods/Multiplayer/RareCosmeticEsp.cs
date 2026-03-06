using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Rare Cosmetic ESP", "If anyone has a special cosmetic it will be visible through walls", ButtonType.Togglable,
        AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class RareCosmeticEsp : hamburburmod
{
    protected override void OnEnable()
    {
        if (NetworkSystem.Instance.InRoom)
            foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
                CheckAndApplyEsp(rig);

        RigUtils.OnRigCosmeticsLoaded += CheckAndApplyEsp;
        RigUtils.OnRigUnloaded        += RestoreCosmetics;
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigCosmeticsLoaded -= CheckAndApplyEsp;
        RigUtils.OnRigUnloaded        -= RestoreCosmetics;

        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            CheckAndApplyEsp(rig);
    }

    private void CheckAndApplyEsp(VRRig rig)
    {
        if (!Plugin.Instance.specialCosmetics.Keys.Any(cosmeticKey => rig._playerOwnedCosmetics.Contains(cosmeticKey)) ||
            rig.isLocal)
            return;

        foreach (GameObject cosmeticObject in rig.cosmetics.Where(cosmeticObject =>
                                                                          Plugin.Instance.specialCosmetics.Keys
                                                                                 .Any(cosmeticKey =>
                                                                                          cosmeticObject.name
                                                                                                 .Contains(
                                                                                                          cosmeticKey))))
        {
            if (!cosmeticObject.TryGetComponent(out MeshRenderer meshRenderer))
                continue;

            meshRenderer.material.shader = Shader.Find("GUI/Text Shader");
            meshRenderer.material.color  = Plugin.Instance.MainColour;
        }
    }

    private void RestoreCosmetics(VRRig rig)
    {
        foreach (GameObject cosmeticObject in rig.cosmetics.Where(cosmeticObject =>
                                                                          Plugin.Instance.specialCosmetics.Keys
                                                                                 .Any(cosmeticKey =>
                                                                                          cosmeticObject.name
                                                                                                 .Contains(
                                                                                                          cosmeticKey))))
        {
            if (!cosmeticObject.TryGetComponent(out MeshRenderer meshRenderer))
                continue;

            // ReSharper disable once ShaderLabShaderReferenceNotResolved
            meshRenderer.material.shader = Shader.Find("hatlas");
            meshRenderer.material.color  = new Color(1, 1, 1, 1);
        }
    }
}