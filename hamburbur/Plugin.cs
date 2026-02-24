using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using GorillaLocomotion;
using GorillaNetworking;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Scoreboard;
using hamburbur.Mods.Settings;
using hamburbur.Server_API;
using hamburbur.Tools;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace hamburbur;

public class Plugin : MonoBehaviour
{
    public static string BeeMovieScript;
    public        bool   MenuLoaded;

    public bool PlayedStartAnim;

    public readonly Color MainColour = new(0.1694782f, 0.1504984f, 0.3584906f);

    public readonly Vector3 MenuLocalPositionLeft = Vector3.one * 0.06f;

    public readonly Vector3    MenuLocalPositionRight = new(-0.06f, 0.06f, 0.06f);
    public readonly Quaternion MenuLocalRotationLeft  = Quaternion.Euler(315f, 0f, 180f);
    public readonly Quaternion MenuLocalRotationRight = Quaternion.Euler(315f, 0f, 0f);

    public readonly Color SecondaryColour = new(0.03906193f, 0.0252314f, 0.1981132f);
    private         int   amountOfMods;

    private TextMeshPro cocHeadingText;
    private TextMeshPro cocText;

    private float gtPlayerControllerToRealRatio;

    private Harmony harmony;

    private AudioSource menuAudioSource;
    private TextMeshPro motdBodyText;

    private TextMeshPro motdHeadingText;

    private       GameObject rBall, lBall;
    private       GameObject stumpObj;
    private       bool       versionOkay;
    public static Plugin     Instance { get; private set; }

    public Shader      UberShader      { get; private set; }
    public AssetBundle HamburburBundle { get; private set; }
    public GameObject  ComponentHolder { get; private set; }
    public AudioClip   HamburgerSound  { get; private set; }

    public AudioClip ILikeFemboysSound { get; private set; }

    public TMP_FontAsset DiloWorldFont { get; private set; }

    public Texture2D HamburburIcon { get; private set; }
    public Texture2D ErrorIcon     { get; private set; }

    public Camera FirstPersonCamera { get; private set; }
    public Camera ThirdPersonCamera { get; private set; }

    public GameObject GnomePrefab            { get; private set; }
    public GameObject ConsoleIndicatorPrefab { get; private set; }

    public bool IsSteam { get; private set; }

    public Material MainMaterial      { get; private set; }
    public Material SecondaryMaterial { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        harmony = new Harmony(Constants.PluginGuid);
        harmony.PatchAll();

        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
    }

    private void LateUpdate()
    {
        if (!versionOkay)
            return;

        if (!MenuLoaded)
            return;

        if (cocText != null)
        {
            bool   inRoom         = PhotonNetwork.InRoom;
            int    fps            = Mathf.RoundToInt(1f / Time.smoothDeltaTime);
            string roomCode       = inRoom ? PhotonNetwork.CurrentRoom.Name : "NaN";
            string peopleInCode   = inRoom ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "NaN";
            string gameModeString = inRoom ? NetworkSystem.Instance.GameModeString : "NaN";
            string ping           = inRoom ? PhotonNetwork.GetPing().ToString() : "NaN";
            cocText.text =
                    $"<size=150%><b>Welcome to hamburbur {(NetworkSystem.Instance.LocalPlayer.SanitizedNickName.IsNullOrEmpty() ? "BADGORILLA" : NetworkSystem.Instance.LocalPlayer.SanitizedNickName)}!</b></size>\n\n<size=125%><b>Room Stats</b></size>\nFPS: {fps}\nRoom Code: {roomCode}\nPeople In Code: {peopleInCode}/10\nGameMode String: {gameModeString}\nPing: {ping}\n\n<size=125%><b>Menu Stats</b><size=125%>\nAmount Of Mods: {amountOfMods}\nMenu Build: {Constants.PluginVersion}\n{(Constants.BetaBuild ? "Beta Build" : "Release Build")}\n\n<size=75%>Made with <3 by HanSolo and ZlothY</size>";

            motdHeadingText.font = DiloWorldFont;
            motdBodyText.font    = DiloWorldFont;

            cocHeadingText.font = DiloWorldFont;
            cocText.font        = DiloWorldFont;

            const float CharacterSpacing = 1f;
            const float WordSpacing      = 1f;
            const float LineSpacing      = 1f;

            motdHeadingText.characterSpacing = CharacterSpacing;
            motdBodyText.characterSpacing    = CharacterSpacing;
            cocHeadingText.characterSpacing  = CharacterSpacing;
            cocText.characterSpacing         = CharacterSpacing;

            motdHeadingText.wordSpacing = WordSpacing;
            motdBodyText.wordSpacing    = WordSpacing;
            cocHeadingText.wordSpacing  = WordSpacing;
            cocText.wordSpacing         = WordSpacing;

            motdHeadingText.lineSpacing = LineSpacing;
            motdBodyText.lineSpacing    = LineSpacing;
            cocHeadingText.lineSpacing  = LineSpacing;
            cocText.lineSpacing         = LineSpacing;
        }
        
        if (motdBodyText != null && motdBodyText.text != HamburburData.Data["messageOfTheDayText"].ToObject<string>())
            motdBodyText.text = HamburburData.Data["messageOfTheDayText"].ToObject<string>();

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

        bool isRigEnabled = VRRig.LocalRig.enabled;
        rBall.SetActive(!isRigEnabled);
        lBall.SetActive(!isRigEnabled);
        if (!isRigEnabled && Tools.Utils.InVR)
            GorillaTagger.Instance.rightHandTriggerCollider.transform.position =
                    MenuHandler.Instance.ButtonPresser.transform.position;
    }

    private void OnGameInitialized()
    {
        Debug.Log(Constants.HamburgerAscii + Constants.HamburburTextAscii + Constants.PluginDescription);

        PlatformTagJoin platform =
                (PlatformTagJoin)Traverse.Create(PlayFabAuthenticator.instance).Field("platform").GetValue();

        IsSteam = platform.PlatformTag.Contains("Steam");

        Stream bundleStream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("hamburbur.Resources.hamburbur");

        HamburburBundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();

        // ReSharper disable once ShaderLabShaderReferenceNotResolved
        UberShader = Shader.Find("GorillaTag/UberShader");

        MainMaterial      = new Material(UberShader) { color = MainColour, };
        SecondaryMaterial = new Material(UberShader) { color = SecondaryColour, };

        ComponentHolder = new GameObject("hamburbur components");
        ComponentHolder.AddComponent<CoroutineManager>();

        menuAudioSource              = ComponentHolder.AddComponent<AudioSource>();
        menuAudioSource.spatialBlend = 0f;
        menuAudioSource.playOnAwake  = false;

        HamburgerSound = MenuSoundsHandler.LoadWavFromResource("hamburbur.Resources.hamburger.wav");
        PlaySound(HamburgerSound);

        HamburburIcon = LoadEmbeddedImage("hamburbur.Resources.hamburbur.png");
        ErrorIcon     = LoadEmbeddedImage("hamburbur.Resources.error.png");

        FirstPersonCamera = GTPlayer.Instance.mainCamera;
        ThirdPersonCamera = GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0).GetComponent<Camera>();

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

        ComponentHolder.AddComponent<HamburburData>();
        HamburburData.OnDataReloaded += data =>
                                        {
                                            if (hasDoneDelayedStart)
                                                return;

                                            hasDoneDelayedStart = true;

                                            if (!Constants.BetaBuild)
                                            {
                                                string hamburburStatus = (string)data["hamburburStatus"];
                                                Version latestMenuVersion =
                                                        new((string)data["latestMenuVersion"] ?? string.Empty);

                                                Version minimumMenuVersion =
                                                        new((string)data["minimumMenuVersion"] ?? string.Empty);

                                                Version currentVersion = new(Constants.PluginVersion);

                                                if (hamburburStatus != "Undetected")
                                                {
                                                    CreateStumpStatus(
                                                            $"Hamburbur currently isn't available.\nReason: {hamburburStatus}",
                                                            ErrorIcon);

                                                    return;
                                                }

                                                if (currentVersion < minimumMenuVersion)
                                                {
                                                    CreateStumpStatus(
                                                            $"You are using an outdated version of hamburbur.\nLatest version: {data["latestMenuVersion"]}\nMinimum version: {data["minimumMenuVersion"]}\nCurrent version: {Constants.PluginVersion}",
                                                            HamburburIcon);

                                                    return;
                                                }

                                                if (currentVersion < latestMenuVersion)
                                                    CreateStumpStatus(
                                                            $"You are not on the latest version of hamburbur ({data["latestMenuVersion"]})\nYou are currently on version {Constants.PluginVersion}. We recommend updating.",
                                                            HamburburIcon);
                                            }

                                            versionOkay = true;

                                            NetworkSystem.Instance.OnMasterClientSwitchedEvent += MasterNotification;
                                            NetworkSystem.Instance.OnJoinedRoomEvent += () => MasterNotification(null);

                                            GnomePrefab = HamburburBundle.LoadAsset<GameObject>("GNOME");

                                            if (GnomePrefab.TryGetComponent(out Renderer gnomeRenderer))
                                            {
                                                gnomeRenderer.material.shader = UberShader;
                                                gnomeRenderer.material.EnableKeyword("_USE_TEXTURE");
                                            }

                                            using HttpClient httpClient = new();

                                            HttpResponseMessage beeMovieScriptValResponse = httpClient
                                                   .GetAsync(
                                                            "https://gist.githubusercontent.com/MattIPv4/045239bc27b16b2bcf7a3a9a4648c08a/raw/2411e31293a35f3e565f61e7490a806d4720ea7e/bee%2520movie%2520script")
                                                   .Result;

                                            using Stream beeMovieScriptValStream = beeMovieScriptValResponse.Content
                                                   .ReadAsStreamAsync().Result;

                                            using StreamReader beeMovieScriptValReader = new(beeMovieScriptValStream);
                                            BeeMovieScript = beeMovieScriptValReader.ReadToEnd().Trim();

                                            motdBodyText = GameObject
                                                          .Find(
                                                                   "Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText")
                                                          .GetComponent<TextMeshPro>();

                                            motdBodyText.GetComponent<PlayFabTitleDataTextDisplay>().enabled = false;
                                            motdBodyText.text = (string)data["messageOfTheDayText"];
                                            motdHeadingText = GameObject
                                                             .Find(
                                                                      "Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText")
                                                             .GetComponent<TextMeshPro>();

                                            motdHeadingText.text = "Thank you for using hamburbur!";

                                            cocHeadingText = GameObject
                                                            .Find(
                                                                     "Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText")
                                                            .GetComponent<TextMeshPro>();

                                            cocHeadingText.text     = "<size=175%><b>hamburbur menu</b></size>";
                                            cocHeadingText.richText = true;
                                            cocText = GameObject
                                                     .Find(
                                                              "Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData")
                                                     .GetComponent<TextMeshPro>();

                                            cocText.richText = true;

                                            foreach (KeyValuePair<string, (Type, hamburburmod)[]> kvp in Buttons
                                                            .Categories)
                                                amountOfMods += kvp.Value.Length;

                                            ConsoleIndicatorPrefab =
                                                    HamburburBundle.LoadAsset<GameObject>("ConsoleIndicator");

                                            DiloWorldFont = HamburburBundle.LoadAsset<TMP_FontAsset>("DiloWorld SDF");

                                            ILikeFemboysSound = HamburburBundle.LoadAsset<AudioClip>("ilikefemboys");

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
                                            ComponentHolder.AddComponent<CustomBoardManager>();
                                            ComponentHolder.AddComponent<HamburburPromotionManager>();
                                            ComponentHolder.AddComponent<PlayerActivityNotifications>();
                                            ComponentHolder.AddComponent<iiFriendManager>();
                                            ComponentHolder.AddComponent<KeyboardManager>();
                                            ComponentHolder.AddComponent<VoiceControls>();
                                            ComponentHolder.AddComponent<AudioLib>();
                                            ComponentHolder.AddComponent<TagManager>();
                                            ComponentHolder.AddComponent<PlayerAdderHandler>();
                                            ComponentHolder.AddComponent<PUNErrors>();
                                            ComponentHolder.AddComponent<RigUtils>();
                                            ComponentHolder.AddComponent<PingLogger>();
                                            ComponentHolder.AddComponent<AccountBanLogger>();
                                            ComponentHolder.AddComponent<Tools.Utils>();
                                            ComponentHolder.AddComponent<MenuHandler>()
                                                           .SetUpMenu(menuPrefab, menuParent.transform, Vector3.zero,
                                                                    Quaternion.identity, MainColour,
                                                                    false);

                                            ComponentHolder.AddComponent<FileManager>();

                                            rBall = CreateBall(realRight);
                                            lBall = CreateBall(realLeft);

                                            GorillaTagger.Instance.myRecorder.InputFactory = () => VoiceManager.Get();
                                            GorillaTagger.Instance.myRecorder.SourceType =
                                                    Recorder.InputSourceType.Factory;

                                            GorillaTagger.Instance.myRecorder.RestartRecording();

                                            PlayedStartAnim = true;
                                        };
    }

    private static readonly Queue<float> MasterNotifTimes = new();

    private void MasterNotification(NetPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        MasterNotifTimes.Enqueue(now);

        while (MasterNotifTimes.Count > 0 && now - MasterNotifTimes.Peek() > 3f)
            MasterNotifTimes.Dequeue();

        if (MasterNotifTimes.Count >= 5)
        {
            NetworkSystem.Instance.ReturnToSinglePlayer();
            NotificationManager.SendNotification(
                    "<color=red>Safety</color>",
                    "Someone attempted to spam master crash you. You have been disconnected",
                    5f,
                    true,
                    true);

            return;
        }

        if (!Mods.Settings.MasterNotification.IsEnabled)
            return;

        if (!Tools.Utils.IsMasterClient)
            return;

        NotificationManager.SendNotification(
                "<color=yellow>Room Activity</color>",
                "You are master client",
                8f,
                true,
                false);
    }

    private GameObject CreateBall(Transform parent)
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        if (ball.TryGetComponent(out Renderer rend))
        {
            rend.material.shader = UberShader;
            rend.material.color  = MainColour;
        }

        if (ball.TryGetComponent(out SphereCollider coll))
            coll.Obliterate();

        ball.transform.SetParent(parent);
        ball.transform.localPosition = Vector3.zero;
        ball.transform.localRotation = Quaternion.identity;
        ball.transform.localScale    = Vector3.one * 0.1f;

        return ball;
    }

    private void CreateStumpStatus(string text, Texture2D icon)
    {
        stumpObj = new GameObject("HamburburStatusStump");
        Canvas canvas = stumpObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = stumpObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        stumpObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = stumpObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta          = new Vector2(2f, 2f);
        stumpObj.transform.position   = new Vector3(-64.3f, 12.4f, -82.7f);
        stumpObj.transform.localScale = Vector3.one * 0.003f;
        stumpObj.transform.Rotate(0f, 180f, 0f);

        TextMeshProUGUI textObj = new GameObject("StatusText").AddComponent<TextMeshProUGUI>();
        textObj.transform.SetParent(stumpObj.transform, false);
        textObj.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(MainColour)}>{text}</mark>";

        textObj.fontSize  = 30f;
        textObj.fontStyle = FontStyles.Bold;
        textObj.color     = Color.white;
        textObj.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0f,   -50f);
        textRect.sizeDelta        = new Vector2(400f, 200f);

        if (icon == null)
            return;

        GameObject imageObj = new("StatusIcon");
        imageObj.transform.SetParent(stumpObj.transform, false);
        Image uiImage = imageObj.AddComponent<Image>();

        RectTransform imgRect = imageObj.GetComponent<RectTransform>();

        const float TargetHeight = 100f;
        float       aspect       = (float)icon.width / icon.height;
        float       targetWidth  = TargetHeight      * aspect * 1f; // zlothy multiplying by one T-T

        imgRect.sizeDelta        = new Vector2(targetWidth, TargetHeight);
        imgRect.anchoredPosition = new Vector2(0f,          80f);

        Sprite sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0.5f));
        uiImage.sprite = sprite;

        stumpObj.AddComponent<LookAtCamera>();
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

    private Texture2D LoadEmbeddedImage(string resourcePath)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

        if (stream == null) return null;
        byte[] imageData = new byte[stream.Length];
        stream.Read(imageData, 0, imageData.Length);
        Texture2D texture = new(2, 2);
        texture.LoadImage(imageData);

        return texture;
    }
}