using System;
using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.CustomMaps.GorillaMystery;

public abstract class SpamMysteryVote : hamburburmod
{
    private static readonly Type[] VoteModTypes =
    [
            typeof(SpamVoteMap1),
            typeof(SpamVoteMap2),
            typeof(SpamVoteMap3),
    ];

    private float nextVoteTime;

    protected abstract int MapNumber { get; }

    protected override void Start() => MysteryTagState.EnsureInitialized();

    protected override void OnEnable()
    {
        foreach (Type type in VoteModTypes)
            if (type != GetType() && ModRegistry.TryGet(type, out hamburburmod voteMod) && voteMod.Enabled)
                voteMod.SetEnabledFromSystem(false);
    }

    protected override void Update()
    {
        MysteryTagState.PollGameStatus();

        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null || MysteryTagState.GameActive ||
            Time.time                                          < nextVoteTime)
            return;

        nextVoteTime = Time.time + 0.1f;

        MysteryTagNetwork.Send(
                MysteryTagEvents.PlayerVote,
                (double)Random.Range(0, 99999),
                (double)MapNumber);
    }
}

[hamburburmod(                "Spam Vote Map 1",    "Continuously forces room votes to Gorilla Mystery map one while no game is active",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpamVoteMap1 : SpamMysteryVote
{
    protected override int MapNumber => 1;
}

[hamburburmod(                "Spam Vote Map 2",    "Continuously forces room votes to Gorilla Mystery map two while no game is active",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpamVoteMap2 : SpamMysteryVote
{
    protected override int MapNumber => 2;
}

[hamburburmod(                "Spam Vote Map 3",    "Continuously forces room votes to Gorilla Mystery map three while no game is active",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpamVoteMap3 : SpamMysteryVote
{
    protected override int MapNumber => 3;
}