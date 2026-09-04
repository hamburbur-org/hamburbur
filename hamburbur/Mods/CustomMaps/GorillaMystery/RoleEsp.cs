using System;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Multiplayer;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.GorillaMystery;

public abstract class MysteryRoleEsp : hamburburmod
{
    private readonly   Dictionary<VRRig, Renderer> boxes    = [];
    private readonly   Vector3                     boxScale = new(0.31f, 0.41f, 0.31f);
    protected override Type[]                      IncompatibleMods => [typeof(BoxESP3D),];

    protected abstract Color          RoleColour { get; }
    protected abstract MysteryTagRole Role       { get; }
    protected abstract string         BoxName    { get; }

    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void OnEnable()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs())
            CreateBox(rig);

        RigUtils.OnRigLoaded                       += CreateBox;
        RigUtils.OnRigUnloaded                     += DestroyBox;
        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateBoxLayers;
        UpdateBoxLayers(FirstPersonVisuals.FirstPersonOnly);
    }

    protected override void LateUpdate()
    {
        MysteryTagState.PollGameStatus();

        foreach ((VRRig rig, Renderer renderer) in boxes)
        {
            if (rig == null || renderer == null || rig.Creator == null)
                continue;

            int actorNumber = rig.Creator.ActorNumber;
            renderer.enabled = MysteryTagState.GameActive && MysteryTagState.IsAlive(actorNumber) &&
                               MysteryTagState.GetRole(actorNumber) == Role;

            renderer.material.color       = new Color(RoleColour.r, RoleColour.g, RoleColour.b, 0.4f);
            renderer.transform.rotation   = rig.transform.rotation;
            renderer.transform.localScale = Vector3.Scale(boxScale, rig.transform.localScale);
        }
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded                       -= CreateBox;
        RigUtils.OnRigUnloaded                     -= DestroyBox;
        FirstPersonVisuals.OnFirstPersonOnlyChange -= UpdateBoxLayers;

        foreach (VRRig rig in boxes.Keys.ToArray())
            DestroyBox(rig);

        boxes.Clear();
    }

    private void CreateBox(VRRig rig)
    {
        if (rig == null || rig.IsLocalRig() || boxes.ContainsKey(rig))
            return;

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = BoxName;
        box.transform.SetParent(rig.transform);
        box.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale    = Vector3.Scale(boxScale, rig.transform.localScale);

        Renderer renderer = box.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("GUI/Text Shader");
        renderer.material.color  = new Color(RoleColour.r, RoleColour.g, RoleColour.b, 0.4f);
        renderer.enabled         = false;
        boxes[rig]               = renderer;

        box.GetComponent<Collider>()?.Obliterate();
        UpdateBoxLayers(FirstPersonVisuals.FirstPersonOnly);
    }

    private void DestroyBox(VRRig rig)
    {
        if (rig == null)
            return;

        if (boxes.TryGetValue(rig, out Renderer renderer))
        {
            renderer?.gameObject.Obliterate();
            boxes.Remove(rig);

            return;
        }

        Transform oldBox = rig.transform.Find(BoxName);
        oldBox?.gameObject.Obliterate();
    }

    private void UpdateBoxLayers(bool firstPersonOnly)
    {
        UnityLayer layer = firstPersonOnly ? UnityLayer.FirstPersonOnly : UnityLayer.Default;
        foreach (Renderer renderer in boxes.Values)
            renderer?.gameObject.SetLayer(layer);
    }
}

[hamburburmod(                "Murderer ESP", "Shows the murderer with a red box", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MurdererEsp : MysteryRoleEsp
{
    protected override Color          RoleColour => Color.red;
    protected override MysteryTagRole Role       => MysteryTagRole.Murderer;
    protected override string         BoxName    => "HamburburMysteryMurdererESP";
}

[hamburburmod(                "Sheriff ESP", "Shows the sheriff with a blue box", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SheriffEsp : MysteryRoleEsp
{
    protected override Color          RoleColour => Color.blue;
    protected override MysteryTagRole Role       => MysteryTagRole.Sheriff;
    protected override string         BoxName    => "HamburburMysterySheriffESP";
}

[hamburburmod(                "Innocent ESP",       "Shows living innocents with green boxes", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class InnocentEsp : MysteryRoleEsp
{
    protected override Color          RoleColour => Color.green;
    protected override MysteryTagRole Role       => MysteryTagRole.Innocent;
    protected override string         BoxName    => "HamburburMysteryInnocentESP";
}