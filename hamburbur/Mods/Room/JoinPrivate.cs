using GorillaNetworking;
using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Room;

[hamburburmod(                "Join private", "Makes you join a specific code", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinPrivate : hamburburmod
{
    protected override void Pressed() =>
            ButtonHandler.Instance.Prompt(new PromptData(
                    PromptType.Keyboard,
                    "Enter room code",
                    code => PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, JoinType.Solo),
                    null));
}