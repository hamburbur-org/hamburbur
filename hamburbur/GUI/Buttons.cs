using System;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Blocks;
using hamburbur.Mods.Categories;
using hamburbur.Mods.Console;
using hamburbur.Mods.Console.Assets;
using hamburbur.Mods.Console.Fun;
using hamburbur.Mods.Credits;
using hamburbur.Mods.CustomMaps;
using hamburbur.Mods.CustomMaps.ChimpCombat;
using hamburbur.Mods.CustomMaps.CrownTag;
using hamburbur.Mods.CustomMaps.GorillaMystery;
using hamburbur.Mods.CustomMaps.MecchaGorilla;
using hamburbur.Mods.CustomMaps.SlapBattles;
using hamburbur.Mods.CustomMaps.XenoGorilla;
using hamburbur.Mods.Fun;
using hamburbur.Mods.Macros;
using hamburbur.Mods.MapLoader;
using hamburbur.Mods.MasterClient;
using hamburbur.Mods.Misc;
using hamburbur.Mods.Movement;
using hamburbur.Mods.Movement.Walker;
using hamburbur.Mods.Multiplayer;
using hamburbur.Mods.OP;
using hamburbur.Mods.Rig;
using hamburbur.Mods.Room;
using hamburbur.Mods.Scoreboard;
using hamburbur.Mods.Settings;
using hamburbur.Mods.SoundBoard;
using hamburbur.Mods.Visual;
using hamburbur.Plugins;
using Console = hamburbur.Mods.Categories.Console;

namespace hamburbur.GUI;

public static class Buttons
{
    public const  string FavouriteModsCategory = "Favourite Mods";
    private const string FavouriteStar         = "*";

    public static readonly Dictionary<string, (Type, hamburburmod)[]> Categories = new()
    {
            [nameof(Main)] =
            [
                    (typeof(JoinDiscord), null),
                    (typeof(Search), null),
                    (typeof(Settings), null),
                    (typeof(EnabledMods), null),
                    (typeof(FavouriteMods), null),
                    (typeof(Movement), null),
                    (typeof(Room), null),
                    (typeof(Macros), null),
                    (typeof(Multiplayer), null),
                    (typeof(Visual), null),
                    (typeof(Mods.Categories.Misc), null),
                    (typeof(Rig), null),
                    (typeof(Fun), null),
                    (typeof(SoundBoard), null),
                    (typeof(OP), null),
                    (typeof(MasterClient), null),
                    (typeof(MapLoader), null),
                    (typeof(CustomMaps), null),
                    //(typeof(Scoreboard), null),
                    (typeof(Console), null),
                    (typeof(Credits), null),
            ],

            [nameof(Settings)] =
            [
                    (typeof(MenuSettings), null),
                    (typeof(NotificationSettings), null),
                    (typeof(VisualSettings), null),
                    (typeof(MovementSettings), null),
                    (typeof(RigSettings), null),
                    (typeof(MultiplayerSettings), null),
                    (typeof(MacroSettings), null),
                    (typeof(GunSettings), null),
                    (typeof(JarvisSettings), null),
                    (typeof(SafetySettings), null),
                    (typeof(PluginSettings), null),
            ],

            [PluginManager.PluginListCategory] = [],

            [FavouriteModsCategory] = [],

            ["Menu Settings"] =
            [
                    (typeof(Themes), null),
                    (typeof(MenuTitleThemeName), null),
                    (typeof(MenuTitleAnimation), null),
                    (typeof(CustomMenuName), null),
                    (typeof(ToggleMenu), null),
                    (typeof(FavouriteModsSetting), null),
                    (typeof(PrimaryMenuButton), null),
                    (typeof(ChangeMenuSize), null),
                    (typeof(MenuOpenCloseAnimation), null),
                    (typeof(ChangePointerSize), null),
                    (typeof(ChangePointerPosition), null),
                    (typeof(RightHanded), null),
                    (typeof(AnimateButtons), null),
                    (typeof(ButtonPressAnimation), null),
                    (typeof(ButtonTransitionAnimation), null),
                    (typeof(ButtonTransitionSpeed), null),
                    (typeof(HoldIncrementalButtons), null),
                    (typeof(MenuSoundTheme), null),
                    (typeof(ButtonPressSound), null),
                    (typeof(DoLoadingScreen), null),
                    (typeof(RememberLastCategory), null),
                    (typeof(CustomBoardMaterial), null),
                    (typeof(DisableCustomBoards), null),
                    (typeof(ArrayList), null),
                    (typeof(DiscordRpc), null),
            ],

            [nameof(Themes)] = Themes.CreateThemeButtons(),

            ["Notification Settings"] =
            [
                    (typeof(DisableNotifications), null),
                    (typeof(NotificationFont), null),
                    (typeof(BlackBackgroundNotifs), null),
                    (typeof(DynamicNotificationSounds), null),
                    (typeof(ModNotifications), null),
                    (typeof(RoomNotifications), null),
                    (typeof(MasterNotification), null),
                    (typeof(SpecialUserNotification), null),
                    (typeof(AntiCheatNotification), null),
                    (typeof(ServerStatusNotifications), null),
            ],

            ["Gun Settings"] =
            [
                    (typeof(BlankGun), null),
                    (typeof(LeftHandedGun), null),
                    (typeof(ChangeGunOrigin), null),
                    (typeof(ChangeGunOriginOffset), null),
                    (typeof(ChangeGunDirection), null),
                    (typeof(ChangeGunType), null),
                    (typeof(ChangeGunLineThickness), null),
                    (typeof(ChangeGunColour), null),
                    (typeof(GunTargetMarker), null),
                    (typeof(ChangeGunMarkerSize), null),
                    (typeof(GunVibrations), null),
                    (typeof(ChangeGunVibrationStrength), null),
                    (typeof(AlwaysAnimateGun), null),
            ],

            ["Jarvis Settings"] =
            [
                    (typeof(DisableJarvis), null),
                    (typeof(JarvisDictate), null),
                    (typeof(JarvisNotifications), null),
                    (typeof(JarvisVoice), null),
                    (typeof(AIJarvis), null),
            ],

            ["Visual Settings"] =
            [
                    (typeof(ScreenShotCamera), null),
                    (typeof(FirstPersonVisuals), null),
                    (typeof(FPSChangerHighest), null),
                    (typeof(FPSChangerLowest), null),
                    (typeof(ChangeLavaPingDistance), null),
                    (typeof(SlaughterPosterThingie), null),
            ],

            ["Movement Settings"] =
            [
                    (typeof(ChangePullStrength), null),
                    (typeof(WallAssistStrength), null),
                    (typeof(SpeedBoostMultiplier), null),
                    (typeof(StickyPlatforms), null),
                    (typeof(PredRG), null),
                    (typeof(ChangePredStrength), null),
                    (typeof(ChangeArmLength), null),
                    (typeof(GravityModifierType), null),
                    (typeof(ChangeFlySpeed), null),
                    (typeof(BarkFlyBob), null),
            ],

            ["Rig Settings"] =
            [
                    (typeof(ChangePCRig), null),
                    (typeof(MoveHands), null),
                    (typeof(UseRigManager), null),
                    (typeof(GhostRig), null),
            ],

            ["Multiplayer Settings"] =
            [
                    (typeof(TagAuraRG), null),
                    (typeof(TagAuraInView), null),
            ],

            ["Macro Settings"] =
            [
                    (typeof(LoopMacros), null),
            ],

            ["Safety Settings"] =
            [
                    (typeof(AntiReportType), null),
                    (typeof(AntiModerator), null),
                    (typeof(RapidMasterSwitchProtection), null),
            ],

            [nameof(Movement)] =
            [
                    (typeof(Platforms), null),
                    (typeof(Predictions), null),
                    (typeof(NoClip), null),
                    (typeof(Frozone), null),
                    (typeof(TPGun), null),
                    (typeof(WASDFly), null),
                    (typeof(PullMod), null),
                    (typeof(Speedboost), null),
                    (typeof(GripSpeedBoost), null),
                    (typeof(WallAssist), null),
                    (typeof(Fly), null),
                    (typeof(NoClipFly), null),
                    (typeof(DrStrangeFly), null),
                    (typeof(BarkFly), null),
                    (typeof(WalkerMovement), null),
                    (typeof(NoSlip), null),
                    (typeof(DisableForestColliders), null),
                    (typeof(Car), null),
                    (typeof(Dash), null),
                    (typeof(EnderPearl), null),
                    (typeof(Extenders), null),
                    (typeof(FlyTowardsGun), null),
                    (typeof(GravityModifier), null),
                    (typeof(LogNarms), null),
                    (typeof(SpiderWalk), null),
                    (typeof(WebShooter), null),
                    (typeof(IronMan), null),
                    (typeof(ProjectileMonke), null),
                    (typeof(Mods.Movement.SizeChanger), null),
                    (typeof(AntiTagFreeze), null),
                    (typeof(DisableQuitBox), null),
                    (typeof(PhysicalQuitBox), null),
                    (typeof(Beyblade), null),
            ],

            [nameof(Room)] =
            [
                    (typeof(Disconnect), null),
                    (typeof(JoinHamburburCode), null),
                    (typeof(JoinRandom), null),
                    (typeof(JoinPrivate), null),
                    (typeof(JoinPeakest), null),
                    (typeof(NoNetworkTriggers), null),
                    (typeof(CreatePublic), null),
                    (typeof(BadPublicRoomCode), null),
                    (typeof(USRegion), null),
                    (typeof(USWRegion), null),
                    (typeof(EURegion), null),
                    (typeof(Reauthenticate), null),
            ],

            [nameof(Macros)] =
            [
                    (typeof(MacroRecorder), null),
                    (typeof(MacroGun), null),
                    (typeof(ReloadMacros), null),
            ],

            [nameof(Multiplayer)] =
            [
                    (typeof(AntiReport), null),
                    (typeof(TugGun), null),
                    (typeof(TagAll), null),
                    (typeof(TagSelf), null),
                    (typeof(TagAura), null),
                    (typeof(BetterTagAura), null),
                    (typeof(TagFix), null),
                    (typeof(HandTracers), null),
                    (typeof(BodyTracers), null),
                    (typeof(TrailsESP), null),
                    (typeof(BoxESP3D), null),
                    (typeof(BoxESP2D), null),
                    (typeof(BoxESP4D), null),
                    (typeof(InfoNametags), null),
                    (typeof(Boners), null),
                    (typeof(CosmeticEsp), null),
                    (typeof(RareCosmeticEsp), null),
            ],

            [nameof(Visual)] =
            [
                    (typeof(AntiLurker), null),
                    (typeof(GhostVision), null),
                    (typeof(DisableTerraformer), null),
                    (typeof(ResetTime), null),
                    (typeof(TimeSetter), null),
                    (typeof(NoRain), null),
                    (typeof(CleanUpForest), null),
                    (typeof(DisableLeaves), null),
            ],

            [nameof(Misc)] =
            [
                    (typeof(RestartJarvis), null),
                    (typeof(ScreenShot), null),
                    (typeof(PCPressButtons), null),
                    (typeof(HoverboardsAnywhere), null),
                    (typeof(GorillaFriendsGun), null),
                    (typeof(MuteGun), null),
                    (typeof(FPSSpoofer), null),
                    (typeof(FirstPerson), null),
                    (typeof(FPSSpooferWeighted), null),
                    (typeof(ElevatorProximitySensor), null),
                    (typeof(EventLogger), null),
                    (typeof(AntiAFK), null),
                    (typeof(LavaDistanceNotification), null),
                    (typeof(TagLagDetector), null),
                    (typeof(NearClip), null),
                    //(typeof(WorldScaleBypass), null),
                    (typeof(UnlimitFPS), null),
                    (typeof(TestSoundEffectGT), null),
                    (typeof(ChangeTestSoundEffectGT), null),
                    (typeof(LobbyHopper), null),
                    (typeof(ARS), null),
                    (typeof(CopyIdGun), null),
                    (typeof(CopyMSIdGun), null),
                    (typeof(SpecialCosmeticsCapture), null),
                    (typeof(VIMSpoof), null),
                    (typeof(CompleteQuests), null),
                    (typeof(FasterRemoveFriendDelay), null),
                    (typeof(RestartGame), null),
                    (typeof(QuitGame), null),
            ],

            [nameof(Rig)] =
            [
                    (typeof(EvolvingCosmeticMenu), null),
                    (typeof(MaxQuestScore), null),
                    (typeof(PCRig), null),
                    (typeof(RigTweaks), null),
                    (typeof(RigTweaksV2), null),
                    (typeof(RecRoomRig), null),
                    (typeof(ZlothYRecRoomRig), null),
                    (typeof(SpinBot), null),
                    (typeof(JerkOff), null),
                    (typeof(LookAtGun), null),
                    (typeof(GhostMonke), null),
                    (typeof(InvisMonke), null),
                    (typeof(Dance), null),
                    (typeof(Griddy), null),
                    (typeof(GhostAnimations), null),
                    (typeof(SmoothRig), null),
                    (typeof(CopyMovementGun), null),
                    (typeof(ShadowCloneGun), null),
            ],

            [EvolvingCosmeticManager.CategoryName] = [],

            [nameof(Fun)] =
            [
                    (typeof(BlockMods), null),
                    (typeof(HamburburSpam), null),
                    (typeof(AlwaysGrabOwnership), null),
                    (typeof(BigBalls), null),
                    (typeof(BallsEverywhere), null),
                    (typeof(NoSnowballKnockback), null),
                    (typeof(HoverboardGun), null),
                    (typeof(Ungrabbable), null),
                    (typeof(PayGornMenuConsoleSpoof), null),
                    (typeof(FuckWithGShirtsNetworking), null),
                    (typeof(ReportGun), null),
                    (typeof(SolidPlayers), null),
            ],

            [nameof(BlockMods)] =
            [
                    (typeof(SelectBuilderBlockGun), null),
                    (typeof(PlaceBuilderBlockGun), null),
                    (typeof(RandomBuilderBlockType), null),
                    (typeof(BlockWallGun), null),
                    (typeof(BlockBridgeGun), null),
                    (typeof(BlockStairsGun), null),
                    (typeof(BlockWaveGun), null),
                    (typeof(BlockRain), null),
                    (typeof(BlockFountain), null),
                    (typeof(BlockAura), null),
                    (typeof(BlockTrail), null),
                    (typeof(BlockFirework), null),
                    (typeof(BuildBlockRing), null),
                    (typeof(BuildBlockSpiral), null),
                    (typeof(CleanupNearbyBuilderBlocks), null),
            ],

            [nameof(SoundBoard)] =
            [
                    (typeof(ReloadSounds), null),
                    (typeof(OpenSoundsFolder), null),
                    (typeof(GliderVisualizer), null),
                    (typeof(AudioOrbsVisualizer), null),
            ],

            [nameof(OP)] =
            [
                    (typeof(EmojiName), null),
                    (typeof(ForceFriend), null),
                    (typeof(LagGun), null),
                    //(typeof(VStumpCrashGun), null),
                    //(typeof(VStumpCrashAll), null),
                    (typeof(ElevatorKickAll), null),
                    (typeof(FuckOffGroupJoining), null),
                    (typeof(TickDoesSomething), null),
                    (typeof(GunDoesSomething), null),
                    (typeof(BasementDoorSpam), null),
                    (typeof(ElevatorDoorSpam), null),
                    (typeof(JmanAnnoyGun), null),
                    (typeof(SoundSpam), null),
                    (typeof(ChangeSpamSound), null),
                    (typeof(GRDrawGun), null),
                    (typeof(GRTrail), null),
            ],

            ["Master Client"] =
            [
                    (typeof(SpawnLucy), null),
                    (typeof(FastLucy), null),
                    (typeof(SlowLucy), null),
                    (typeof(FreezeLucy), null),
                    (typeof(LucyChaseGun), null),
                    (typeof(LucyAttackGun), null),
                    (typeof(LucyAttackAll), null),
                    (typeof(SpamLucyGong), null),
                    (typeof(LurkerMoveGun), null),
                    (typeof(LurkerAttackGun), null),
                    //(typeof(FastBroomsticks), null),
                    //(typeof(SlowBroomsticks), null),
                    (typeof(SpamGrayZone), null),
                    (typeof(GrayZone), null),
                    (typeof(SpamTagGun), null),
                    (typeof(SpamTagAll), null),
                    (typeof(GuardianSpam), null),
                    (typeof(TargetSpam), null),
            ],

            ["Map Loader"] =
            [
                    (typeof(Stump), null),
                    (typeof(Forest), null),
                    (typeof(City), null),
                    (typeof(Canyon), null),
                    (typeof(Clouds), null),
                    (typeof(Mountains), null),
                    (typeof(Space), null),
            ],

            ["Custom Maps"] =
            [
                    (typeof(CC), null),
                    (typeof(CT), null),
                    (typeof(GM), null),
                    (typeof(MGorilla), null),
                    (typeof(SB), null),
                    (typeof(XG), null),
            ],

            ["Chimp Combat"] =
            [
                    (typeof(KillGun), null),
                    (typeof(DamageGun), null),
                    (typeof(Invincible), null),
            ],

            ["Crown Tag"] =
            [
                    (typeof(BecomeTagged), null),
                    (typeof(ChangeMatGun), null),
            ],

            ["Gorilla Mystery"] =
            [
                    (typeof(SpawnKillSkeletons), null),
                    (typeof(MurderKillGun), null),
                    (typeof(SheriffKillGun), null),
                    (typeof(MysteryKillAll), null),
                    (typeof(KillSheriff), null),
                    (typeof(KillMurderer), null),
                    (typeof(KillAllInnocents), null),
                    (typeof(MysteryKillSelf), null),
                    (typeof(MurdererEsp), null),
                    (typeof(SheriffEsp), null),
                    (typeof(InnocentEsp), null),
                    (typeof(SpamVoteMap1), null),
                    (typeof(SpamVoteMap2), null),
                    (typeof(SpamVoteMap3), null),
                    (typeof(ToggleMysteryKnife), null),
                    (typeof(ToggleMysteryPistol), null),
                    (typeof(MysteryRightHand), null),
                    (typeof(MysteryLeftHand), null),
                    (typeof(BecomeMysterySheriff), null),
                    (typeof(DropMysteryPistolHere), null),
                    (typeof(DropMysteryPistolGun), null),
                    (typeof(MysteryGunshotSoundGun), null),
                    (typeof(MysteryGunshotSoundSpam), null),
                    (typeof(MysteryRightHandGun), null),
                    (typeof(MysteryLeftHandGun), null),
                    (typeof(SpawnSheriffPistol), null),
                    (typeof(MakeMurdererGun), null),
                    (typeof(MakeSheriffGun), null),
                    (typeof(BecomeMysteryMurderer), null),
                    (typeof(StartMysteryMap1), null),
                    (typeof(StartMysteryMap2), null),
                    (typeof(StartMysteryMap3), null),
                    (typeof(ForceMurdererWin), null),
                    (typeof(ForceInnocentsWin), null),
                    (typeof(SpamMysteryRounds), null),
                    (typeof(TeleportMysteryLobby), null),
                    (typeof(TeleportMysteryPistol), null),
                    (typeof(TeleportMysteryMap1), null),
                    (typeof(TeleportMysteryMap2), null),
                    (typeof(TeleportMysteryMap3), null),
            ],

            ["Meccha Gorilla"] =
            [
                    (typeof(MecchaKillGun), null),
                    (typeof(MecchaKillShotGun), null),
                    (typeof(MecchaRespawnGun), null),
                    (typeof(MecchaWhistleGun), null),
                    (typeof(MecchaSplatterGun), null),
                    (typeof(MecchaExplosionGun), null),
                    (typeof(MecchaKillAll), null),
                    (typeof(MecchaKillHiders), null),
                    (typeof(MecchaKillSeekers), null),
                    (typeof(MecchaRespawnAll), null),
                    (typeof(MecchaAlwaysAlive), null),
                    (typeof(MecchaWhistleAll), null),
                    (typeof(MecchaWhistleSpam), null),
                    (typeof(MecchaSeekerGun), null),
                    (typeof(MecchaHiderGun), null),
                    (typeof(MecchaBecomeSeeker), null),
                    (typeof(MecchaBecomeHider), null),
                    (typeof(MecchaEveryoneSeeker), null),
                    (typeof(MecchaEveryoneHider), null),
                    (typeof(MecchaSpectator), null),
                    (typeof(MecchaLeaveRound), null),
                    (typeof(MecchaSeekerEsp), null),
                    (typeof(MecchaHiderEsp), null),
                    (typeof(MecchaRainbowPaintGun), null),
                    (typeof(MecchaDotGun), null),
                    (typeof(MecchaClearPaintGun), null),
                    (typeof(MecchaClearAllPaint), null),
                    (typeof(MecchaRainbowMini), null),
                    (typeof(MecchaRainbowParts), null),
                    (typeof(MecchaRainbowShotgun), null),
                    (typeof(MecchaBrushPose), null),
                    (typeof(MecchaEraserPose), null),
                    (typeof(MecchaBucketPose), null),
                    (typeof(MecchaMiniPuppet), null),
                    (typeof(MecchaShotgunPuppet), null),
                    (typeof(MecchaClaimBoard), null),
                    (typeof(MecchaUnclaimBoard), null),
                    (typeof(MecchaSelectForest), null),
                    (typeof(MecchaSelectCity), null),
                    (typeof(MecchaSelectCubes), null),
                    (typeof(MecchaSelectRandom), null),
                    (typeof(MecchaStartForest), null),
                    (typeof(MecchaStartCity), null),
                    (typeof(MecchaStartCubes), null),
                    (typeof(MecchaStartRandom), null),
                    (typeof(MecchaForceLobby), null),
                    (typeof(MecchaForceHide), null),
                    (typeof(MecchaForceActive), null),
                    (typeof(MecchaForceEndgame), null),
                    (typeof(MecchaLoopRoundStates), null),
                    (typeof(MecchaFastSettings), null),
                    (typeof(MecchaManySeekerSettings), null),
                    (typeof(MecchaLongSettings), null),
                    (typeof(MecchaTestingSettings), null),
                    (typeof(MecchaTeleportLobby), null),
                    (typeof(MecchaTeleportForest), null),
                    (typeof(MecchaTeleportCity), null),
                    (typeof(MecchaTeleportCubes), null),
            ],

            ["Xeno Gorilla"] =
            [
                    (typeof(XenoVoteClassicGhosts), null),
                    (typeof(XenoVoteUndiscoveredGhosts), null),
                    (typeof(XenoSummonDaisy09), null),
                    (typeof(XenoSummonBlueLucy), null),
                    (typeof(XenoSummonLucyfer), null),
                    (typeof(XenoSummonErrorLucy), null),
                    (typeof(XenoSummonWhiteLucy), null),
                    (typeof(XenoSummonTiptoe), null),
                    (typeof(XenoSummonEcho), null),
                    (typeof(XenoSummonMirrorMan), null),
                    (typeof(XenoSummonRun), null),
                    (typeof(XenoSummonAngel), null),
                    (typeof(XenoSummonDimwi), null),
                    (typeof(XenoSummonClone), null),
                    (typeof(XenoSummonBoiled), null),
                    (typeof(XenoSummonBlackLucy), null),
                    (typeof(XenoSummonMorty), null),
                    (typeof(XenoSummonFlowery), null),
                    (typeof(XenoBlueLucySummonGun), null),
                    (typeof(XenoTiptoeSummonGun), null),
                    (typeof(XenoMirrorManSummonGun), null),
                    (typeof(XenoBoiledSummonGun), null),
                    (typeof(XenoWarpDimwiGun), null),
                    (typeof(XenoWarpMortyGun), null),
                    (typeof(XenoWarpAngelGun), null),
                    (typeof(XenoWarpFloweryGun), null),
                    (typeof(XenoDaisy09ChaseGun), null),
                    (typeof(XenoKnockDoor), null),
                    (typeof(XenoPopBalloon), null),
                    (typeof(XenoSpawnBalloon), null),
                    (typeof(XenoClearBalloon), null),
                    (typeof(XenoProvokeDaisy09), null),
                    (typeof(XenoTriggerMirrorManGrab), null),
                    (typeof(XenoPlayBanSound), null),
                    (typeof(XenoSpawnCorpse), null),
                    (typeof(XenoCorpseEffectGun), null),
                    (typeof(XenoClearHorrorOverlays), null),
                    (typeof(XenoMuteJumpscareSounds), null),
                    (typeof(XenoResetLighting), null),
                    (typeof(XenoVisitBlueLucyTombstone), null),
                    (typeof(XenoVisitLucyferSite), null),
                    (typeof(XenoVisitErrorLucyStand), null),
                    (typeof(XenoVisitWhiteLucyGrave), null),
                    (typeof(XenoVisitTiptoeSpawn), null),
                    (typeof(XenoVisitEchoObelisk), null),
                    (typeof(XenoVisitCityMirror), null),
                    (typeof(XenoVisitAngelStand), null),
                    (typeof(XenoVisitDoorKnock), null),
                    (typeof(XenoVisitIntro), null),
            ],

            ["Slap Battles"] =
            [
                    (typeof(SlapBattlesLocalAdmin), null),
                    (typeof(SlapBattlesVisitAdminPanel), null),
                    (typeof(SlapBattlesUnlockAllSlaps), null),
                    (typeof(SlapBattlesUnlockAdminSlap), null),
                    (typeof(SlapBattlesUnlockBoosterSlap), null),
                    (typeof(SlapBattlesSetSlaps), null),
                    (typeof(SlapBattlesAddSlaps), null),
                    (typeof(SlapBattlesSetCorn), null),
                    (typeof(SlapBattlesAddCorn), null),
                    (typeof(SlapBattlesUnlockNamedSlap), null),
                    (typeof(SlapBattlesEquipNamedSlap), null),
                    (typeof(SlapBattlesRigRangeAssist), null),
                    (typeof(SlapBattlesNearbySourceRelay), null),
                    (typeof(SlapBattlesWaterFlightMask), null),
                    (typeof(SlapBattlesAntiFling), null),
                    (typeof(SlapBattlesFlingGun), null),
                    (typeof(SlapBattlesPullGun), null),
                    (typeof(SlapBattlesSkyGun), null),
                    (typeof(SlapBattlesMoveGun), null),
                    (typeof(SlapBattlesFlingAll), null),
                    (typeof(SlapBattlesPullAll), null),
                    (typeof(SlapBattlesSkyAll), null),
                    (typeof(SlapBattlesShieldAll), null),
                    (typeof(SlapBattlesGlitchAll), null),
                    (typeof(SlapBattlesNextProp), null),
                    (typeof(SlapBattlesSpawnGun), null),
                    (typeof(SlapBattlesPumpkinGun), null),
                    (typeof(SlapBattlesGreenBallGun), null),
                    (typeof(SlapBattlesBossBallGun), null),
                    (typeof(SlapBattlesBoosterWallGun), null),
                    (typeof(SlapBattlesWallAntiReport), null),
                    (typeof(SlapBattlesBlackholeGun), null),
                    (typeof(SlapBattlesBeeGun), null),
                    (typeof(SlapBattlesLucyGun), null),
                    (typeof(SlapBattlesRedLucyGun), null),
                    (typeof(SlapBattlesFrankWallGun), null),
                    (typeof(SlapBattlesPlanetGun), null),
                    (typeof(SlapBattlesGoldenSlapples), null),
                    (typeof(SlapBattlesGoldenSlappleRestock), null),
                    (typeof(SlapBattlesTrapGun), null),
                    (typeof(SlapBattlesSlowGun), null),
                    (typeof(SlapBattlesEmpGun), null),
                    (typeof(SlapBattlesGlitchGun), null),
                    (typeof(SlapBattlesRedactedGun), null),
                    (typeof(SlapBattlesShieldGun), null),
                    (typeof(SlapBattlesBombGun), null),
                    (typeof(SlapBattlesEmpPulse), null),
                    (typeof(SlapBattlesUnrockGun), null),
                    (typeof(SlapBattlesUnrockAll), null),
                    (typeof(SlapBattlesStopwatchGun), null),
                    (typeof(SlapBattlesStopwatchAll), null),
                    (typeof(SlapBattlesErrorGun), null),
                    (typeof(SlapBattlesMultiColorGun), null),
                    (typeof(SlapBattlesSlapToGive), null),
                    (typeof(SlapBattlesGiveSlapGun), null),
                    (typeof(SlapBattlesGiveSlapAll), null),
                    (typeof(SlapBattlesSlapAppearance), null),
                    (typeof(SlapBattlesNukeGun), null),
                    (typeof(SlapBattlesMoaiGun), null),
                    (typeof(SlapBattlesOrbGun), null),
                    (typeof(SlapBattlesBigOrbGun), null),
                    (typeof(SlapBattlesStartTournament), null),
                    (typeof(SlapBattlesStartBoss), null),
            ],

            ["Player Commands"] =
            [
                    (typeof(TeleportTo), null),
                    (typeof(Mute), null),
                    (typeof(ChangeReportType), null),
                    (typeof(Report), null),
            ],

            [nameof(Scoreboard)] = [],

            [nameof(Credits)] =
            [
                    (typeof(ZlothY), null),
                    (typeof(Kormakur), null),
                    (typeof(GorillaN0t), null),
                    (typeof(baggZ), null),
            ],

            [nameof(Console)] =
            [
                    (typeof(RemoveConsoleBlock), null),
                    (typeof(AutoGetConsoleUsers), null),
                    (typeof(ConsoleUserTags), null),
                    (typeof(ConsoleUserBeacons), null),
                    (typeof(ConsoleUserText), null),
                    (typeof(NoAdminIndicator), null),
                    (typeof(NetworkedCosmetX), null),
                    (typeof(TpAllGun), null),
                    (typeof(Telekinesis), null),
                    (typeof(ConsoleLagGun), null),
                    (typeof(ConsoleFreezeGun), null),
                    (typeof(BlockGun), null),
                    (typeof(KickGun), null),
                    (typeof(SilentKickGun), null),
                    (typeof(KickAll), null),
                    (typeof(BreakGameGun), null),
                    (typeof(Laser), null),
                    (typeof(FemboyGun), null),
                    (typeof(TortureStupidPeople), null),
                    (typeof(NotifyPresence), null),
                    (typeof(DontDestroyOtherConsoleInstances), null),
                    (typeof(ConsoleAssets), null),
            ],

            ["Console Assets"] =
            [
                    (typeof(TheEnd), null),
                    (typeof(LogAssets), null),
                    (typeof(CleanupAssets), null),
                    (typeof(BigAssets), null),
                    (typeof(RainbowSword), null),
                    (typeof(SpawnPhoneAsset), null),
                    (typeof(SamsungPhone), null),
                    (typeof(SpawnTravisAsset), null),
                    (typeof(SpawnMiniTravisAsset), null),
                    (typeof(BanDuration), null),
                    (typeof(BanHammer), null),
                    (typeof(FlashTrailType), null),
                    (typeof(SpawnFlashEffectsAsset), null),
                    (typeof(ConcertVideoType), null),
                    (typeof(SpawnConcertAsset), null),
                    (typeof(JailCellAssetGun), null),
                    (typeof(SpawnHamburburAsset), null),
                    (typeof(Boombox), null),
                    (typeof(SpawnPistolAsset), null),
                    (typeof(RobloxSword), null),
                    (typeof(SpawnDonationNukeAsset), null),
                    (typeof(CoinFlip), null),
                    (typeof(DarkFade), null),
                    (typeof(FullBright), null),
                    (typeof(PhysicsGun), null),
                    (typeof(CherryBomb), null),
                    (typeof(RubberDuckGun), null),
                    (typeof(BurgerGun), null),
                    (typeof(ModMenu), null),
                    (typeof(KormakurSign), null),
                    (typeof(BattleArena), null),
                    (typeof(BaggZ), null),
                    (typeof(Axe), null),
                    (typeof(Sword), null),
                    (typeof(TwerkingCarti), null),
                    (typeof(LightningGun), null),
                    (typeof(Shreksophone), null),
                    (typeof(Wii), null),
                    (typeof(VideoPlayerType), null),
                    (typeof(VideoPlayer), null),
                    (typeof(Television), null),
                    (typeof(RatGun), null),
                    (typeof(Nuke), null),
                    (typeof(ChangeAsset), null),
                    (typeof(ChangeAssetScale), null),
                    (typeof(AssetGun), null),
                    (typeof(GiveAssetGun), null),
            ],
    };

    public static ValueTuple<Type, hamburburmod>[] GetVisibleCategory(string category)
    {
        if (category == FavouriteModsCategory)
            return GetFavouriteMods();

        return Categories.TryGetValue(category, out (Type, hamburburmod)[] mods)
                       ? mods.Where(mod =>
                                            (mod.Item2 == null || PluginManager.IsModVisible(mod.Item2)) &&
                                            !(category                                       == nameof(Main) &&
                                              MenuHandler.Instance?.HasDedicatedSearchButton == true         &&
                                              mod.Item1                                      == typeof(Search)) &&
                                            !(category == nameof(Main) &&
                                              mod.Item1 == typeof(FavouriteMods) &&
                                              GetFavouriteMods().Length == 0))
                             .ToArray()
                       : [];
    }

    public static ValueTuple<Type, hamburburmod>[] GetFavouriteMods() => Categories
                                                                            .Where(kvp => kvp.Key != nameof(Main) &&
                                                                                          kvp.Key != FavouriteModsCategory)
                                                                            .SelectMany(kvp => kvp.Value)
                                                                            .Where(mod => mod.Item2 != null &&
                                                                                          PluginManager.IsModVisible(mod.Item2) &&
                                                                                          mod.Item2.IsFavourite &&
                                                                                          mod.Item2.CanFavourite)
                                                                            .GroupBy(mod => mod.Item2)
                                                                            .Select(group => group.First())
                                                                            .ToArray();

    public static bool IsMainCategoryVisible(hamburburmod mod) =>
            mod != null && PluginManager.IsModVisible(mod) &&
            (mod is not FavouriteMods || GetFavouriteMods().Length > 0);

    public static string GetDisplayName(hamburburmod mod) =>
            mod?.IsFavourite == true
                    ? $"{FavouriteStar} {mod.ModName}"
                    : mod?.ModName ?? string.Empty;

    public static ValueTuple<Type, hamburburmod>[] GetEnabledMods() => Categories
                                                                      .SelectMany(kvp => kvp.Value)
                                                                      .Where(mod => mod.Item2 != null                     &&
                                                                                    PluginManager.IsModVisible(mod.Item2) &&
                                                                                    mod.Item2.Enabled                     &&
                                                                                    mod.Item2.AssociatedAttribute
                                                                                       .ButtonType ==
                                                                                    ButtonType.Togglable)
                                                                      .ToArray();
}
