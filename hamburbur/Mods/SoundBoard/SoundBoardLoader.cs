using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using hamburbur.GUI;
using hamburbur.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace hamburbur.Mods.SoundBoard;

public static class SoundBoardLoader
{
    private static readonly Dictionary<string, AudioClip> AudioFilePool = [];
    public static           bool                          HasLoadedAllSounds;

    private static readonly Queue<(string path, string name, Action<AudioClip> cb)> loadQueue = new();
    private static          bool                                                    isLoading;

    public static void LoadSoundButtons()
    {
        foreach (string path in FileManager.Instance.GetSoundFiles())
        {
            string fileName = Path.GetFileName(path);

            Sound mod =
                    (Sound)ButtonHandler.AddButton("SoundBoard", typeof(Sound));

            mod.SoundName = fileName;
            mod.SoundPath = path;
        }
    }

    public static void LoadSound(string filePath, string fileName, Action<AudioClip> callback)
    {
        if (AudioFilePool.TryGetValue(fileName, out AudioClip clip))
        {
            callback?.Invoke(clip);

            return;
        }

        loadQueue.Enqueue((filePath, fileName, callback));

        if (!isLoading)
            CoroutineManager.Instance.StartCoroutine(ProcessQueue());
    }

    private static IEnumerator ProcessQueue()
    {
        isLoading = true;

        while (loadQueue.Count > 0)
        {
            (string path, string name, Action<AudioClip> cb) = loadQueue.Dequeue();

            yield return LoadSoundRoutine(path, name, cb);
        }

        isLoading = false;
    }

    private static IEnumerator LoadSoundRoutine(string filePath, string fileName, Action<AudioClip> callback)
    {
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(
                $"file://{filePath}",
                GetAudioType(Path.GetExtension(filePath))
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load {filePath}: {request.error}");
            callback?.Invoke(null);

            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        clip.LoadAudioData();

        AudioFilePool[fileName] = clip;
        callback?.Invoke(clip);
    }

    private static AudioType GetAudioType(string extension) => extension.ToLower() switch
                                                               {
                                                                       ".wav" => AudioType.WAV,
                                                                       ".ogg" => AudioType.OGGVORBIS,
                                                                       ".mp3" => AudioType.MPEG,
                                                                       var _  => AudioType.UNKNOWN,
                                                               };
}