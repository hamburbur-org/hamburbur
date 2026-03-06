using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("2D Box ESP", "Puts a 2D box over players that you can see through walls", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class BoxESP2D : hamburburmod
{
    private const string BoxGUID = "ajskldfbnklewmvew0uthds";

    private readonly Dictionary<VRRig, BoxData> boxes = new();

    private GameObject boxPrefab;

    protected override void Start()
    {
        boxPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("2DBox");
        base.Start();
    }

    protected override void LateUpdate()
    {
        foreach ((VRRig rig, BoxData boxData) in boxes)
        {
            boxData.BoxTransform.LookAt(Camera.main.transform);
            boxData.SetBoxColour(GetTargetColour(rig));
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

        foreach (BoxData boxData in boxes.Values)
            boxData.BoxTransform.gameObject.SetLayerRecursively(unityLayer);
    }

    private void ObliterateBox(VRRig rig)
    {
        if (rig == null)
            return;

        if (boxes.TryGetValue(rig, out BoxData boxData))
        {
            if (boxData.BoxTransform != null)
                boxData.BoxTransform.gameObject.Obliterate();

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

        GameObject box = Object.Instantiate(boxPrefab, rig.transform);
        BoxData boxData = new()
        {
                BoxTransform = box.transform,
                BoxRenderers = box.transform.Children().Select(t => t.GetComponent<Renderer>()).ToArray(),
        };

        boxes.Add(rig, boxData);

        UpdateBoxVisuals(FirstPersonVisuals.FirstPersonOnly);
        boxData.SetBoxColour(GetTargetColour(rig));
        box.transform.localScale = Vector3.one * 0.8f;
        box.name                 = BoxGUID;
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

    private struct BoxData
    {
        public Transform  BoxTransform;
        public Renderer[] BoxRenderers;

        public void SetBoxColour(Color colour)
        {
            foreach (Renderer renderer in BoxRenderers)
                renderer.material.color = colour;
        }
    }
}