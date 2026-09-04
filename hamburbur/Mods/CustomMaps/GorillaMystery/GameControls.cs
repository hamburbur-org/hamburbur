using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.GorillaMystery;

[hamburburmod(            "Toggle Knife",       "Toggles the Gorilla Mystery knife using changeWeaponState",
        ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class ToggleMysteryKnife : hamburburmod
{
    protected override void Pressed() =>
            MysteryTagNetwork.Send(MysteryTagEvents.ChangeWeaponState, 1d);
}

[hamburburmod(            "Toggle Pistol",      "Toggles the Gorilla Mystery pistol using changeWeaponState",
        ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class ToggleMysteryPistol : hamburburmod
{
    protected override void Pressed() =>
            MysteryTagNetwork.Send(MysteryTagEvents.ChangeWeaponState, 2d);
}

[hamburburmod(            "Weapon Right Hand",  "Moves your Gorilla Mystery weapon to your right hand",
        ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class MysteryRightHand : hamburburmod
{
    protected override void Pressed()
    {
        if (PhotonNetwork.LocalPlayer != null)
            MysteryTagNetwork.Send(
                    MysteryTagEvents.HandChanged,
                    (double)PhotonNetwork.LocalPlayer.ActorNumber,
                    0d);
    }
}

[hamburburmod(            "Weapon Left Hand",   "Moves your Gorilla Mystery weapon to your left hand",
        ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class MysteryLeftHand : hamburburmod
{
    protected override void Pressed()
    {
        if (PhotonNetwork.LocalPlayer != null)
            MysteryTagNetwork.Send(
                    MysteryTagEvents.HandChanged,
                    (double)PhotonNetwork.LocalPlayer.ActorNumber,
                    1d);
    }
}

[hamburburmod(            "Become Sheriff",     "Claims the sheriff role when the map currently has no sheriff",
        ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class BecomeMysterySheriff : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        if (PhotonNetwork.LocalPlayer != null)
            MysteryTagNetwork.Send(
                    MysteryTagEvents.ChooseSheriff,
                    (double)PhotonNetwork.LocalPlayer.ActorNumber);
    }
}

[hamburburmod(            "[Master] Become Murderer", "Assigns you as the Gorilla Mystery murderer",
        ButtonType.Fixed, AccessSetting.Public,       EnabledType.Disabled, 0)]
public class BecomeMysteryMurderer : hamburburmod
{
    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void Pressed()
    {
        if (!MysteryTagNetwork.RequireMasterClient() || PhotonNetwork.LocalPlayer == null)
            return;

        MysteryTagNetwork.Send(
                MysteryTagEvents.ChooseMurder,
                (double)PhotonNetwork.LocalPlayer.ActorNumber);
    }
}

public abstract class StartMysteryMap : hamburburmod
{
    protected abstract int MapNumber { get; }

    protected override void Pressed()
    {
        if (MysteryTagNetwork.RequireMasterClient())
            MysteryTagNetwork.Send(MysteryTagEvents.StartGame, (double)MapNumber);
    }
}

[hamburburmod(            "[Master] Start Map 1", "Immediately starts Gorilla Mystery on map one",
        ButtonType.Fixed, AccessSetting.Public,   EnabledType.Disabled, 0)]
public class StartMysteryMap1 : StartMysteryMap
{
    protected override int MapNumber => 1;
}

[hamburburmod(            "[Master] Start Map 2", "Immediately starts Gorilla Mystery on map two",
        ButtonType.Fixed, AccessSetting.Public,   EnabledType.Disabled, 0)]
public class StartMysteryMap2 : StartMysteryMap
{
    protected override int MapNumber => 2;
}

[hamburburmod(            "[Master] Start Map 3", "Immediately starts Gorilla Mystery on map three",
        ButtonType.Fixed, AccessSetting.Public,   EnabledType.Disabled, 0)]
public class StartMysteryMap3 : StartMysteryMap
{
    protected override int MapNumber => 3;
}

[hamburburmod(            "[Master] Murderer Wins", "Ends the round with a murderer victory",
        ButtonType.Fixed, AccessSetting.Public,     EnabledType.Disabled, 0)]
public class ForceMurdererWin : hamburburmod
{
    protected override void Pressed()
    {
        if (MysteryTagNetwork.RequireMasterClient())
            MysteryTagNetwork.Send(MysteryTagEvents.EndGame, 1d);
    }
}

[hamburburmod(            "[Master] Innocents Win", "Ends the round with an innocents victory",
        ButtonType.Fixed, AccessSetting.Public,     EnabledType.Disabled, 0)]
public class ForceInnocentsWin : hamburburmod
{
    protected override void Pressed()
    {
        if (MysteryTagNetwork.RequireMasterClient())
            MysteryTagNetwork.Send(MysteryTagEvents.EndGame, 2d);
    }
}

[hamburburmod(                "[Master] Spam Rounds", "Alternates starting and ending Gorilla Mystery rounds every 0.5 seconds",
        ButtonType.Togglable, AccessSetting.Public,   EnabledType.Disabled, 0)]
public class SpamMysteryRounds : hamburburmod
{
    private const float EventDelay = 0.3f;
    private       float nextEventTime;
    private       int   nextMap    = 1;
    private       int   nextWinner = 1;

    private bool sendStart = true;
    private bool warnedNotMaster;

    protected override void OnEnable()
    {
        sendStart       = true;
        warnedNotMaster = false;
        nextEventTime   = 0f;
        nextMap         = 1;
        nextWinner      = 1;
    }

    protected override void Update()
    {
        if (!Tools.Utils.IsMasterClient)
        {
            if (warnedNotMaster)
                return;

            MysteryTagNetwork.RequireMasterClient();
            warnedNotMaster = true;

            return;
        }

        warnedNotMaster = false;

        if (Time.time < nextEventTime)
            return;

        nextEventTime = Time.time + EventDelay;

        if (sendStart)
        {
            MysteryTagNetwork.Send(MysteryTagEvents.StartGame, (double)nextMap);
            nextMap = nextMap % 3 + 1;
        }
        else
        {
            MysteryTagNetwork.Send(MysteryTagEvents.EndGame, (double)nextWinner);
            nextWinner = nextWinner == 1 ? 2 : 1;
        }

        sendStart = !sendStart;
    }
}