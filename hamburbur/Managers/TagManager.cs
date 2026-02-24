using System;
using System.Collections.Generic;
using GorillaGameModes;
using GorillaTagScripts;
using hamburbur.Components;
using hamburbur.Tools;
using Photon.Pun;

namespace hamburbur.Managers;

public class TagManager : Singleton<TagManager>
{
    public readonly HashSet<VRRig> TaggedRigs   = [];
    public readonly HashSet<VRRig> UnTaggedRigs = [];

    private void Start()
    {
        RigUtils.OnMatIndexChange += OnMatIndexChange;
        RigUtils.OnRigLoaded      += OnRigSpawned;
        RigUtils.OnRigUnloaded    += OnRigCached;
    }

    public static bool IsTagged(VRRig rig)
    {
        bool isInfectionTagged = rig.setMatIndex == 2 || rig.setMatIndex == 11;
        bool isRockTagged      = rig.setMatIndex == 1;

        return isInfectionTagged || isRockTagged;
    }

    private void OnMatIndexChange(VRRig rig)
    {
        TaggedRigs.Remove(rig);
        UnTaggedRigs.Remove(rig);

        if (IsTagged(rig))
            TaggedRigs.Add(rig);
        else
            UnTaggedRigs.Add(rig);
    }

    private void OnRigSpawned(VRRig rig)
    {
        TaggedRigs.Remove(rig);
        UnTaggedRigs.Remove(rig);

        if (IsTagged(rig))
            TaggedRigs.Add(rig);
        else
            UnTaggedRigs.Add(rig);
    }

    private void OnRigCached(VRRig rig)
    {
        if (rig == null)
            return;

        TaggedRigs.Remove(rig);
        UnTaggedRigs.Remove(rig);
    }

    public void AddInfected(NetPlayer plr)
    {
        if (!PhotonNetwork.InRoom || GorillaGameManager.instance == null)
            return;

        switch (GorillaGameManager.instance.GameType())
        {
            case GameModeType.Infection:
            case GameModeType.InfectionCompetitive:
            case GameModeType.SuperInfect:
            case GameModeType.FreezeTag:
            case GameModeType.PropHunt:
                GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
                if (tagManager.isCurrentlyTag)
                    tagManager.ChangeCurrentIt(plr);
                else if (!tagManager.currentInfected.Contains(plr))
                    tagManager.AddInfectedPlayer(plr);

                break;

            case GameModeType.Ghost:
            case GameModeType.Ambush:
                GorillaAmbushManager ghostManager = (GorillaAmbushManager)GorillaGameManager.instance;
                if (ghostManager.isCurrentlyTag)
                    ghostManager.ChangeCurrentIt(plr);
                else if (!ghostManager.currentInfected.Contains(plr))
                    ghostManager.AddInfectedPlayer(plr);

                break;

            case GameModeType.Paintbrawl:
                GorillaPaintbrawlManager paintbrawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                paintbrawlManager.playerLives[plr.ActorNumber] = 0;

                break;

            case GameModeType.Casual:

            case GameModeType.HuntDown:

            case GameModeType.Custom:

            case GameModeType.Guardian:

            case GameModeType.SuperCasual:

            case GameModeType.Count:

            case GameModeType.None:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RemoveInfected(NetPlayer plr)
    {
        if (!PhotonNetwork.InRoom || GorillaGameManager.instance == null)
            return;

        switch (GorillaGameManager.instance.GameType())
        {
            case GameModeType.Infection:
            case GameModeType.InfectionCompetitive:
            case GameModeType.SuperInfect:
            case GameModeType.FreezeTag:
            case GameModeType.PropHunt:
                GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
                switch (tagManager.isCurrentlyTag)
                {
                    case true when tagManager.currentIt == plr:
                        tagManager.currentIt = null;

                        break;

                    case false when tagManager.currentInfected.Contains(plr):
                        tagManager.currentInfected.Remove(plr);

                        break;
                }

                break;

            case GameModeType.Ghost:
            case GameModeType.Ambush:
                GorillaAmbushManager ghostManager = (GorillaAmbushManager)GorillaGameManager.instance;
                switch (ghostManager.isCurrentlyTag)
                {
                    case true when ghostManager.currentIt == plr:
                        ghostManager.currentIt = null;

                        break;

                    case false when ghostManager.currentInfected.Contains(plr):
                        ghostManager.currentInfected.Remove(plr);

                        break;
                }

                break;

            case GameModeType.Paintbrawl:
                GorillaPaintbrawlManager paintbrawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                paintbrawlManager.playerLives[plr.ActorNumber] = 3;

                break;

            case GameModeType.Casual:

            case GameModeType.HuntDown:

            case GameModeType.Custom:

            case GameModeType.Guardian:

            case GameModeType.SuperCasual:

            case GameModeType.Count:

            case GameModeType.None:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void AddRock(NetPlayer plr)
    {
        if (!PhotonNetwork.InRoom || GorillaGameManager.instance == null)
            return;

        switch (GorillaGameManager.instance.GameType())
        {
            case GameModeType.Infection:
            case GameModeType.InfectionCompetitive:
            case GameModeType.SuperInfect:
            case GameModeType.FreezeTag:
            case GameModeType.PropHunt:
                GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
                tagManager.ChangeCurrentIt(plr);

                break;

            case GameModeType.Ghost:
            case GameModeType.Ambush:
                GorillaAmbushManager ghostManager = (GorillaAmbushManager)GorillaGameManager.instance;
                ghostManager.ChangeCurrentIt(plr);

                break;

            case GameModeType.Paintbrawl:
                GorillaPaintbrawlManager paintbrawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                paintbrawlManager.playerLives[plr.ActorNumber] = 0;

                break;

            case GameModeType.Casual:

            case GameModeType.HuntDown:

            case GameModeType.Custom:

            case GameModeType.Guardian:

            case GameModeType.SuperCasual:

            case GameModeType.Count:

            case GameModeType.None:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RemoveRock(NetPlayer plr)
    {
        if (!PhotonNetwork.InRoom || GorillaGameManager.instance == null)
            return;

        switch (GorillaGameManager.instance.GameType())
        {
            case GameModeType.Infection:
            case GameModeType.InfectionCompetitive:
            case GameModeType.SuperInfect:
            case GameModeType.FreezeTag:
            case GameModeType.PropHunt:
                GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
                if (tagManager.currentIt == plr)
                    tagManager.ChangeCurrentIt(null);

                break;

            case GameModeType.Ghost:
            case GameModeType.Ambush:
                GorillaAmbushManager ghostManager = (GorillaAmbushManager)GorillaGameManager.instance;
                if (ghostManager.currentIt == plr)
                    ghostManager.ChangeCurrentIt(null);

                break;

            case GameModeType.Paintbrawl:
                GorillaPaintbrawlManager paintbrawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                paintbrawlManager.playerLives[plr.ActorNumber] = 3;

                break;

            case GameModeType.Casual:

            case GameModeType.HuntDown:

            case GameModeType.Custom:

            case GameModeType.Guardian:

            case GameModeType.SuperCasual:

            case GameModeType.Count:

            case GameModeType.None:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}