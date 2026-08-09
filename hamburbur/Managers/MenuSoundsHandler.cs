using System;
using System.IO;
using System.Reflection;
using GorillaLocomotion;
using hamburbur.Components;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Managers;

public class MenuSoundsHandler : Singleton<MenuSoundsHandler>
{
    public AudioClip MenuOpenSound            { get; private set; }
    public AudioClip MenuDynamicOpenSound     { get; private set; }
    public AudioClip MenuDynamicCloseSound    { get; private set; }
    public AudioClip NotificationSound        { get; private set; }
    public AudioClip DynamicNotificationSound { get; private set; }
    public AudioClip CancelSound              { get; private set; }
    public AudioClip ThinkingSound            { get; private set; }
    public AudioClip GotResponseSound         { get; private set; }
    public AudioClip CameraShutterSound       { get; private set; }

    //Button Press Sounds
    private AudioClip Default       { get; set; }
    private AudioClip KeyboardClick { get; set; }
    private AudioClip Pop           { get; set; }
    private AudioClip Discord       { get; set; }
    private AudioClip SmoothClick   { get; set; }
    private AudioClip HardClick     { get; set; }
    private AudioClip UiEnter       { get; set; }
    private AudioClip Wii           { get; set; }
    private AudioClip Minecraft     { get; set; }
    private AudioClip Untitled      { get; set; }
    private AudioClip Destiny       { get; set; }
    private AudioClip Watch         { get; set; }
    private AudioClip Creamy        { get; set; }

    private void Start()
    {
        AssetBundle bundle = Plugin.Instance.HamburburBundle;
        
        MenuOpenSound            = bundle.LoadAsset<AudioClip>("openMenu");
        MenuDynamicOpenSound     = bundle.LoadAsset<AudioClip>("DynamicOpen");
        MenuDynamicCloseSound    = bundle.LoadAsset<AudioClip>("DynamicClose");
        NotificationSound        = bundle.LoadAsset<AudioClip>("notification");
        DynamicNotificationSound = bundle.LoadAsset<AudioClip>("DynamicNotification");
        CancelSound              = bundle.LoadAsset<AudioClip>("cancel");
        ThinkingSound            = bundle.LoadAsset<AudioClip>("thinking");
        GotResponseSound         = bundle.LoadAsset<AudioClip>("gotresponse");
        CameraShutterSound       = bundle.LoadAsset<AudioClip>("cameraShutter");

        //Button Press Sounds
        Default       = bundle.LoadAsset<AudioClip>("Default");
        KeyboardClick = bundle.LoadAsset<AudioClip>("Keyboard");
        Pop           = bundle.LoadAsset<AudioClip>("Pop");
        Discord       = bundle.LoadAsset<AudioClip>("Discord");
        SmoothClick   = bundle.LoadAsset<AudioClip>("SmoothClick");
        HardClick     = bundle.LoadAsset<AudioClip>("HardClick");
        UiEnter       = bundle.LoadAsset<AudioClip>("UiEnter");
        Wii           = bundle.LoadAsset<AudioClip>("Wii");
        Minecraft     = bundle.LoadAsset<AudioClip>("Minecraft");
        Untitled      = bundle.LoadAsset<AudioClip>("untitled");
        Destiny       = bundle.LoadAsset<AudioClip>("destiny");
        Watch         = bundle.LoadAsset<AudioClip>("watch");
        Creamy        = bundle.LoadAsset<AudioClip>("creamy");
    }

    public void PlayButtonPressSound(bool leftHand)
    {
        try
        {
            if (leftHand)
                VRRig.LocalRig.leftHandPlayer.GTPlayOneShot(GetCurrentButtonPressedSound());
            else
                VRRig.LocalRig.rightHandPlayer.GTPlayOneShot(GetCurrentButtonPressedSound());
        }
        catch
        {
            VRRig.LocalRig.PlayHandTapLocal(GetSoundIndex(), leftHand, 1f);
        }
    }

    public void PlayButtonPressSound()
    {
        try
        {
            Plugin.Instance.PlaySound(GetCurrentButtonPressedSound());
        }
        catch
        {
            PlayHandTapMenu(GetSoundIndex());
        }
    }

    private int GetSoundIndex() => ButtonPressSound.Sounds[ButtonPressSound.Instance.IncrementalValue] switch
                                   {
                                           "Og Sound"       => 67,
                                           "Keyboard Click" => 66,
                                           "Glass"          => 106,
                                           "Krisp Wood"     => 114,
                                           "Rustic Click"   => 271,
                                           "Drip"           => 311,
                                           "Jman Okay"      => 336,
                                           "Jman Ahhhh"     => 337,
                                           var _            => 67,
                                   };

    private AudioClip GetCurrentButtonPressedSound() =>
            ButtonPressSound.Sounds[ButtonPressSound.Instance.IncrementalValue] switch
            {
                    nameof(Default)     => Default,
                    nameof(Pop)         => Pop,
                    nameof(Discord)     => Discord,
                    nameof(SmoothClick) => SmoothClick,
                    nameof(HardClick)   => HardClick,
                    nameof(UiEnter)     => UiEnter,
                    nameof(Wii)         => Wii,
                    nameof(Minecraft)   => Minecraft,
                    nameof(Untitled)    => Untitled,
                    nameof(Destiny)     => Destiny,
                    nameof(Watch)       => Watch,
                    nameof(Creamy)      => Creamy,
                    var _               => throw new ArgumentOutOfRangeException(),
            };

    public void PlayHandTapMenu(int audioClipIndex)
    {
        if (audioClipIndex <= -1 || audioClipIndex >= GTPlayer.Instance.materialData.Count)
            return;

        GTPlayer.MaterialData materialData = GTPlayer.Instance.materialData[audioClipIndex];
        Plugin.Instance.PlaySound(materialData.overrideAudio
                                          ? materialData.audio
                                          : GTPlayer.Instance.materialData[0].audio);
    }

    public static AudioClip LoadWavFromResource(string resourcePath)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

        if (stream == null)
            return null;

        byte[] buffer = new byte[stream.Length];
        // ReSharper disable once MustUseReturnValue
        stream.Read(buffer, 0, buffer.Length);

        WAV     wav = new(buffer);
        float[] samples;

        if (wav.ChannelCount == 2)
        {
            samples = new float[wav.SampleCount];
            for (int i = 0; i < wav.SampleCount; i++)
                samples[i] = (wav.LeftChannel[i] + wav.RightChannel[i]) * 0.5f;
        }
        else
        {
            samples = wav.LeftChannel;
        }

        AudioClip audioClip = AudioClip.Create(resourcePath, wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(samples, 0);

        return audioClip;
    }
}