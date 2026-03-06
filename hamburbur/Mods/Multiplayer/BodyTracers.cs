using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Body Tracers", "Shows tracers to every single player from your body", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class BodyTracers : hamburburmod
{
    private readonly Dictionary<VRRig, LineRenderer> tracers = new();

    protected override void LateUpdate()
    {
        foreach (KeyValuePair<VRRig, LineRenderer> tracer in tracers)
        {
            tracer.Value.SetPosition(0, GTPlayer.Instance.headCollider.transform.position - new Vector3(0f, 0.6f, 0f));
            tracer.Value.SetPosition(1, tracer.Key.transform.position);
            tracer.Value.material.color = GetTargetColour(tracer.Key);
        }
    }

    protected override void OnEnable()
    {
        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            CreateTracer(rig);

        RigUtils.OnRigLoaded   += CreateTracer;
        RigUtils.OnRigUnloaded += DestroyTracer;

        UpdateTracerVisuals(FirstPersonVisuals.FirstPersonOnly);
        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateTracerVisuals;
    }

    protected override void OnDisable()
    {
        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            DestroyTracer(rig);

        RigUtils.OnRigLoaded   -= CreateTracer;
        RigUtils.OnRigUnloaded -= DestroyTracer;

        FirstPersonVisuals.OnFirstPersonOnlyChange -= UpdateTracerVisuals;

        tracers.Clear();
    }

    private void UpdateTracerVisuals(bool firstPersonOnly)
    {
        UnityLayer unityLayer = firstPersonOnly ? UnityLayer.FirstPersonOnly : UnityLayer.Default;

        foreach (LineRenderer line in tracers.Values)
            line.gameObject.SetLayer(unityLayer);
    }

    private void CreateTracer(VRRig rig)
    {
        GameObject   lineObject = new("hamburbur tracer");
        LineRenderer line       = lineObject.AddComponent<LineRenderer>();

        Color lineColor = GetTargetColour(rig);

        float scale = GTPlayer.Instance.scale;
        line.startWidth = 0.01f * scale;
        line.endWidth   = 0.01f * scale;

        line.positionCount = 2;
        line.useWorldSpace = true;

        line.material.shader = Shader.Find("GUI/Text Shader");
        line.material.color  = lineColor;

        tracers.Add(rig, line);

        UpdateTracerVisuals(FirstPersonVisuals.FirstPersonOnly);
    }

    private void DestroyTracer(VRRig rig)
    {
        if (rig == null)
            return;

        if (!tracers.TryGetValue(rig, out LineRenderer line))
            return;

        line.gameObject.Obliterate();
        tracers.Remove(rig);
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