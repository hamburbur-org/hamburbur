using System.Linq;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.MecchaGorilla;

[hamburburmod(                "Claim Control Board", "Claims the Meccha Gorilla control board", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,  0)]
public class MecchaClaimBoard : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Send(MecchaEvents.Claim, (double)MecchaNetwork.LocalId);
}

[hamburburmod(                "Unclaim Control Board", "Clears the current control-board claim", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,    0)]
public class MecchaUnclaimBoard : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Send(MecchaEvents.Unclaim, (double)MecchaNetwork.LocalId);
}

public abstract class MecchaSelectMap : hamburburmod
{
    protected abstract int  Map       { get; }
    protected override void Pressed() => MecchaNetwork.Send(MecchaEvents.SelectMap, (double)Map);
}

[hamburburmod(                "Select Forest", "Selects Forest on the round board", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaSelectForest : MecchaSelectMap
{
    protected override int Map => 1;
}

[hamburburmod(                "Select City", "Selects City on the round board", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaSelectCity : MecchaSelectMap
{
    protected override int Map => 2;
}

[hamburburmod(                "Select Colorful Cubes", "Selects Colorful Cubes on the round board", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,    0)]
public class MecchaSelectCubes : MecchaSelectMap
{
    protected override int Map => 3;
}

[hamburburmod(                "Select Random Map", "Selects Random on the round board", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaSelectRandom : MecchaSelectMap
{
    protected override int Map => 99;
}

public abstract class MecchaStartMap : hamburburmod
{
    protected abstract int Map { get; }
    protected override void Pressed()
    {
        int map = Map == 99 ? Random.Range(1, 4) : Map;
        MecchaNetwork.Send(MecchaEvents.StartRound, (double)map);
    }
}

[hamburburmod(                "Start Forest Round", "Starts a round on Forest", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaStartForest : MecchaStartMap
{
    protected override int Map => 1;
}

[hamburburmod(                "Start City Round", "Starts a round on City", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaStartCity : MecchaStartMap
{
    protected override int Map => 2;
}

[hamburburmod(                "Start Cubes Round", "Starts a round on Colorful Cubes", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaStartCubes : MecchaStartMap
{
    protected override int Map => 3;
}

[hamburburmod(                "Start Random Round", "Starts a round on a random non-sentinel map", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaStartRandom : MecchaStartMap
{
    protected override int Map => 99;
}

[hamburburmod(                "Force Lobby", "Returns the map state to the lobby", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MecchaForceLobby : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.SetRoundState(1, 0f, MecchaState.RoundMap);
}

[hamburburmod(                "Force Hide Phase",   "Starts a ten-second seeker-wait phase", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaForceHide : hamburburmod
{
    protected override void Start()   => MecchaState.EnsureInitialized();
    protected override void Pressed() => MecchaNetwork.SetRoundState(3, 10f, SafeMap());
    private static     int  SafeMap() => MecchaState.RoundMap is >= 1 and <= 3 ? MecchaState.RoundMap : 1;
}

[hamburburmod(                "Force Active Round", "Releases seekers into a two-minute active round", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaForceActive : hamburburmod
{
    protected override void Start()   => MecchaState.EnsureInitialized();
    protected override void Pressed() => MecchaNetwork.SetRoundState(4, 120f, SafeMap());
    private static     int  SafeMap() => MecchaState.RoundMap is >= 1 and <= 3 ? MecchaState.RoundMap : 1;
}

[hamburburmod(                "Force Endgame Reveal", "Starts the thirty-second painter reveal", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,   0)]
public class MecchaForceEndgame : hamburburmod
{
    protected override void Start()   => MecchaState.EnsureInitialized();
    protected override void Pressed() => MecchaNetwork.SetRoundState(5, 30f, SafeMap(), 1);
    private static     int  SafeMap() => MecchaState.RoundMap is >= 1 and <= 3 ? MecchaState.RoundMap : 1;
}

[hamburburmod(                "Loop Round States",  "Alternates active round and lobby every half second", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaLoopRoundStates : hamburburmod
{
    private bool  active;
    private float next;
    protected override void Update()
    {
        if (Time.time < next) return;
        next   = Time.time + 0.5f;
        active = !active;
        MecchaNetwork.SetRoundState(active ? 4 : 1, active ? 120f : 0f,
                MecchaState.RoundMap is >= 1 and <= 3 ? MecchaState.RoundMap : 1);
    }
}

[hamburburmod(                "Fast Round Settings", "Sets minimum hide, match, check and whistle timers", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,  0)]
public class MecchaFastSettings : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Settings(1, 10, 10, 10, 10, true);
}

[hamburburmod(                "Many Seeker Settings", "Sets eight seekers and short phase timers", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,   0)]
public class MecchaManySeekerSettings : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Settings(8, 10, 120, 30, 10, true);
}

[hamburburmod(                "Long Round Settings", "Sets every round timer to its maximum", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,  0)]
public class MecchaLongSettings : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Settings(1, 300, 600, 120, 120, false);
}

[hamburburmod(                "Testing Mode Settings", "Enables solo testing with the normal round timers", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,    0)]
public class MecchaTestingSettings : hamburburmod
{
    protected override void Pressed() => MecchaNetwork.Settings(1, 60, 120, 30, 30, true);
}

public abstract class MecchaTeleport : hamburburmod
{
    protected abstract string PointName { get; }
    protected override void Pressed()
    {
        GameObject point = GameObject.Find(PointName);
        point ??= Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(item => item.name == PointName);
        if (point != null) Tools.Utils.TeleportPlayer(point.transform.position);
    }
}

[hamburburmod(                "Teleport Lobby",     "Teleports to Meccha Gorilla's lobby point", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaTeleportLobby : MecchaTeleport
{
    protected override string PointName => "LobbyTeleportPointEMPTY";
}

[hamburburmod(                "Teleport Forest",    "Teleports to the Forest round point", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaTeleportForest : MecchaTeleport
{
    protected override string PointName => "ForestMapTeleportPointEMPTY";
}

[hamburburmod(                "Teleport City",      "Teleports to the City round point", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MecchaTeleportCity : MecchaTeleport
{
    protected override string PointName => "CityMapTeleportPointEMPTY";
}

[hamburburmod(                "Teleport Colorful Cubes", "Teleports to the Colorful Cubes round point", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,      0)]
public class MecchaTeleportCubes : MecchaTeleport
{
    protected override string PointName => "ColorfulCubesTeleportPointEMPTY";
}