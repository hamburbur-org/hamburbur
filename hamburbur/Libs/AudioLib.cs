using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mods.Settings;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Libs;

public class AudioLib : MonoBehaviour
{
    private static AudioLib    instance;
    private        AudioSource source;

    public static AudioLib Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new("Hamburbur AudioLib");
                instance = obj.AddComponent<AudioLib>();
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (source == null)
        {
            source             = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }
    }

    public void SpeakText(string text, float volume = 1f) => StartCoroutine(SpeakRoutine(text, volume));

    public IEnumerator SpeakRoutine(string text, float volume)
    {
        string fileName = $"{GetSHA256(text)}.mp3";
        string directoryPath = Path.Combine(Application.persistentDataPath,
                $"Hamburbur_TextToSpeech_Voice-{JarvisVoice.Voices[JarvisVoice.Instance.IncrementalValue]}");

        string filePath = Path.Combine(directoryPath, fileName);

        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        if (!File.Exists(filePath))
        {
            string narratorName = JarvisVoice.Voices[JarvisVoice.Instance.IncrementalValue];

            if (text.Length > 550)
                text = text[..550];

            using UnityWebRequest request = new(
                    "https://lazypy.ro/tts/request_tts.php?service=Streamlabs&voice="
                  + narratorName + "&text=" + UnityWebRequest.EscapeURL(text),
                    "POST");

            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[AudioLib] Error getting TTS: " + request.error);

                yield break;
            }

            string jsonResponse = request.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                Debug.LogError("[AudioLib] Empty TTS JSON response.");

                yield break;
            }

            Dictionary<string, object> responseData =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            if (responseData == null || !responseData.ContainsKey("audio_url"))
            {
                Debug.LogError("[AudioLib] Invalid TTS JSON, missing audio_url: " + jsonResponse);

                yield break;
            }

            if (responseData == null || !responseData.ContainsKey("audio_url") || responseData["audio_url"] == null)
            {
                Debug.LogError("[AudioLib] Streamlabs JSON missing or null audio_url: " + jsonResponse);

                yield break;
            }

            string audioUrl = responseData["audio_url"].ToString().Replace("\\", "");

            using UnityWebRequest dataRequest = UnityWebRequest.Get(audioUrl);

            yield return dataRequest.SendWebRequest();

            if (dataRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[AudioLib] Error downloading TTS: " + audioUrl);

                yield break;
            }

            File.WriteAllBytes(filePath, dataRequest.downloadHandler.data);
        }

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[AudioLib] Error loading TTS clip: {www.error}");

            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip == null)
        {
            Debug.LogError("[AudioLib] Clip is null after download.");

            yield break;
        }

        while (clip.loadState != AudioDataLoadState.Loaded)
            yield return null;

        if (JarvisNotifications.IsEnabled)
            NotificationManager.SendNotification(
                    $"<color=#{ColorUtility.ToHtmlStringRGB(Plugin.Instance.MainColour)}>{char.ToUpper(VoiceControls.Instance.LastUsedWakeWord[0]) + VoiceControls.Instance.LastUsedWakeWord[1..]}</color>",
                    text, clip.length, false, false);

        VoiceManager.Get().AudioClip(clip);
        Debug.Log($"[AudioLib] Playing TTS clip '{text}' with length {clip.length} sec.");

        yield return new WaitForSeconds(clip.length);
    }

    public static string GetSHA256(string text)
    {
        using SHA256  sha   = SHA256.Create();
        byte[]        bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        StringBuilder sb    = new();
        foreach (byte b in bytes) sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}