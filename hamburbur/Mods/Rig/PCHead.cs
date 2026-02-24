using hamburbur.Components;
using HarmonyLib;

namespace hamburbur.Mods.Rig;

public class PCHead : hamburburmod
{
    public static      bool           IsEnabled;
    protected override string         PersistentModName => "PC Head";
    public override    string         Description       => "The head rotates with the camera on PC";
    public override    ButtonType     ButtonType        => ButtonType.Togglable;
    public override    AccessSettings AccessSettings    => AccessSettings.Public;

    private void OnEnable()  => IsEnabled = true;
    private void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class PCHeadPatch
{
    private static void Postfix(VRRig __instance)
    {
        if (!PCHead.IsEnabled || !__instance.isLocal)
            return;

        __instance.head.rigTarget.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
    }
}