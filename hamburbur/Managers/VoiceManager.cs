using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Voice;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hamburbur.Managers;

// https://github.com/Seralyth/Seralyth-Menu/blob/master/Managers/VoiceManager.cs
public class VoiceManager : IAudioReader<float>
{
    private const double NetworkMixFreshnessSeconds = 0.1d;

    private readonly List<Clip> audioClips = [];

    private readonly int loopLength;

    /// <summary>
    ///     A list of post processors that can be used to edit the buffer after all the audio data is compiled.
    /// </summary>
    public readonly Dictionary<string, Action<float[]>> PostProcessors = new();

    private float[] lastMixedBuffer;
    private double  lastMixedBufferDspTime = double.NegativeInfinity;

    private int       lastSamplePosition;
    private float[]   localChannelOutputBuffer;
    private float[]   localOutputBuffer;
    private AudioClip microphoneClip;

    private int           outputRate = 48000;
    private float         pitch      = 1f;
    private float         resample;
    private int           samplingRate = 48000;
    private float         step;
    private NetworkSystem subscribedNetworkSystem;

    private float[] tempBuffer;

    private VoiceManager(int loopLength = 1, string device = null)
    {
        this.loopLength = loopLength;
        StartRecording(device);
        EnsureRoomLeaveHook();
    }

    /// <summary>
    ///     A read-only list of AudioClips currently playing
    /// </summary>
    public IReadOnlyList<Clip> AudioClips => audioClips.AsReadOnly();

    /// <summary>
    ///     Gets or sets the microphone's recording status. This does not stop the pushed AudioClip from playing.
    /// </summary>
    public bool MuteMicrophone { get; set; }

    /// <summary>
    ///     Gets or sets the output rate used for AudioClip samples.
    /// </summary>
    public int OutputRate
    {
        get => outputRate;

        set
        {
            outputRate = value;
            RestartMicrophone();
        }
    }

    /// <summary>
    ///     Gets or sets the microphone gain multiplier.
    /// </summary>
    private float Gain { get; } = 1;

    /// <summary>
    ///     Gets or sets the pitch. Lowest possible value can be 0.1f.
    /// </summary>
    public float Pitch
    {
        get => pitch;
        set => pitch = Mathf.Max(0.1f, value);
    }

    /// <summary>
    ///     Gets or sets the decision on if the post-processing should affect the applied Audio Clip or not.
    /// </summary>
    public bool PostProcessClip { get; set; }

    private string CurrentDevice { get; set; }

    public static VoiceManager Instance { get; private set; }

    /// <summary>
    ///     Gets or sets the microphone sampling rate. Setting a value restarts the microphone.
    /// </summary>
    public int SamplingRate
    {
        get => samplingRate;

        set
        {
            samplingRate = value;
            RestartMicrophone();
        }
    }

    public int    Channels => 1;
    public string Error    { get; private set; }

    /// <summary>
    ///     Used to pull the next chunk of audio samples.
    /// </summary>

    // this is automatically called by photon
    public bool Read(float[] buffer)
    {
        if (microphoneClip == null || string.IsNullOrEmpty(CurrentDevice)) return false;

        int samples = Mathf.CeilToInt(buffer.Length * step);
        int pos     = Microphone.GetPosition(CurrentDevice);
        int available = pos < lastSamplePosition
                                ? microphoneClip.samples - lastSamplePosition + pos
                                : pos                                         - lastSamplePosition;

        if (available < samples) return false;

        if (tempBuffer == null || tempBuffer.Length != samples)
            tempBuffer = new float[samples];

        int remaining = microphoneClip.samples - lastSamplePosition;
        if (remaining >= samples)
        {
            microphoneClip.GetData(tempBuffer, lastSamplePosition);
        }
        else
        {
            microphoneClip.GetData(tempBuffer, lastSamplePosition);
            int     wrap       = samples - remaining;
            float[] wrapBuffer = new float[wrap];
            microphoneClip.GetData(wrapBuffer, 0);
            Array.Copy(wrapBuffer, 0, tempBuffer, remaining, wrap);
        }

        float[] microphoneBuffer = new float[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            float microphoneSample = 0;
            if (!MuteMicrophone && !audioClips.Any(c => c.TransmitOverVoice && c.MuteMicrophone))
            {
                int index     = (int)resample;
                int nextIndex = index + 1;
                if (index >= tempBuffer.Length)
                {
                    resample  = 0f;
                    index     = 0;
                    nextIndex = 1;
                }

                if (nextIndex >= tempBuffer.Length) nextIndex = 0;

                microphoneSample = Mathf.Lerp(tempBuffer[index], tempBuffer[nextIndex], resample - index);

                resample += step * pitch;
                if (resample >= tempBuffer.Length) resample = 0f;
            }

            microphoneBuffer[i] = microphoneSample * Gain;
        }

        if (!PostProcessClip)
            foreach (Action<float[]> postProcess in PostProcessors.Values)
                postProcess?.Invoke(microphoneBuffer);

        for (int i = 0; i < buffer.Length; i++)
        {
            float pushed = NextAudioClipSample();
            buffer[i] = Mathf.Clamp(microphoneBuffer[i] + pushed, -1f, 1f);
        }

        if (PostProcessClip)
            foreach (Action<float[]> postProcess in PostProcessors.Values)
                postProcess?.Invoke(buffer);

        if (lastMixedBuffer == null || lastMixedBuffer.Length != buffer.Length)
            lastMixedBuffer = new float[buffer.Length];

        Array.Copy(buffer, lastMixedBuffer, buffer.Length);
        lastMixedBufferDspTime = AudioSettings.dspTime;

        lastSamplePosition = (lastSamplePosition + samples) % microphoneClip.samples;

        return true;
    }

    public void Dispose()
    {
        if (subscribedNetworkSystem != null)
        {
            subscribedNetworkSystem.OnReturnedToSinglePlayer -= (Action)StopNetworkAudioClips;
            subscribedNetworkSystem                          =  null;
        }

        StopAudioClips();
        StopRecording();
        Instance = null;
    }

    /// <summary>
    ///     Returns a valid VoiceManager instance. If the Instance variable is null, it will create a new VoiceManager.
    /// </summary>
    /// <param name="loopLength">
    ///     Length (in seconds) of the looping mic buffer, handled by Unity when the microphone is
    ///     started, only used if the instance is null.
    /// </param>
    /// <param name="device">The microphone device to be used in recording, if the instance is null.</param>
    /// <returns></returns>
    public static VoiceManager Get(int loopLength = 1, string device = null)
    {
        Instance ??= new VoiceManager(loopLength, device);
        Instance.EnsureRoomLeaveHook();

        return Instance;
    }

    /// <summary>
    ///     Starts the microphone recording.
    /// </summary>
    /// <param name="device"> Microphone device name to be used. If empty, the default microphone is selected.</param>
    public bool StartRecording(string device = null)
    {
        Error = null;

        if (Microphone.devices.Length == 0)
        {
            Error = "No microphone devices found";
            Debug.LogWarning(Error);

            return false;
        }

        CurrentDevice = string.IsNullOrEmpty(device) ? Microphone.devices[0] : device;

        if (Microphone.IsRecording(CurrentDevice))
            Microphone.End(CurrentDevice);

        microphoneClip     = Microphone.Start(CurrentDevice, true, loopLength, samplingRate);
        lastSamplePosition = 0;
        step               = samplingRate / (float)OutputRate;

        return true;
    }

    /// <summary>
    ///     Stops the microphone recording.
    /// </summary>
    public bool StopRecording()
    {
        if (!string.IsNullOrEmpty(CurrentDevice) && Microphone.IsRecording(CurrentDevice))
            Microphone.End(CurrentDevice);

        microphoneClip     = null;
        lastSamplePosition = 0;

        return true;
    }

    /// <summary>
    ///     Switches the microphone device and restarts recording.
    /// </summary>
    /// <param name="device">Microphone device name to be used.</param>
    public bool SwitchMicrophone(string device)
        => StopRecording() && StartRecording(device);

    /// <summary>
    ///     Restarts the microphone using the current device, or the default if none is set.
    /// </summary>
    public bool RestartMicrophone()
        => StopRecording() && StartRecording(CurrentDevice);

    /// <summary>
    ///     Pushes an <see cref="UnityEngine.AudioClip" /> into the output stream.
    /// </summary>
    /// <param name="clip"><see cref="UnityEngine.AudioClip" /> to play.</param>
    /// <param name="disableMicrophone">Whether to mute the microphone while the clip plays.</param>
    /// <returns>
    ///     <see cref="System.Guid" />
    /// </returns>
    public Guid AudioClip(AudioClip clip, bool disableMicrophone = false)
    {
        if (clip == null)
            return Guid.Empty;

        if (clip.frequency != OutputRate)
            clip = Resample(clip, OutputRate);

        int     channels = clip.channels;
        float[] raw      = new float[clip.samples * channels];
        clip.GetData(raw, 0);

        float[] mono = new float[clip.samples];
        if (channels == 1)
            for (int i = 0; i < clip.samples; i++)
                mono[i] = raw[i];
        else
            for (int i = 0; i < clip.samples; i++)
            {
                float sum       = 0f;
                int   baseIndex = i * channels;
                for (int c = 0; c < channels; c++)
                    sum += raw[baseIndex + c];

                mono[i] = sum / channels;
            }

        Guid id = Guid.NewGuid();
        Clip playingClip = new()
        {
                Id                = id,
                Source            = clip,
                Samples           = mono,
                Position          = 0f,
                Step              = clip.frequency / (float)OutputRate,
                MuteMicrophone    = disableMicrophone,
                TransmitOverVoice = NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom,
                LocalAudioSource  = new GameObject(id.ToString()).AddComponent<AudioSource>(),
        };

        audioClips.Add(playingClip);

        playingClip.LocalAudioSource.GTPlayOneShot(clip);

        if (CoroutineManager.Instance != null)
            CoroutineManager.Instance.StartCoroutine(WaitForLocalPlaybackToFinish(id, playingClip.LocalAudioSource));

        return id;
    }

    /// <summary>
    ///     Resamples the given <see cref="UnityEngine.AudioClip" /> to the specified sample rate.
    /// </summary>
    /// <param name="source">The <see cref="UnityEngine.AudioClip" /> to be resampled.</param>
    /// <param name="targetSampleRate">The desired sample rate for the resulting audio clip.</param>
    /// <returns>A new <see cref="UnityEngine.AudioClip" /> containing the resampled audio data.</returns>

    // this is pretty heavy, but the only fix I could think of for making clip length consistent.
    public static AudioClip Resample(AudioClip source, int sampleRate)
    {
        if (source == null || source.frequency == sampleRate)
            return source;

        int channels      = source.channels;
        int sourceSamples = source.samples;

        float[] sourceData = new float[sourceSamples * channels];
        source.GetData(sourceData, 0);

        int     targetSamples = Mathf.CeilToInt(source.length * sampleRate);
        float[] targetData    = new float[targetSamples * channels];

        float ratio = (float)(sourceSamples - 1) / (targetSamples - 1);

        for (int i = 0; i < targetSamples; i++)
        {
            float srcIndex = i * ratio;
            int   index    = Mathf.FloorToInt(srcIndex);
            int   next     = Mathf.Min(index + 1, sourceSamples - 1);
            float t        = srcIndex - index;

            for (int c = 0; c < channels; c++)
                targetData[i * channels + c] =
                        Mathf.Lerp(sourceData[index * channels + c], sourceData[next * channels + c], t);
        }

        AudioClip resampled = UnityEngine.AudioClip.Create(
                source.name,
                targetSamples,
                channels,
                sampleRate,
                false
        );

        resampled.SetData(targetData, 0);

        return resampled;
    }

    /// <summary>
    ///     Fills the provided buffer with the current mixed audio output for waveform analysis.
    ///     A fresh Photon Voice mix is used while in a room. Local-only playback is mixed directly from
    ///     its AudioSources so visualizers continue working in single player and across room transitions.
    /// </summary>
    public void GetMixedOutput(float[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return;

        Array.Clear(buffer, 0, buffer.Length);

        double currentDspTime = AudioSettings.dspTime;
        bool hasFreshNetworkMix = NetworkSystem.Instance != null                  &&
                                  NetworkSystem.Instance.InRoom                   &&
                                  lastMixedBuffer                         != null &&
                                  currentDspTime - lastMixedBufferDspTime <= NetworkMixFreshnessSeconds;

        if (hasFreshNetworkMix)
        {
            int len = Mathf.Min(buffer.Length, lastMixedBuffer.Length);
            Array.Copy(lastMixedBuffer, buffer, len);
        }

        MixLocalPlaybackOutput(buffer, !hasFreshNetworkMix);
    }

    private void MixLocalPlaybackOutput(float[] buffer, bool includeTransmittedClips)
    {
        int requiredBufferSize = Mathf.NextPowerOfTwo(buffer.Length);
        if (localOutputBuffer == null || localOutputBuffer.Length < requiredBufferSize)
            localOutputBuffer = new float[requiredBufferSize];

        if (localChannelOutputBuffer == null || localChannelOutputBuffer.Length < requiredBufferSize)
            localChannelOutputBuffer = new float[requiredBufferSize];

        foreach (Clip clip in audioClips)
        {
            if (clip.LocalAudioSource == null    ||
                !clip.LocalAudioSource.isPlaying ||
                !includeTransmittedClips && clip.TransmitOverVoice)
                continue;

            Array.Clear(localOutputBuffer, 0, localOutputBuffer.Length);
            clip.LocalAudioSource.GetOutputData(localOutputBuffer, 0);

            int   channelCount = Mathf.Max(clip.Source?.channels ?? 1, 1);
            float channelGain  = 1f / Mathf.Sqrt(channelCount);
            for (int i = 0; i < buffer.Length; i++)
                localOutputBuffer[i] *= channelGain;

            for (int channel = 1; channel < channelCount; channel++)
            {
                Array.Clear(localChannelOutputBuffer, 0, localChannelOutputBuffer.Length);
                clip.LocalAudioSource.GetOutputData(localChannelOutputBuffer, channel);

                for (int i = 0; i < buffer.Length; i++)
                    localOutputBuffer[i] += localChannelOutputBuffer[i] * channelGain;
            }

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = Mathf.Clamp(buffer[i] + localOutputBuffer[i], -1f, 1f);
        }
    }

    /// <summary>
    ///     Stops the specified <see cref="UnityEngine.AudioClip" /> from playing.
    /// </summary>
    /// <param name="id">The GUID of the <see cref="UnityEngine.AudioClip" /> to stop.</param>
    public bool StopAudioClip(Guid id)
    {
        int index = audioClips.FindIndex(c => c.Id == id);

        if (index == -1) return false;

        StopLocalPlayback(audioClips[index]);
        audioClips.RemoveAt(index);

        return true;
    }

    /// <summary>
    ///     Stops all the currently playing audio clips.
    /// </summary>
    public void StopAudioClips()
    {
        foreach (Clip clip in audioClips)
            StopLocalPlayback(clip);

        audioClips.Clear();
    }

    /// <summary>
    ///     Returns the next sample from the pushed audio buffer each time the Read() function is called.
    /// </summary>
    private float NextAudioClipSample()
    {
        if (audioClips.Count == 0)
            return 0f;

        float mixed = 0f;

        for (int i = audioClips.Count - 1; i >= 0; i--)
        {
            Clip clip = audioClips[i];

            if (!clip.TransmitOverVoice)
                continue;

            int index = (int)clip.Position;

            if (index >= clip.Samples.Length)
            {
                clip.TransmitOverVoice = false;

                continue;
            }

            int nextIndex = index + 1;
            if (nextIndex >= clip.Samples.Length)
            {
                mixed                  += clip.Samples[index];
                clip.TransmitOverVoice =  false;

                continue;
            }

            float frac = clip.Position - index;
            mixed += Mathf.Lerp(clip.Samples[index], clip.Samples[nextIndex], frac);

            clip.Position += clip.Step;
        }

        return mixed;
    }

    private void EnsureRoomLeaveHook()
    {
        NetworkSystem networkSystem = NetworkSystem.Instance;

        if (networkSystem == null || networkSystem == subscribedNetworkSystem)
            return;

        if (subscribedNetworkSystem != null)
            subscribedNetworkSystem.OnReturnedToSinglePlayer -= (Action)StopNetworkAudioClips;

        subscribedNetworkSystem                          =  networkSystem;
        subscribedNetworkSystem.OnReturnedToSinglePlayer += (Action)StopNetworkAudioClips;
    }

    /// <summary>
    ///     Stops clips from being sent through Photon Voice without interrupting their local playback.
    ///     This is used when returning to single player so soundboard state and Jarvis speech can finish normally.
    /// </summary>
    private void StopNetworkAudioClips()
    {
        foreach (Clip clip in audioClips)
            clip.TransmitOverVoice = false;

        if (lastMixedBuffer != null)
            Array.Clear(lastMixedBuffer, 0, lastMixedBuffer.Length);

        lastMixedBufferDspTime = double.NegativeInfinity;
    }

    private IEnumerator WaitForLocalPlaybackToFinish(Guid id, AudioSource localAudioSource)
    {
        yield return null;

        while (localAudioSource != null && localAudioSource.isPlaying)
            yield return null;

        int index = audioClips.FindIndex(c => c.Id == id && c.LocalAudioSource == localAudioSource);
        if (index != -1)
        {
            audioClips[index].LocalAudioSource = null;
            audioClips.RemoveAt(index);
        }

        if (localAudioSource != null)
            Object.Destroy(localAudioSource.gameObject);
    }

    private static void StopLocalPlayback(Clip clip)
    {
        if (clip?.LocalAudioSource == null)
            return;

        clip.LocalAudioSource.Stop();
        Object.Destroy(clip.LocalAudioSource.gameObject);
        clip.LocalAudioSource = null;
    }

    public sealed class Clip
    {
        public AudioSource LocalAudioSource;
        public bool        MuteMicrophone;
        public float       Position;
        public float[]     Samples;
        public float       Step;
        public bool        TransmitOverVoice;
        public Guid        Id     { get; set; }
        public AudioClip   Source { get; set; }
    }
}