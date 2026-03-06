using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Bone ESP", "ESP but its their bones", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Boners : hamburburmod
{
    protected override void OnEnable()
    {
        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            SkellonifyRig(rig);

        RigUtils.OnRigLoaded   += SkellonifyRig;
        RigUtils.OnRigUnloaded += UnSkellonifyRig;
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded   -= SkellonifyRig;
        RigUtils.OnRigUnloaded -= UnSkellonifyRig;

        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            UnSkellonifyRig(rig);
    }

    private void SkellonifyRig(VRRig rig)
    {
        rig.skeleton.enabled                  = true;
        rig.skeleton.renderer.enabled         = true;
        rig.skeleton.renderer.material.shader = Shader.Find("GUI/Text Shader");
        rig.skeleton.renderer.material.color  = GetTargetColour(rig);
    }

    private void UnSkellonifyRig(VRRig rig)
    {
        rig.skeleton.enabled                  = false;
        rig.skeleton.renderer.enabled         = false;
        rig.skeleton.renderer.material.shader = Shader.Find("GorillaTag/UberShader");
    }

    private Color GetTargetColour(VRRig rig)
    {
        if (rig.bodyRenderer.cosmeticBodyType == GorillaBodyType.Skeleton)
            return Color.green;

        return rig.setMatIndex switch
               {
                       1       => Color.red,
                       2 or 11 => new Color(1f, 0.3288f, 0f, 1f),
                       3 or 7  => Color.blue,
                       12      => Color.green,
                       var _   => rig.playerColor,
               };
    }
}