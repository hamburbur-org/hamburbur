using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using GorillaLocomotion;
using GorillaNetworking;
using hamburbur.GUI;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Rig;
using hamburbur.Mods.Settings;
using hamburbur.Plugins;
using hamburbur.Server_Api_Communicator;
using hamburbur.Tools;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace hamburbur;

public class Plugin : MonoBehaviour
{
    public static string BeeMovieScript;
    public        bool   MenuLoaded;
    public        bool   JarvisDidFirstInitialisation;

    public bool PlayedStartAnim;

    public readonly Color MainColour = new(0.1694782f, 0.1504984f, 0.3584906f);

    public readonly Vector3    MenuLocalPositionLeft  = new(0.06f, 0.05f, 0.06f);
    public readonly Vector3    MenuLocalPositionRight = new(-0.06f, 0.05f, 0.06f);
    public readonly Quaternion MenuLocalRotationLeft  = Quaternion.Euler(315f, 345f, 180f);
    public readonly Quaternion MenuLocalRotationRight = Quaternion.Euler(315f, 15f,  0f);

    public readonly Color SecondaryColour = new(0.03906193f, 0.0252314f, 0.1981132f);

    public readonly Dictionary<string, string> SpecialCosmetics         = new();
    public readonly Dictionary<string, string> SpecialCosmeticsDetailed = new();
    private         int                        amountOfMods;
    private         TextSnapshot               cocHeadingSnapshot;

    private TextMeshPro                 cocHeadingText;
    private TextMeshPro                 cocText;
    private TextSnapshot                cocTextSnapshot;
    private PlayFabTitleDataTextDisplay cocTitleDataDisplay;
    private bool                        cocTitleDataDisplayEnabled;
    private bool                        customBoardTextEnabled = true;
    private string                      fpsText;

    private float gtPlayerControllerToRealRatio;

    private float lastFpsUpdate;

    private       AudioSource                 menuAudioSource;
    private       TextSnapshot                motdBodySnapshot;
    private       TextMeshPro                 motdBodyText;
    private       TextSnapshot                motdHeadingSnapshot;
    private       TextMeshPro                 motdHeadingText;
    private       PlayFabTitleDataTextDisplay motdTitleDataDisplay;
    private       bool                        motdTitleDataDisplayEnabled;
    private       bool                        versionOkay;
    public static Plugin                      Instance { get; private set; }

    public AssetBundle HamburburBundle { get; private set; }
    public GameObject  ComponentHolder { get; private set; }
    public AudioClip   HamburgerSound  { get; private set; }

    public TMP_FontAsset DiloWorldFont { get; private set; }

    public Texture2D HamburburIcon { get; private set; }
    public Texture2D ErrorIcon     { get; private set; }

    public Camera FirstPersonCamera { get; private set; }
    public Camera ThirdPersonCamera { get; private set; }

    public GameObject GnomePrefab { get; private set; }

    public bool IsSteam { get; private set; }

    public Material MainMaterial      { get; private set; }
    public Material SecondaryMaterial { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        if (!PatchManager.PatchAll())
            return;

        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
    }

    private void LateUpdate()
    {
        if (!versionOkay)
            return;

        if (!MenuLoaded)
            return;

        if (customBoardTextEnabled)
        {
            EnsureCustomBoardTextStyle();

            if (cocText != null)
            {
                bool inRoom = PhotonNetwork.InRoom;

                if (lastFpsUpdate + 0.1f < Time.time)
                {
                    lastFpsUpdate = Time.time;
                    int    fps    = Mathf.RoundToInt(1f / Time.smoothDeltaTime);
                    string colour = fps > 60 ? fps > 72 ? "green" : "yellow" : "red";
                    fpsText = $"<color={colour}>{fps}</color>";
                }

                string roomCode       = inRoom ? PhotonNetwork.CurrentRoom.Name : "NaN";
                string peopleInCode   = inRoom ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "NaN";
                string maxInCode      = inRoom ? PhotonNetwork.CurrentRoom.MaxPlayers.ToString() : "NaN";
                string gameModeString = inRoom ? NetworkSystem.Instance.GameModeString : "NaN";
                string ping           = inRoom ? PhotonNetwork.GetPing().ToString() : "NaN";
                cocText.text =
                        $"<size=150%><b>Welcome to hamburbur {(NetworkSystem.Instance.LocalPlayer.SanitizedNickName.IsNullOrEmpty() ? "" : NetworkSystem.Instance.LocalPlayer.SanitizedNickName)}!</b></size>\n\n<size=125%><b>Room Stats</b></size>\nFPS: {fpsText}\nRoom Code: {roomCode}\nPeople In Code: {peopleInCode}/{maxInCode}\nGameMode String: {gameModeString}\nPing: {ping}\n\n<size=125%><b>Menu Stats</b><size=125%>\nAmount Of Mods: {amountOfMods}\nMenu Build: {Constants.PluginVersion}\n{(Constants.BetaBuild ? "Beta Build" : "Release Build")}\n\n<size=75%>Made with <3 by ZlothY</size>";
            }
        }

        if (customBoardTextEnabled    &&
            motdBodyText      != null &&
            motdBodyText.text != HamburburOrgData.Data["messageOfTheDayText"].ToObject<string>())
            motdBodyText.text = HamburburOrgData.Data["messageOfTheDayText"].ToObject<string>();

        Transform realRight = Tools.Utils.RealRightController;
        Transform realLeft  = Tools.Utils.RealLeftController;

        float gtPlayerControllerScaleThingy = GTPlayer.Instance.rightHand.controllerTransform.lossyScale.magnitude *
                                              gtPlayerControllerToRealRatio;

        realRight.position = GTPlayer.Instance.rightHand.controllerTransform.position +
                             GTPlayer.Instance.rightHand.controllerTransform.rotation *
                             (GTPlayer.Instance.rightHand.handOffset * gtPlayerControllerScaleThingy);

        realRight.rotation = GTPlayer.Instance.rightHand.controllerTransform.rotation *
                             GTPlayer.Instance.rightHand.handRotOffset;

        realRight.localScale = gtPlayerControllerScaleThingy * Vector3.one;

        realLeft.position = GTPlayer.Instance.leftHand.controllerTransform.position +
                            GTPlayer.Instance.leftHand.controllerTransform.rotation *
                            (GTPlayer.Instance.leftHand.handOffset * gtPlayerControllerScaleThingy);

        realLeft.rotation = GTPlayer.Instance.leftHand.controllerTransform.rotation *
                            GTPlayer.Instance.leftHand.handRotOffset;

        realLeft.localScale = gtPlayerControllerScaleThingy * Vector3.one;
    }

    private void OnGameInitialized()
    {
        Debug.Log(Constants.HamburgerAscii + Constants.HamburburTextAscii + Constants.PluginDescription);

        PlatformTagJoin platform =
                (PlatformTagJoin)Traverse.Create(PlayFabAuthenticator.instance).Field(nameof(platform)).GetValue();

        IsSteam = platform.PlatformTag.Contains("Steam");

        Stream bundleStream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("hamburbur.Resources.hamburbur");

        HamburburBundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();

        MainMaterial      = new Material(Shaders.UberShader) { color = MainColour, };
        SecondaryMaterial = new Material(Shaders.UberShader) { color = SecondaryColour, };

        ComponentHolder = new GameObject("hamburbur components");
        ComponentHolder.AddComponent<CoroutineManager>();

        menuAudioSource              = ComponentHolder.AddComponent<AudioSource>();
        menuAudioSource.spatialBlend = 0f;
        menuAudioSource.playOnAwake  = false;

        HamburgerSound = HamburburBundle.LoadAsset<AudioClip>("hamburger");
        PlaySound(HamburgerSound);

        HamburburIcon = Tools.Utils.LoadEmbeddedImage("hamburbur.png");
        ErrorIcon     = Tools.Utils.LoadEmbeddedImage("error.png");

        FirstPersonCamera = GTPlayer.Instance.mainCamera;
        ThirdPersonCamera = GorillaTagger.Instance.thirdPersonCamera?.transform.GetChild(0)?.GetComponent<Camera>();

        if (PlayerPrefsExtensions.GetBool(DoLoadingScreen.PlayerPrefsKey, true))
        {
            GameObject loadingScreenHolder = new("hamburbur loading screen");
            loadingScreenHolder.AddComponent<LoadingScreenManager>();
        }
        else
        {
            DelayedStart();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void DelayedStart()
    {
        bool hasDoneDelayedStart = false;

        ComponentHolder.AddComponent<HamburburOrgData>();
        HamburburOrgData.OnDataReloaded += data =>
                                           {
                                               JObject cosmetics = data["specialCosmetics"]!
                                                      .ToObject<JObject>();

                                               foreach (JProperty prop in cosmetics.Properties())
                                                   SpecialCosmetics[prop.Name] = prop.Value.ToString();

                                               JObject cosmeticsDetailed = data["specialCosmeticsDetailed"]!
                                                      .ToObject<JObject>();

                                               foreach (JProperty prop in cosmeticsDetailed.Properties())
                                                   SpecialCosmeticsDetailed[prop.Name] = prop.Value.ToString();

                                               if (!Constants.BetaBuild)
                                               {
                                                   string hamburburStatus = (string)data[nameof(hamburburStatus)];
                                                   Version latestMenuVersion =
                                                           new((string)data[nameof(latestMenuVersion)] ?? string.Empty);

                                                   Version minimumMenuVersion =
                                                           new((string)data[nameof(minimumMenuVersion)] ??
                                                               string.Empty);

                                                   Version currentVersion = new(Constants.PluginVersion);

                                                   if (hamburburStatus != "Undetected")
                                                   {
                                                       HamburburPromotionManager.Instance.CreateStumpStatus(
                                                               $"Hamburbur currently isn't available.\nReason: {hamburburStatus}",
                                                               ErrorIcon);

                                                       return;
                                                   }

                                                   if (currentVersion < minimumMenuVersion)
                                                   {
                                                       HamburburPromotionManager.Instance.CreateStumpStatus(
                                                               $"You are using an outdated version of hamburbur.\nLatest version: {data[nameof(latestMenuVersion)]}\nMinimum version: {data[nameof(minimumMenuVersion)]}\nCurrent version: {Constants.PluginVersion}",
                                                               HamburburIcon);

                                                       return;
                                                   }

                                                   if (currentVersion < latestMenuVersion)
                                                       HamburburPromotionManager.Instance.CreateStumpStatus(
                                                               $"You are not on the latest version of hamburbur ({data[nameof(latestMenuVersion)]})\nYou are currently on version {Constants.PluginVersion}. We recommend updating.",
                                                               HamburburIcon);
                                               }

                                               versionOkay = true;

                                               if (hasDoneDelayedStart)
                                                   return;

                                               hasDoneDelayedStart = true;

                                               NetworkSystem.Instance.OnMasterClientSwitchedEvent += MasterNotification;
                                               NetworkSystem.Instance.OnJoinedRoomEvent +=
                                                       () => MasterNotification(null);

                                               GnomePrefab = HamburburBundle.LoadAsset<GameObject>("GNOME");

                                               if (GnomePrefab.TryGetComponent(out Renderer gnomeRenderer))
                                               {
                                                   gnomeRenderer.material.shader = Shaders.UberShader;
                                                   gnomeRenderer.material.EnableKeyword("_USE_TEXTURE");
                                               }

                                               using HttpClient httpClient = new();

                                               HttpResponseMessage beeMovieScriptValResponse = httpClient
                                                                                              .GetAsync(
                                                                                                       "https://gist.githubusercontent.com/MattIPv4/045239bc27b16b2bcf7a3a9a4648c08a/raw/2411e31293a35f3e565f61e7490a806d4720ea7e/bee%2520movie%2520script")
                                                                                              .Result;

                                               using Stream beeMovieScriptValStream = beeMovieScriptValResponse.Content
                                                                                                               .ReadAsStreamAsync().Result;

                                               using StreamReader beeMovieScriptValReader =
                                                       new(beeMovieScriptValStream);

                                               BeeMovieScript = beeMovieScriptValReader.ReadToEnd().Trim();

                                               motdBodyText = GameObject
                                                             .Find(
                                                                      "Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText")
                                                             .GetComponent<TextMeshPro>();

                                               motdHeadingText = GameObject
                                                                .Find(
                                                                         "Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText")
                                                                .GetComponent<TextMeshPro>();

                                               cocHeadingText = GameObject
                                                               .Find(
                                                                        "Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText")
                                                               .GetComponent<TextMeshPro>();

                                               cocText = GameObject
                                                        .Find(
                                                                 "Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData")
                                                        .GetComponent<TextMeshPro>();
                                               
                                               cocHeadingText.transform.localPosition = new Vector3(4.3226f,      -1.2771f, 6.3961f);
                                               cocText.transform.localPosition        = new Vector3(4.7613f, -1.3168f, 5.8447f);

                                               motdHeadingText.transform.localPosition = new Vector3(2.6652f,      -4.5832f, 8.2396f);
                                               motdBodyText.transform.localPosition    = new Vector3(1.8459f, -3.9933f, 7.9184f);

                                               motdBodySnapshot    = new TextSnapshot(motdBodyText);
                                               motdHeadingSnapshot = new TextSnapshot(motdHeadingText);
                                               cocHeadingSnapshot  = new TextSnapshot(cocHeadingText);
                                               cocTextSnapshot     = new TextSnapshot(cocText);

                                               motdTitleDataDisplay =
                                                       motdBodyText.GetComponent<PlayFabTitleDataTextDisplay>();

                                               cocTitleDataDisplay =
                                                       cocText.GetComponent<PlayFabTitleDataTextDisplay>();

                                               motdTitleDataDisplayEnabled =
                                                       motdTitleDataDisplay != null && motdTitleDataDisplay.enabled;

                                               cocTitleDataDisplayEnabled =
                                                       cocTitleDataDisplay != null && cocTitleDataDisplay.enabled;

                                               foreach (KeyValuePair<string, (Type, hamburburmod)[]> kvp in Buttons
                                                               .Categories)
                                                   amountOfMods += kvp.Value.Length;

                                               DiloWorldFont =
                                                       HamburburBundle.LoadAsset<TMP_FontAsset>("DiloWorld SDF");

                                               SetCustomBoardTextEnabled(!DisableCustomBoards.IsEnabled);

                                               gtPlayerControllerToRealRatio =
                                                       1 / GTPlayer.Instance.leftHand.controllerTransform.lossyScale
                                                                   .magnitude;

                                               Transform realRight = new GameObject("RealRightController").transform;
                                               Tools.Utils.RealRightController = realRight;

                                               Transform realLeft =
                                                       new GameObject("RealLeftController")
                                                              .transform;

                                               Tools.Utils.RealLeftController = realLeft;

                                               GameObject menuParent = new("hamburbur menu parent");

                                               menuParent.transform.SetParent(realLeft);
                                               menuParent.transform.localPosition = MenuLocalPositionLeft;
                                               menuParent.transform.localRotation = MenuLocalRotationLeft;
                                               menuParent.transform.localScale    = Vector3.one;

                                               GameObject menuPrefab =
                                                       HamburburBundle.LoadAsset<GameObject>("hamburburv2");

                                               Themes.ThemesDict["hamburburv2"] = menuPrefab;

                                               ComponentHolder.AddComponent<InputManager>();
                                               ComponentHolder.AddComponent<MenuSoundsHandler>();
                                               ComponentHolder.AddComponent<HamburburPromotionManager>();
                                               ComponentHolder.AddComponent<PlayerActivityNotifications>();
                                               ComponentHolder.AddComponent<SpecialUserActivityNotifications>();
                                               ComponentHolder.AddComponent<KeyboardManager>();
                                               ComponentHolder.AddComponent<VoiceControls>();
                                               ComponentHolder.AddComponent<AudioLib>();
                                               ComponentHolder.AddComponent<TagManager>();
                                               //ComponentHolder.AddComponent<PlayerAdderHandler>();
                                               ComponentHolder.AddComponent<PUNErrors>();
                                               ComponentHolder.AddComponent<RigUtils>();
                                               ComponentHolder.AddComponent<PingLogger>();
                                               ComponentHolder.AddComponent<AccountBanLogger>();
                                               ComponentHolder.AddComponent<TrackerManager>();
                                               ComponentHolder.AddComponent<Tools.Utils>();
                                               ComponentHolder.AddComponent<FileManager>();
                                               ComponentHolder.AddComponent<PluginManager>();
                                               ComponentHolder.AddComponent<MenuHandler>()
                                                              .SetUpMenu(menuPrefab,        menuParent.transform, Vector3.zero,
                                                                       Quaternion.identity, MainColour,           SecondaryColour,
                                                                       -0.29f,
                                                                       false, true);

                                               ComponentHolder.AddComponent<EvolvingCosmeticManager>();

                                               ComponentHolder.AddComponent<CustomBoardManager>();
                                               ComponentHolder.AddComponent<NotificationManager>();
                                               ComponentHolder.AddComponent<MenuWebsocket>();
                                               ComponentHolder.AddComponent<SeralythFriendManager>();
                                               ComponentHolder.AddComponent<GorillaFriendsUtils>();

                                               GorillaTagger.Instance.myRecorder.InputFactory =
                                                       () => VoiceManager.Get();

                                               GorillaTagger.Instance.myRecorder.SourceType =
                                                       Recorder.InputSourceType.Factory;

                                               GorillaTagger.Instance.myRecorder.RestartRecording();

                                               PlayedStartAnim = true;
                                           };
    }

    public void SetCustomBoardTextEnabled(bool enabled)
    {
        customBoardTextEnabled = enabled;

        if (motdBodyText == null || motdHeadingText == null || cocHeadingText == null || cocText == null)
            return;

        if (enabled)
        {
            ApplyCustomBoardText();

            return;
        }

        RestoreDefaultBoardText();
    }

    private void ApplyCustomBoardText()
    {
        if (motdTitleDataDisplay != null)
            motdTitleDataDisplay.enabled = false;

        if (cocTitleDataDisplay != null)
            cocTitleDataDisplay.enabled = false;

        motdBodyText.text    = HamburburOrgData.Data["messageOfTheDayText"].ToObject<string>();
        motdHeadingText.text = "Thank you for using hamburbur!";
        cocHeadingText.text  = "<size=175%><b>hamburbur</b></size>";

        motdBodyText.richText    = true;
        motdHeadingText.richText = true;
        cocHeadingText.richText  = true;
        cocText.richText         = true;

        ApplyCustomBoardTextStyle(motdBodyText);
        ApplyCustomBoardTextStyle(motdHeadingText);
        ApplyCustomBoardTextStyle(cocHeadingText);
        ApplyCustomBoardTextStyle(cocText);
    }

    private void RestoreDefaultBoardText()
    {
        motdBodySnapshot?.Restore(motdBodyText);
        motdHeadingSnapshot?.Restore(motdHeadingText);
        cocHeadingSnapshot?.Restore(cocHeadingText);
        cocTextSnapshot?.Restore(cocText);

        RefreshTitleDataDisplay(motdTitleDataDisplay, motdTitleDataDisplayEnabled);
        RefreshTitleDataDisplay(cocTitleDataDisplay,  cocTitleDataDisplayEnabled);
    }

    private void EnsureCustomBoardTextStyle()
    {
        if (cocHeadingText != null && cocHeadingText.text != "<size=175%><b>hamburbur</b></size>")
            cocHeadingText.text = "<size=175%><b>hamburbur</b></size>";

        ApplyCustomBoardTextStyle(motdBodyText);
        ApplyCustomBoardTextStyle(motdHeadingText);
        ApplyCustomBoardTextStyle(cocHeadingText);
        ApplyCustomBoardTextStyle(cocText);
    }

    private static void RefreshTitleDataDisplay(PlayFabTitleDataTextDisplay titleDataDisplay, bool wasEnabled)
    {
        if (titleDataDisplay == null)
            return;

        titleDataDisplay.enabled = wasEnabled;

        if (!wasEnabled)
            return;

        titleDataDisplay.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
    }

    private void ApplyCustomBoardTextStyle(TextMeshPro text)
    {
        if (text == null)
            return;

        text.font             = DiloWorldFont;
        text.characterSpacing = 1f;
        text.wordSpacing      = 1f;
        text.lineSpacing      = 1f;
    }

    private sealed class TextSnapshot(TextMeshPro target)
    {
        private readonly float         characterSpacing = target.characterSpacing;
        private readonly TMP_FontAsset font             = target.font;
        private readonly float         lineSpacing      = target.lineSpacing;
        private readonly bool          richText         = target.richText;
        private readonly string        text             = target.text;
        private readonly float         wordSpacing      = target.wordSpacing;

        public void Restore(TextMeshPro target)
        {
            if (target == null)
                return;

            target.text             = text;
            target.font             = font;
            target.richText         = richText;
            target.characterSpacing = characterSpacing;
            target.wordSpacing      = wordSpacing;
            target.lineSpacing      = lineSpacing;
        }
    }

    private static readonly Queue<float> MasterNotifTimes = new();

    private void MasterNotification(NetPlayer player)
    {
        if (RapidMasterSwitchProtection.IsEnabled)
        {
            float now = Time.realtimeSinceStartup;
            MasterNotifTimes.Enqueue(now);

            while (MasterNotifTimes.Count > 0 && now - MasterNotifTimes.Peek() > 3f)
                MasterNotifTimes.Dequeue();

            if (MasterNotifTimes.Count >= 5)
            {
                MasterNotifTimes.Clear();
                NetworkSystem.Instance.ReturnToSinglePlayer();
                NotificationManager.SendNotification(
                        "<color=red>Safety</color>",
                        "Master client switched too quickly, so you were disconnected",
                        5f,
                        true,
                        true);

                return;
            }
        }
        else
        {
            MasterNotifTimes.Clear();
        }

        if (!Mods.Settings.MasterNotification.IsEnabled || !Tools.Utils.IsMasterClient)
            return;

        NotificationManager.SendNotification(
                "<color=yellow>Room Activity</color>",
                "You are master client",
                8f,
                true,
                false);
    }

    public void PlayTutorialVideo(string videoUrl) => StartCoroutine(ZlothyStupid(videoUrl));

    private IEnumerator ZlothyStupid(string videoUrl)
    {
        while (GTPlayer.Instance == null)
            yield return null;

        GTPlayer.Instance.disableMovement = true;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "hamburbur Tutorial Video";

        quad.transform.position = GTPlayer.Instance.bodyCollider.transform.forward +
                                  GTPlayer.Instance.bodyCollider.transform.position;

        quad.transform.LookAt(GTPlayer.Instance.bodyCollider.transform);
        quad.transform.Rotate(0f, 180f, 0f);
        quad.transform.localScale = new Vector3(1.2f, 0.675f, 1f);

        quad.GetComponent<Collider>().Obliterate();

        VideoPlayer vp = quad.AddComponent<VideoPlayer>();
        vp.url             = videoUrl;
        vp.playOnAwake     = false;
        vp.audioOutputMode = VideoAudioOutputMode.None;

        RenderTexture rt = new(512, 512, 0);
        vp.targetTexture = rt;

        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Unlit/Texture"))
        {
                mainTexture = rt,
        };

        vp.loopPointReached += _ => quad.Obliterate();

        vp.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && menuAudioSource != null)
            menuAudioSource.PlayOneShot(clip);
    }
}