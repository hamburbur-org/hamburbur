using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("4D Box ESP", "Puts a 4D box over players that you can see through walls", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class BoxESP4D : hamburburmod
{
    private const string BoxGUID = "gutyet78etf76efgi7ew6ftg8796werftg";

    private readonly Dictionary<VRRig, Renderer> boxes = new();

    private readonly Vector3 boxScale = new(0.9f, 0.9f, 0.000001f);

    protected override void LateUpdate()
    {
        foreach (KeyValuePair<VRRig, Renderer> kvp in boxes)
        {
            VRRig    rig      = kvp.Key;
            Renderer renderer = kvp.Value;

            if (rig == null || renderer == null)
                continue;

            Color newColor = GetTargetColour(rig);
            renderer.material.color = new Color(newColor.r, newColor.g, newColor.b, 0.4f);

            renderer.transform.rotation   = rig.transform.rotation;
            renderer.transform.localScale = Vector3.Scale(boxScale, rig.transform.localScale);
        }
    }

    protected override void OnEnable()
    {
        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => !rig.isLocal))
            CreateBox(rig);

        RigUtils.OnRigLoaded   += CreateBox;
        RigUtils.OnRigUnloaded += ObliterateBox;

        UpdateBoxVisuals(FirstPersonVisuals.FirstPersonOnly);
        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateBoxVisuals;
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded   -= CreateBox;
        RigUtils.OnRigUnloaded -= ObliterateBox;

        foreach (VRRig rig in VRRigCache.m_activeRigs)
            ObliterateBox(rig);

        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateBoxVisuals;

        boxes.Clear();
    }

    private void UpdateBoxVisuals(bool firstPersonOnly)
    {
        UnityLayer unityLayer = firstPersonOnly ? UnityLayer.FirstPersonOnly : UnityLayer.Default;

        foreach (Renderer renderer in boxes.Values)
            renderer.gameObject.SetLayer(unityLayer);
    }

    private void ObliterateBox(VRRig rig)
    {
        if (rig == null)
            return;

        if (boxes.TryGetValue(rig, out Renderer renderer))
        {
            if (renderer != null)
                renderer.gameObject.Obliterate();

            boxes.Remove(rig);
        }
        else
        {
            Transform oldBox = rig.transform.Find(BoxGUID);
            if (oldBox != null)
                oldBox.gameObject.Obliterate();
        }
    }

    private void CreateBox(VRRig rig)
    {
        if (rig.isLocal || boxes.ContainsKey(rig))
            return;

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = BoxGUID;
        box.transform.SetParent(rig.transform, false);
        box.transform.localRotation = rig.transform.rotation;
        box.transform.localScale    = Vector3.Scale(boxScale, rig.transform.localScale);

        if (box.TryGetComponent(out Renderer renderer))
        {
            Color boxColor = GetTargetColour(rig);
            renderer.material.shader = Shader.Find("GUI/Text Shader");
            renderer.material.color  = new Color(boxColor.r, boxColor.g, boxColor.b, 0.35f);
            boxes[rig]               = renderer;
        }

        UpdateBoxVisuals(FirstPersonVisuals.FirstPersonOnly);

        box.GetComponent<BoxCollider>().Obliterate();
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