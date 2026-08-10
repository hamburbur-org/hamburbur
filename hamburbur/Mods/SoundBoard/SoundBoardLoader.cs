using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hamburbur.GUI;
using hamburbur.Managers;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace hamburbur.Mods.SoundBoard;

public static class SoundBoardLoader
{
    private const string SoundBoardCategory = "SoundBoard";

    private sealed class PendingSoundLoad
    {
        public readonly List<Action<AudioClip>> Callbacks = [];
        public int Generation;
    }

    private static readonly Dictionary<string, AudioClip>       AudioFilePool = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, PendingSoundLoad> PendingLoads = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Queue<(string path, string name, int generation)> LoadQueue = new();

    private static bool hasLoadedSoundButtons;
    private static bool isLoading;
    private static int  loadGeneration;

    public static bool HasLoadedAllSounds { get; private set; }
    public static bool IsLoadingAllSounds { get; private set; }

    public static void LoadSoundButtons()
    {
        if (hasLoadedSoundButtons || FileManager.Instance == null)
            return;

        hasLoadedSoundButtons = true;

        foreach (string path in GetCurrentSoundFiles())
        {
            if (ButtonHandler.AddButton(SoundBoardCategory, typeof(Sound)) is not Sound mod)
                continue;

            mod.SoundName = Path.GetFileName(path);
            mod.SoundPath = path;
        }

        ButtonHandler.Instance?.UpdateButtons();
    }

    public static int ReloadSoundButtons()
    {
        CancelPendingLoads();

        if (Buttons.Categories.TryGetValue(SoundBoardCategory, out (Type, Mod_Backend.hamburburmod)[] buttons))
            foreach (Mod_Backend.hamburburmod soundButton in buttons
                         .Where(button => button.Item1 == typeof(Sound))
                         .Select(button => button.Item2)
                         .Where(button => button != null)
                         .ToArray())
                ButtonHandler.RemoveButton(soundButton);

        ClearAudioCache();
        hasLoadedSoundButtons = false;
        LoadSoundButtons();

        if (MenuHandler.Instance != null)
            MenuHandler.Instance.PageIndex = 0;

        ButtonHandler.Instance?.UpdateButtons();

        return GetCurrentSoundFiles().Count;
    }

    public static void LoadSound(string filePath, string fileName, Action<AudioClip> callback)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            callback?.Invoke(null);
            return;
        }

        if (AudioFilePool.TryGetValue(filePath, out AudioClip clip) && clip != null)
        {
            callback?.Invoke(clip);
            return;
        }

        if (PendingLoads.TryGetValue(filePath, out PendingSoundLoad pendingLoad) &&
            pendingLoad.Generation == loadGeneration)
        {
            if (callback != null)
                pendingLoad.Callbacks.Add(callback);

            return;
        }

        PendingSoundLoad newPendingLoad = new() { Generation = loadGeneration, };
        if (callback != null)
            newPendingLoad.Callbacks.Add(callback);

        PendingLoads[filePath] = newPendingLoad;
        LoadQueue.Enqueue((filePath, fileName, loadGeneration));

        if (!isLoading)
            CoroutineManager.Instance.StartCoroutine(ProcessQueue());
    }

    public static void LoadAllSounds(Action<int, int> onComplete = null)
    {
        if (IsLoadingAllSounds)
            return;

        List<string> soundFiles = GetCurrentSoundFiles();
        int          generation = loadGeneration;
        int          total      = soundFiles.Count;
        int          completed  = 0;
        int          loaded     = 0;

        IsLoadingAllSounds = true;

        if (total == 0)
        {
            HasLoadedAllSounds = true;
            IsLoadingAllSounds = false;
            onComplete?.Invoke(0, 0);
            return;
        }

        foreach (string filePath in soundFiles)
            LoadSound(filePath, Path.GetFileName(filePath), clip =>
                                                               {
                                                                   if (generation != loadGeneration)
                                                                       return;

                                                                   completed++;
                                                                   if (clip != null)
                                                                       loaded++;

                                                                   if (completed != total)
                                                                       return;

                                                                   HasLoadedAllSounds = loaded == total;
                                                                   IsLoadingAllSounds = false;
                                                                   onComplete?.Invoke(loaded, total);
                                                               });
    }

    private static IEnumerator ProcessQueue()
    {
        isLoading = true;

        while (LoadQueue.Count > 0)
        {
            (string path, string name, int generation) = LoadQueue.Dequeue();
            yield return LoadSoundRoutine(path, name, generation);
        }

        isLoading = false;
    }

    private static IEnumerator LoadSoundRoutine(string filePath, string fileName, int generation)
    {
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(
                $"file://{filePath}",
                GetAudioType(Path.GetExtension(filePath))
        );

        yield return request.SendWebRequest();

        if (generation != loadGeneration)
            yield break;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load {filePath}: {request.error}");
            CompletePendingLoad(filePath, generation, null);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        if (clip != null)
            clip.name = fileName;

        AudioFilePool[filePath] = clip;
        CompletePendingLoad(filePath, generation, clip);
    }

    private static void CompletePendingLoad(string filePath, int generation, AudioClip clip)
    {
        if (!PendingLoads.TryGetValue(filePath, out PendingSoundLoad pendingLoad) ||
            pendingLoad.Generation != generation)
            return;

        PendingLoads.Remove(filePath);

        foreach (Action<AudioClip> callback in pendingLoad.Callbacks)
            callback?.Invoke(clip);
    }

    private static void CancelPendingLoads()
    {
        loadGeneration++;
        LoadQueue.Clear();
        PendingLoads.Clear();
        HasLoadedAllSounds = false;
        IsLoadingAllSounds = false;
    }

    private static void ClearAudioCache()
    {
        foreach (AudioClip clip in AudioFilePool.Values)
            if (clip != null)
                Object.Destroy(clip);

        AudioFilePool.Clear();
    }

    private static List<string> GetCurrentSoundFiles() =>
            FileManager.Instance?.GetSoundFiles()
                       .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                       .ToList() ?? [];

    private static AudioType GetAudioType(string extension) => extension.ToLowerInvariant() switch
                                                               {
                                                                       ".wav" => AudioType.WAV,
                                                                       ".ogg" => AudioType.OGGVORBIS,
                                                                       ".mp3" => AudioType.MPEG,
                                                                       _      => AudioType.UNKNOWN,
                                                               };
}
