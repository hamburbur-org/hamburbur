using System;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod(                      nameof(Sound), "Plays a sound through your mic in-game", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.AlwaysDisabled, 0)]
public class Sound : hamburburmod
{
    private bool isLoading;
    private Guid playingSound;
    private int  playRequestVersion;

    public string SoundName = "";
    public string SoundPath = "";

    public override string ModName => SoundName;

    protected override void OnEnable()
    {
        if (playingSound != Guid.Empty)
            VoiceManager.Instance?.StopAudioClip(playingSound);

        isLoading = true;
        int requestVersion = ++playRequestVersion;

        SoundBoardLoader.LoadSound(SoundPath, SoundName, audioClip =>
                                                         {
                                                             if (!Enabled || requestVersion != playRequestVersion)
                                                                 return;

                                                             isLoading = false;

                                                             if (audioClip == null)
                                                                 return;

                                                             if (audioClip.loadState == AudioDataLoadState.Unloaded)
                                                                 audioClip.LoadAudioData();

                                                             playingSound = VoiceManager.Get().AudioClip(audioClip);
                                                         });
    }

    protected override void Update()
    {
        if (isLoading)
            return;

        if (playingSound          == Guid.Empty ||
            VoiceManager.Instance == null       ||
            VoiceManager.Instance.AudioClips.All(c => c.Id != playingSound))
            Toggle(ButtonState.Normal, false, false);
    }

    protected override void OnDisable()
    {
        playRequestVersion++;
        isLoading = false;

        if (playingSound != Guid.Empty)
            VoiceManager.Instance?.StopAudioClip(playingSound);

        playingSound = Guid.Empty;
    }
}