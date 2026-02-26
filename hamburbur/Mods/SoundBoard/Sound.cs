using System;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod("Sound", "Plays a sound through your mic in-game", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.AlwaysDisabled, 0)]
public class Sound : hamburburmod
{
    private Guid   playingSound;
    public  string SoundName = "";
    public  string SoundPath = "";

    public override string ModName => SoundName;

    protected override void OnEnable()
    {
        if (playingSound != Guid.Empty)
            VoiceManager.Get().StopAudioClip(playingSound);

        SoundBoardLoader.LoadSound(SoundPath, SoundName, audioClip =>
                                                         {
                                                             if (audioClip == null)
                                                                 return;

                                                             if (audioClip.loadState == AudioDataLoadState.Unloaded)
                                                                 audioClip.LoadAudioData();

                                                             playingSound = VoiceManager.Get().AudioClip(audioClip);
                                                         });
    }

    protected override void Update()
    {
        if (VoiceManager.Get().AudioClips.Where(c => c.Id == playingSound).ToArray().Length < 1)
            Toggle(ButtonState.Normal, false, false);
    }

    protected override void OnDisable()
    {
        VoiceManager.Get().StopAudioClip(playingSound);
        playingSound = Guid.Empty;
    }
}