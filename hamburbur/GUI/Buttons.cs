using System;
using System.Collections.Generic;
using System.Linq;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Categories;
using hamburbur.Mods.Console;
using hamburbur.Mods.Console.Assets;
using hamburbur.Mods.Credits;
using hamburbur.Mods.Credits.Contributors;
using hamburbur.Mods.CustomMaps;
using hamburbur.Mods.CustomMaps.ChimpCombat;
using hamburbur.Mods.CustomMaps.CrownTag;
using hamburbur.Mods.Fun;
using hamburbur.Mods.Macros;
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
using Console = hamburbur.Mods.Categories.Console;

namespace hamburbur.GUI;

public static class Buttons
{
    public static readonly Dictionary<string, (Type, hamburburmod)[]> Categories = new()
    {
            ["Main"] =
            [
                    (typeof(JoinDiscord), null),
                    (typeof(Search), null),
                    (typeof(Settings), null),
                    (typeof(EnabledMods), null),
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
                    (typeof(CustomMaps), null),
                    (typeof(Scoreboard), null),
                    (typeof(Console), null),
                    (typeof(Credits), null),
            ],

            ["Settings"] =
            [
                    (typeof(Themes), null),
                    (typeof(ArrayList), null),
                    (typeof(DiscordRpc), null),
                    (typeof(DynamicMenuSounds), null),
                    (typeof(DynamicNotificationSounds), null),
                    (typeof(ButtonPressSound), null),
                    (typeof(RightHanded), null),
                    (typeof(DoLoadingScreen), null),
                    (typeof(DisableJarvis), null),
                    (typeof(JarvisSpeak), null),
                    (typeof(JarvisNotifications), null),
                    (typeof(ModNotifications), null),
                    (typeof(RoomNotifications), null),
                    (typeof(JarvisVoice), null),
                    (typeof(GPTJarvis), null),
                    (typeof(AntiReportType), null),
                    (typeof(ScreenShotCamera), null),
                    (typeof(FirstPersonVisuals), null),
                    (typeof(ChangePullStrength), null),
                    (typeof(WallAssistStrength), null),
                    (typeof(SpeedBoostMultiplier), null),
                    (typeof(ChangeGunType), null),
                    (typeof(NotificationFont), null),
                    (typeof(AlwaysAnimateGun), null),
                    (typeof(ChangePCRig), null),
                    (typeof(FPSChangerHighest), null),
                    (typeof(FPSChangerLowest), null),
                    (typeof(ChangeLavaPingDistance), null),
                    (typeof(TagAuraRG), null),
                    (typeof(MasterNotification), null),
                    (typeof(MoveHands), null),
                    (typeof(StickyPlatforms), null),
                    (typeof(PredRG), null),
                    (typeof(ChangePredStrength), null),
                    (typeof(ServerStatusNotifications), null),
                    (typeof(ChangeArmLength), null),
                    (typeof(GravityModifierType), null),
                    (typeof(ChangeFlySpeed), null),
                    (typeof(BlackBackgroundNotifs), null),
                    (typeof(ToggleMenu), null),
                    (typeof(ChangeMenuSize), null),
                    (typeof(ChangePointerSize), null),
            ],

            ["Movement"] =
            [
                    (typeof(Platforms), null),
                    (typeof(Predictions), null),
                    (typeof(NoClip), null),
                    (typeof(Frozone), null),
                    (typeof(TPGun), null),
                    (typeof(WASDFly), null),
                    (typeof(PullMod), null),
                    (typeof(Speedboost), null),
                    (typeof(WallAssist), null),
                    (typeof(Fly), null),
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
            ],

            ["Room"] =
            [
                    (typeof(JoinHamburburCode), null),
                    (typeof(JoinPeakest), null),
                    (typeof(NoNetworkTriggers), null),
                    (typeof(CreatePublic), null),
                    (typeof(BadPublicRoomCode), null),
            ],

            ["Macros"] =
            [
                    (typeof(MacroRecorder), null),
                    (typeof(MacroGun), null),
                    (typeof(ReloadMacros), null),
            ],

            ["Multiplayer"] =
            [
                    (typeof(AntiReport), null),
                    (typeof(TugGun), null),
                    (typeof(TagAll), null),
                    (typeof(TagAura), null),
                    (typeof(BetterTagAura), null),
                    (typeof(HandTracers), null),
                    (typeof(BodyTracers), null),
                    (typeof(BoxESP3D), null),
                    (typeof(BoxESP2D), null),
                    (typeof(BoxESP4D), null),
                    (typeof(Boners), null),
            ],

            ["Visual"] =
            [
                    //(typeof(GhostVision), null),
                    (typeof(ResetTime), null),
                    (typeof(TimeSetter), null),
                    (typeof(Rain), null),
                    (typeof(NoRain), null),
                    (typeof(CleanUpForest), null),
            ],

            ["Misc"] =
            [
                    (typeof(RestartJarvis), null),
                    (typeof(ScreenShot), null),
                    (typeof(PCPressButtons), null),
                    (typeof(MonkeClick), null),
                    (typeof(HoverboardsAnywhere), null),
                    (typeof(GorillaFriendsGun), null),
                    (typeof(MuteGun), null),
                    (typeof(FPSSpoofer), null),
                    (typeof(FirstPerson), null),
                    (typeof(FPSSpooferWeighted), null),
                    (typeof(ElevatorProximitySensor), null),
                    (typeof(EventLogger), null),
                    //(typeof(AntiLurker), null),
                    (typeof(AntiAFK), null),
                    (typeof(LavaDistanceNotification), null),
                    (typeof(TagLagDetector), null),
                    (typeof(NearClip), null),
                    //(typeof(WorldScaleBypass), null),
                    (typeof(UnlimitFPS), null),
                    (typeof(TestSoundEffectGT), null),
                    (typeof(ChangeTestSoundEffectGT), null),
                    (typeof(ForceEnableHands), null),
                    (typeof(LobbyHopper), null),
                    (typeof(ARS), null),
                    (typeof(CopyIdGun), null),
                    (typeof(SpecialCosmeticsCapture), null),
            ],

            ["Rig"] =
            [
                    (typeof(PCRig), null),
                    (typeof(RigTweaks), null),
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
            ],

            ["Fun"] =
            [
                    (typeof(HamburburSpam), null),
                    (typeof(AlwaysGrabOwnership), null),
                    (typeof(BigBalls), null),
                    (typeof(BallsEverywhere), null),
                    (typeof(NoSnowballKnockback), null),
                    (typeof(HoverboardGun), null),
                    (typeof(Ungrabbable), null),
                    (typeof(PayGornMenuConsoleSpoof), null),
                    //(typeof(FuckWithGShirtsNetworking), null),
                    //(typeof(FuckWithGChatBoxNetworking), null),
                    (typeof(ReportGun), null),
            ],

            ["SoundBoard"] =
            [
                    (typeof(GliderVisualizer), null),
            ],

            ["OP"] =
            [
                    (typeof(EmojiName), null),
                    (typeof(ElevatorKickAll), null),
                    (typeof(FuckOffGroupJoining), null),
                    (typeof(TickDoesSomething), null),
                    (typeof(GunDoesSomething), null),
                    (typeof(BasementDoorSpam), null),
                    (typeof(ElevatorDoorSpam), null),
                    (typeof(JmanAnnoyGun), null),
                    (typeof(SoundSpam), null),
                    (typeof(ChangeSpamSound), null),
            ],

            ["Master Client"] =
            [
                    (typeof(SpamGrayZone), null),
                    (typeof(GrayZone), null),
                    (typeof(SpamTagGun), null),
            ],

            ["Custom Maps"] =
            [
                    (typeof(CC), null),
                    (typeof(CT), null),
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

            ["Player Commands"] =
            [
                    (typeof(TeleportTo), null),
                    (typeof(Mute), null),
                    (typeof(ChangeReportType), null),
                    (typeof(Report), null),
            ],

            ["Scoreboard"] = [],

            ["Credits"] =
            [
                    (typeof(ZlothY), null),
                    (typeof(GorillaN0t), null),
                    (typeof(Contributors), null),
            ],

            ["Contributors"] =
            [
                    (typeof(baggZ), null),
            ],
            
            ["Console"] =
            [
                    (typeof(RemoveConsoleBlock), null),
                    (typeof(AutoGetConsoleUsers), null),
                    (typeof(ConsoleUserTags), null),
                    (typeof(ConsoleUserBeacons), null),
                    (typeof(ConsoleUserText), null),
                    (typeof(NoAdminIndicator), null),
                    (typeof(NetworkedCosmetX), null),
                    (typeof(TpAllGun), null),
                    (typeof(ConsoleLagGun), null),
                    (typeof(BlockGun), null),
                    (typeof(KickGun), null),
                    (typeof(KickAll), null),
                    (typeof(BreakGameGun), null),
                    (typeof(Laser), null),
                    (typeof(FemboyGun), null),
                    (typeof(TortureStupidPeople), null),
                    (typeof(ConsoleAssets), null),
            ],

            ["Console Assets"] =
            [
                    (typeof(LogAssets), null),
                    (typeof(CleanupAssets), null),
                    (typeof(BigAssets), null),
                    (typeof(RainbowSword), null),
                    (typeof(SpawnPhoneAsset), null),
                    (typeof(SpawnTravisAsset), null),
                    (typeof(SpawnMiniTravisAsset), null),
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
                    //(typeof(Btools), null),
                    (typeof(CoinFlip), null),
                    (typeof(DarkFade), null),
                    (typeof(FullBright), null),
                    (typeof(PhysicsGun), null),
                    (typeof(CherryBomb), null),
                    //(typeof(BrSword), null),
                    //(typeof(Karambit), null),
                    //(typeof(LeviathanAxe), null),
                    (typeof(RubberDuckGun), null),
                    (typeof(BurgerGun), null),
                    (typeof(ModMenu), null),
                    (typeof(KormakurSign), null),
                    (typeof(BattleArena), null),
                    (typeof(BaggZ), null),
                    (typeof(Axe), null),
                    (typeof(Sword), null),
                    (typeof(TwerkingCarti), null),
            ],
    };

    public static ValueTuple<Type, hamburburmod>[] GetEnabledMods() => Categories
                                                                      .SelectMany(kvp => kvp.Value)
                                                                      .Where(mod => mod.Item2.Enabled &&
                                                                                 mod.Item2.AssociatedAttribute
                                                                                        .ButtonType ==
                                                                                 ButtonType.Togglable)
                                                                      .ToArray();
}