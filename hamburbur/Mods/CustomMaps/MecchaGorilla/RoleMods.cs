using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.MecchaGorilla;

[hamburburmod("Seeker Gun", "Changes the target into a seeker", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaSeekerGun : MecchaTargetGun
{
    protected override void Use(VRRig rig) => MecchaNetwork.SetRole(rig.Creator.ActorNumber, true);
}

[hamburburmod("Hider Gun", "Changes the target into a hider", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaHiderGun : MecchaTargetGun
{
    protected override void Use(VRRig rig) => MecchaNetwork.SetRole(rig.Creator.ActorNumber, false);
}

[hamburburmod("Become Seeker", "Gives you the seeker role and shotgun", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaBecomeSeeker : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.SetRole(MecchaNetwork.LocalId, true);
}

[hamburburmod("Become Hider", "Gives you the hider role and painter mini", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaBecomeHider : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.SetRole(MecchaNetwork.LocalId, false);
}

[hamburburmod("Everyone Seeker", "Changes everyone into a seeker", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaEveryoneSeeker : hamburburmod
{
    protected override void Pressed()
    {
        MecchaNetwork.SetRole(MecchaNetwork.LocalId, true);
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null) MecchaNetwork.SetRole(rig.Creator.ActorNumber, true);
    }
}

[hamburburmod("Everyone Hider", "Changes everyone into a hider", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaEveryoneHider : hamburburmod
{
    protected override void Pressed()
    {
        MecchaNetwork.SetRole(MecchaNetwork.LocalId, false);
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null) MecchaNetwork.SetRole(rig.Creator.ActorNumber, false);
    }
}

[hamburburmod("Spectator", "Broadcasts spectator mode for you while enabled", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaSpectator : hamburburmod
{
    protected override void OnEnable() =>
            MecchaNetwork.Send(MecchaEvents.Spectate, (double)MecchaNetwork.LocalId, 1d);
    protected override void OnDisable() =>
            MecchaNetwork.Send(MecchaEvents.Spectate, (double)MecchaNetwork.LocalId, 0d);
}

[hamburburmod("Leave Round", "Marks you as having left the current round", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaLeaveRound : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Send(MecchaEvents.Leave, (double)MecchaNetwork.LocalId);
}

public abstract class MecchaRoleEsp : hamburburmod
{
    private readonly Dictionary<VRRig, Renderer> boxes = [];
    private static readonly Vector3 Scale = new(0.31f, 0.41f, 0.31f);
    protected abstract MecchaRole Role { get; }
    protected abstract Color Colour { get; }
    protected abstract string ObjectName { get; }

    protected override void Start() => MecchaState.EnsureInitialized();

    protected override void OnEnable()
    {
        foreach (VRRig rig in NetworkSystem.Instance.Rigs()) Create(rig);
        RigUtils.OnRigLoaded += Create;
        RigUtils.OnRigUnloaded += Destroy;
        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateLayers;
        UpdateLayers(FirstPersonVisuals.FirstPersonOnly);
    }

    protected override void LateUpdate()
    {
        foreach ((VRRig rig, Renderer renderer) in boxes)
        {
            if (rig == null || renderer == null || rig.Creator == null) continue;
            renderer.enabled = MecchaState.IsAlive(rig.Creator.ActorNumber) &&
                               MecchaState.GetRole(rig.Creator.ActorNumber) == Role;
            renderer.material.color = new Color(Colour.r, Colour.g, Colour.b, 0.4f);
            renderer.transform.rotation = rig.transform.rotation;
            renderer.transform.localScale = Vector3.Scale(Scale, rig.transform.localScale);
        }
    }

    protected override void OnDisable()
    {
        RigUtils.OnRigLoaded -= Create;
        RigUtils.OnRigUnloaded -= Destroy;
        FirstPersonVisuals.OnFirstPersonOnlyChange -= UpdateLayers;
        foreach (VRRig rig in boxes.Keys.ToArray()) Destroy(rig);
        boxes.Clear();
    }

    private void Create(VRRig rig)
    {
        if (rig == null || rig.IsLocalRig() || boxes.ContainsKey(rig)) return;
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = ObjectName;
        box.transform.SetParent(rig.transform);
        box.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = Vector3.Scale(Scale, rig.transform.localScale);
        Renderer renderer = box.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("GUI/Text Shader");
        renderer.material.color = new Color(Colour.r, Colour.g, Colour.b, 0.4f);
        boxes[rig] = renderer;
        box.GetComponent<Collider>()?.Obliterate();
        UpdateLayers(FirstPersonVisuals.FirstPersonOnly);
    }

    private void Destroy(VRRig rig)
    {
        if (rig == null) return;
        if (boxes.TryGetValue(rig, out Renderer renderer))
        {
            renderer?.gameObject.Obliterate();
            boxes.Remove(rig);
        }
        else rig.transform.Find(ObjectName)?.gameObject.Obliterate();
    }

    private void UpdateLayers(bool firstPersonOnly)
    {
        UnityLayer layer = firstPersonOnly ? UnityLayer.FirstPersonOnly : UnityLayer.Default;
        foreach (Renderer renderer in boxes.Values) renderer?.gameObject.SetLayer(layer);
    }
}

[hamburburmod("Seeker ESP", "Shows seekers through walls with red boxes", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaSeekerEsp : MecchaRoleEsp
{
    protected override MecchaRole Role => MecchaRole.Seeker;
    protected override Color Colour => Color.red;
    protected override string ObjectName => "HamburburMecchaSeekerESP";
}

[hamburburmod("Hider ESP", "Shows living hiders through walls with green boxes", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaHiderEsp : MecchaRoleEsp
{
    protected override MecchaRole Role => MecchaRole.Hider;
    protected override Color Colour => Color.green;
    protected override string ObjectName => "HamburburMecchaHiderESP";
}
