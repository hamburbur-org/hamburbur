using System;
using System.IO;
using System.Reflection;
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
        MenuOpenSound            = LoadWavFromResource("hamburbur.Resources.openMenu.wav");
        MenuDynamicOpenSound     = LoadWavFromResource("hamburbur.Resources.DynamicOpen.wav");
        MenuDynamicCloseSound    = LoadWavFromResource("hamburbur.Resources.DynamicClose.wav");
        NotificationSound        = LoadWavFromResource("hamburbur.Resources.notification.wav");
        DynamicNotificationSound = LoadWavFromResource("hamburbur.Resources.DynamicNotification.wav");
        CancelSound              = LoadWavFromResource("hamburbur.Resources.cancel.wav");
        ThinkingSound            = LoadWavFromResource("hamburbur.Resources.thinking.wav");
        GotResponseSound         = LoadWavFromResource("hamburbur.Resources.gotresponse.wav");
        CameraShutterSound       = LoadWavFromResource("hamburbur.Resources.cameraShutter.wav");

        //Button Press Sounds
        Default       = LoadWavFromResource("hamburbur.Resources.Default.wav");
        KeyboardClick = LoadWavFromResource("hamburbur.Resources.Keyboard.wav");
        Pop           = LoadWavFromResource("hamburbur.Resources.Pop.wav");
        Discord       = LoadWavFromResource("hamburbur.Resources.Discord.wav");
        SmoothClick   = LoadWavFromResource("hamburbur.Resources.SmoothClick.wav");
        HardClick     = LoadWavFromResource("hamburbur.Resources.HardClick.wav");
        UiEnter       = LoadWavFromResource("hamburbur.Resources.UiEnter.wav");
        Wii           = LoadWavFromResource("hamburbur.Resources.Wii.wav");
        Minecraft     = LoadWavFromResource("hamburbur.Resources.Minecraft.wav");
        Untitled      = LoadWavFromResource("hamburbur.Resources.untitled.wav");
        Destiny       = LoadWavFromResource("hamburbur.Resources.destiny.wav");
        Watch         = LoadWavFromResource("hamburbur.Resources.watch.wav");
        Creamy        = LoadWavFromResource("hamburbur.Resources.creamy.wav");
    }

    public void PlayButtonPressSound()
    {
        try
        {
            VRRig.LocalRig.rightHandPlayer.GTPlayOneShot(GetCurrentButtonPressedSound());
        }
        catch
        {
            VRRig.LocalRig.PlayHandTapLocal(GetSoundIndex(), false, 1f);
        }
    }

    private int GetSoundIndex() => ButtonPressSound.Sounds[ButtonPressSound.Instance.IncrementalValue] switch
                                   {
                                           "KeyboardClick" => 66,
                                           "Glass"         => 106,
                                           "Krisp Wood"    => 114,
                                           "Rustic Click"  => 271,
                                           "Drip"          => 311,
                                           "Jman Okay"     => 336,
                                           "Jman Ahhhh"    => 337,
                                           var _           => 67,
                                   };

    private AudioClip GetCurrentButtonPressedSound() =>
            ButtonPressSound.Sounds[ButtonPressSound.Instance.IncrementalValue] switch
            {
                    "Default"     => Default,
                    "Pop"         => Pop,
                    "Discord"     => Discord,
                    "SmoothClick" => SmoothClick,
                    "HardClick"   => HardClick,
                    "UiEnter"     => UiEnter,
                    "Wii"         => Wii,
                    "Minecraft"   => Minecraft,
                    "Untitled"    => Untitled,
                    "Destiny"     => Destiny,
                    "Watch"       => Watch,
                    "Creamy"      => Creamy,
                    var _         => throw new ArgumentOutOfRangeException(),
            };

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