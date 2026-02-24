using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;

namespace hamburbur.Mods.Scoreboard;

[hamburburmod(                "Go to Player", "Go to the associated player", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class PlayerLine : hamburburmod
{
    public static VRRig CurrentRig;
    public        VRRig AssociatedRig;

    public override string ModName => AssociatedRig?.OwningNetPlayer()?.SanitizedNickName ??
                                      AssociatedRig?.playerText1.text ?? "UNKNOWN";

    protected override void Pressed()
    {
        CurrentRig = AssociatedRig;
        ButtonHandler.Instance.SetCategory("Player Commands");
    }
}