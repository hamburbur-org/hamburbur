using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod(                "Trails ESP",         "Shows recent movement trails for players", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class TrailsESP : hamburburmod
{
    private readonly Dictionary<VRRig, TrailRenderer> trails = new();

    protected override void OnEnable()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs())
            CreateTrail(rig);

        RigUtils.OnRigLoaded   += CreateTrail;
        RigUtils.OnRigUnloaded += DestroyTrail;

        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateTrailLayers;
        UpdateTrailLayers(FirstPersonVisuals.FirstPersonOnly);
    }

    protected override void LateUpdate()
    {
        foreach (KeyValuePair<VRRig, TrailRenderer> entry in trails.ToArray())
        {
            if (entry.Key == null || entry.Value == null)
            {
                if (entry.Value != null)
                    entry.Value.gameObject.Obliterate();

                trails.Remove(entry.Key);

                continue;
            }

            entry.Value.startColor = GetTargetColour(entry.Key);
            entry.Value.endColor = new Color(entry.Value.startColor.r, entry.Value.startColor.g,
                    entry.Value.startColor.b, 0f);
        }
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded                       -= CreateTrail;
        RigUtils.OnRigUnloaded                     -= DestroyTrail;
        FirstPersonVisuals.OnFirstPersonOnlyChange -= UpdateTrailLayers;

        foreach (TrailRenderer trail in trails.Values)
            if (trail != null)
                trail.gameObject.Obliterate();

        trails.Clear();
    }

    private void CreateTrail(VRRig rig)
    {
        if (rig == null || rig.IsLocalRig() || trails.ContainsKey(rig))
            return;

        GameObject trailObject = new("hamburbur player trail");
        trailObject.transform.SetParent(rig.transform, false);
        trailObject.transform.localPosition = new Vector3(0f, -0.45f, 0f);

        TrailRenderer trail  = trailObject.AddComponent<TrailRenderer>();
        Color         colour = GetTargetColour(rig);
        float         width  = 0.035f * (GTPlayer.Instance?.scale ?? 1f);

        trail.time              = 2.5f;
        trail.minVertexDistance = 0.04f;
        trail.startWidth        = width;
        trail.endWidth          = width * 0.35f;
        trail.numCornerVertices = 4;
        trail.numCapVertices    = 4;
        trail.alignment         = LineAlignment.View;
        trail.textureMode       = LineTextureMode.Stretch;
        trail.material          = new Material(Shader.Find("GUI/Text Shader"));
        trail.startColor        = colour;
        trail.endColor          = new Color(colour.r, colour.g, colour.b, 0f);

        trails[rig] = trail;
        UpdateTrailLayer(trail, FirstPersonVisuals.FirstPersonOnly);
    }

    private void DestroyTrail(VRRig rig)
    {
        if (rig == null || !trails.TryGetValue(rig, out TrailRenderer trail))
            return;

        if (trail != null)
            trail.gameObject.Obliterate();

        trails.Remove(rig);
    }

    private void UpdateTrailLayers(bool firstPersonOnly)
    {
        foreach (TrailRenderer trail in trails.Values)
            UpdateTrailLayer(trail, firstPersonOnly);
    }

    private static void UpdateTrailLayer(TrailRenderer trail, bool firstPersonOnly)
    {
        if (trail == null)
            return;

        trail.gameObject.SetLayer(firstPersonOnly ? UnityLayer.FirstPersonOnly : UnityLayer.Default);
    }

    private static Color GetTargetColour(VRRig rig)
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