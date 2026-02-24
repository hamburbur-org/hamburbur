using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Misc;

[hamburburmod("Force Enable Hands",
        "Makes it so when your controllers get disconnected you dont have to wait for them to come back",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class ForceEnableHands : hamburburmod
{
    protected override void Update()
    {
        if (!Tools.Utils.InVR)
            return;

        ConnectedControllerHandler.Instance.leftControllerValid  = true;
        ConnectedControllerHandler.Instance.rightControllerValid = true;

        ConnectedControllerHandler.Instance.leftValid  = true;
        ConnectedControllerHandler.Instance.rightValid = true;
    }
}