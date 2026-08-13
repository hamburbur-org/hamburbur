using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.MecchaGorilla;

[hamburburmod(                "Rainbow Paint Gun",  "Continuously repaints the targeted mini", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaRainbowPaintGun : MecchaTargetGun
{
    protected override float Delay => 0.2f;

    protected override void Use(VRRig rig) =>
            MecchaNetwork.PaintMini(rig.Creator.ActorNumber, MecchaNetwork.Rainbow(0.35f));
}

[hamburburmod("Paint Dot Gun", "Covers the targeted mini in synchronized rainbow paint dots", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaDotGun : MecchaTargetGun
{
    protected override float Delay => 0.12f;

    protected override void Use(VRRig rig) =>
            MecchaNetwork.Dot(rig.Creator.ActorNumber, Random.insideUnitSphere * 0.22f, 0.045f,
                    MecchaNetwork.Rainbow(0.45f));
}

[hamburburmod("Clear Paint Gun", "Clears synchronized paint dots belonging to the target", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaClearPaintGun : MecchaTargetGun
{
    protected override void Use(VRRig rig) =>
            MecchaNetwork.Send(MecchaEvents.ClearDots, (double)rig.Creator.ActorNumber);
}

[hamburburmod(                "Clear All Paint", "Clears every player's synchronized mini paint dots", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaClearAllPaint : hamburburmod
{
    protected override void Pressed()
    {
        MecchaNetwork.Send(MecchaEvents.ClearDots, (double)MecchaNetwork.LocalId);
        foreach (VRRig rig in NetworkSystem.Instance.Rigs().ToArray())
            if (rig?.Creator != null)
                MecchaNetwork.Send(MecchaEvents.ClearDots, (double)rig.Creator.ActorNumber);
    }
}

[hamburburmod("Rainbow Mini", "Continuously changes your painter mini's bucket color", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaRainbowMini : hamburburmod
{
    private float next;

    protected override void Update()
    {
        if (Time.time < next) return;
        next = Time.time + 0.2f;
        MecchaNetwork.PaintMini(MecchaNetwork.LocalId, MecchaNetwork.Rainbow(0.25f));
    }
}

[hamburburmod("Rainbow Mini Parts", "Cycles rainbow paint across individual mini body parts", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaRainbowParts : hamburburmod
{
    private float next;
    private int   part = 1;

    protected override void Update()
    {
        if (Time.time < next) return;
        next = Time.time + 0.12f;
        MecchaNetwork.PaintPart(MecchaNetwork.LocalId, part, MecchaNetwork.Rainbow(0.3f + part * 0.01f));
        part = part % 8 + 1;
    }
}

[hamburburmod("Rainbow Shotgun", "Continuously changes the networked shotgun color code", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaRainbowShotgun : hamburburmod
{
    private float next;

    protected override void Update()
    {
        if (Time.time < next) return;
        next = Time.time + 0.2f;
        MecchaNetwork.ColorCode(MecchaNetwork.LocalId, MecchaNetwork.Rainbow(0.3f));
    }
}

public abstract class MecchaToolPose : hamburburmod
{
    private            float next;
    protected abstract int   ToolType { get; }

    protected override void Update()
    {
        if (Time.time < next || Tools.Utils.RealRightController == null) return;
        next = Time.time + 0.12f;
        MecchaNetwork.HeldTool(MecchaNetwork.LocalId, ToolType, Tools.Utils.RealRightController);
    }

    protected override void OnDisable()
    {
        if (Tools.Utils.RealRightController != null)
            MecchaNetwork.HeldTool(MecchaNetwork.LocalId, 0, Tools.Utils.RealRightController);
    }
}

[hamburburmod("Network Paint Brush", "Shows a synchronized paint brush on your right hand", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaBrushPose : MecchaToolPose
{
    protected override int ToolType => 1;
}

[hamburburmod(                "Network Eraser", "Shows a synchronized eraser on your right hand", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaEraserPose : MecchaToolPose
{
    protected override int ToolType => 2;
}

[hamburburmod("Network Paint Bucket", "Shows a synchronized paint bucket on your right hand", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaBucketPose : MecchaToolPose
{
    protected override int ToolType => 3;
}

[hamburburmod("Mini Hand Puppet", "Moves your networked painter mini with your right hand", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaMiniPuppet : hamburburmod
{
    private float next;

    protected override void Update()
    {
        Transform hand = Tools.Utils.RealRightController;

        if (hand == null || Time.time < next) return;
        next = Time.time + 0.1f;
        Vector3    p = hand.position;
        Quaternion q = hand.rotation;
        MecchaNetwork.Send(MecchaEvents.Mini, (double)MecchaNetwork.LocalId,
                (double)p.x, (double)p.y, (double)p.z,
                (double)q.x, (double)q.y, (double)q.z, (double)q.w);
    }
}

[hamburburmod("Shotgun Hand Puppet", "Shows and moves your networked shotgun with your right hand",
        ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaShotgunPuppet : hamburburmod
{
    private            float next;
    protected override void  OnEnable() => MecchaNetwork.SetRole(MecchaNetwork.LocalId, true);

    protected override void Update()
    {
        Transform hand = Tools.Utils.RealRightController;

        if (hand == null || Time.time < next) return;
        next = Time.time + 0.1f;
        Vector3    p = hand.position;
        Quaternion q = hand.rotation;
        MecchaNetwork.Send(MecchaEvents.Shotgun, (double)MecchaNetwork.LocalId,
                (double)p.x, (double)p.y, (double)p.z,
                (double)q.x, (double)q.y, (double)q.z, (double)q.w);
    }
}