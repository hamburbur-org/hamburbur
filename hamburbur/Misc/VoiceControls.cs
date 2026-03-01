using System;
using System.Diagnostics;
using System.Linq;
using GorillaLocomotion;
using GorillaNetworking;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Rig;
using hamburbur.Mods.Settings;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using IEpoopenator = System.Collections.IEnumerator;

namespace hamburbur.Misc;

public class VoiceControls : Singleton<VoiceControls>
{
    public string LastUsedWakeWord = "jarvis";

    private readonly string[] replyWords =
    [
            "Yes?",
            "How can I help?",
            "What can I do for you?",
            "I'm here.",
            "Go ahead.",
            "Need something?",
            "What's up?",
            "Hey, I'm listening.",
            "Yeah?",
            "You called?",
            "What's on your mind?",
            "At your service.",
            "Tell me.",
            "Ready when you are.",
            "What's the plan?",
            "I'm ready.",
            "You need me?",
            "How can I assist?",
            "Go on.",
            "Talk to me.",
            "I'm all ears.",
            "Listening.",
            "What do you need?",
            "What's going on?",
            "I'm with you.",
            "Right here.",
            "Yep?",
            "What is it?",
            "Standing by.",
            "Present.",
            "What can I help with?",
            "Fire away.",
            "Waiting.",
            "Here for you.",
            "You got me.",
            "I'm on it.",
            "Lay it on me.",
            "What do you got?",
    ];

    private readonly string[] wakeWords =
            ["jarvis", "system", "assistant", "friday", "echo", "cortana", "mainframe", "ultron", "terminal",];

    private DictationRecognizer dictationRecognizer;
    
    private bool              isListening;
    private KeywordRecognizer wakeRecognizer;

    private IEpoopenator Start()
    {
        if (DisableJarvis.IsEnabled)
            yield break;
        
        if (Application.platform != RuntimePlatform.WindowsPlayer || Environment.OSVersion.Version.Major < 10)
        {
            NotificationManager.SendNotification(
                    "<color=red>Error</color>",
                    "Your system cannot use the voice commands feature",
                    5f,
                    false,
                    false);

            yield break;
        }
        
        if (!Plugin.Instance.JarvisDidFirstInitialisation)
        {
            yield return new WaitForSeconds(5f);
            Plugin.Instance.JarvisDidFirstInitialisation = true;
        }

        yield return AudioLib.Instance.SpeakRoutine("Hamburbur Voice Assistant active", 1f);

        string text = wakeWords.Aggregate("", (current, word) => current + $"[{word}]" + " ");

        NotificationManager.SendNotification(
                $"<color=#{ColorUtility.ToHtmlStringRGB(Plugin.Instance.MainColour)}>Jarvis</color>",
                $"Speak a wake word to begin: {text}",
                5f,
                false,
                false);

        wakeRecognizer                    =  new KeywordRecognizer(wakeWords);
        wakeRecognizer.OnPhraseRecognized += OnWakeWordRecognized;
        wakeRecognizer.Start();

        dictationRecognizer                   =  new DictationRecognizer();
        dictationRecognizer.DictationResult   += OnDictationResult;
        dictationRecognizer.DictationComplete += OnDictationComplete;
        dictationRecognizer.DictationError += (error, hresult) =>
                                              {
                                                  if (!error.Contains(
                                                              "Dictation support is not enabled on this device"))
                                                      return;

                                                  NotificationManager.SendNotification("<color=red>Error</color>",
                                                          "Online Speech Recognition is not enabled on this device. Please enable in in privacy settings to use Voice Controls!",
                                                          5f, true, true);

                                                  Process.Start("ms-settings:privacy-speech");
                                              };

        Log("Voice system ready. Speak a wake word!");
    }

    private void OnDestroy()
    {
        if (wakeRecognizer != null)
        {
            wakeRecognizer.OnPhraseRecognized -= OnWakeWordRecognized;
            wakeRecognizer.Dispose();
        }

        if (dictationRecognizer != null)
        {
            dictationRecognizer.DictationResult   -= OnDictationResult;
            dictationRecognizer.DictationComplete -= OnDictationComplete;
            dictationRecognizer.Dispose();
        }
    }

    private void OnWakeWordRecognized(PhraseRecognizedEventArgs args)
    {
        Log($"Wake word detected: {args.text}");
        LastUsedWakeWord = args.text;

        if (!isListening)
            StartCoroutine(StartDictationRoutine());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEpoopenator StartDictationRoutine()
    {
        isListening = true;

        if (wakeRecognizer != null && wakeRecognizer.IsRunning())
        {
            wakeRecognizer.Stop();
            PhraseRecognitionSystem.Shutdown();
        }

        while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
            yield return null;

        yield return AudioLib.Instance.SpeakRoutine(replyWords[new Random().Next(replyWords.Length)], 1f);

        //putting it here bc for some reason it can hear the tts and its fucking it up for me
        dictationRecognizer.Start();
        Log("Dictation started.");

        const float Timeout = 8f;
        float       timer   = 0f;
        while (dictationRecognizer.Status == SpeechSystemStatus.Running && timer < Timeout)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        if (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            dictationRecognizer.Stop();
            PhraseRecognitionSystem.Shutdown();
            Log("Dictation stopped due to timeout.");

            VoiceManager.Get().AudioClip(MenuSoundsHandler.Instance.CancelSound);
        }

        if (wakeRecognizer != null && !wakeRecognizer.IsRunning())
            wakeRecognizer.Start();

        isListening = false;
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence) =>
            StartCoroutine(OnDictationResultRoutine(text, confidence));

    private IEpoopenator OnDictationResultRoutine(string text, ConfidenceLevel confidence)
    {
        dictationRecognizer.Stop();
        bool   hasYield  = false;
        string lowerText = text.ToLower();

        if (confidence == ConfidenceLevel.Low)
        {
            Log("Confidence level is low.");

            yield return TTSSpeak("Pardon?");
            wakeRecognizer.Start();

            yield break;
        }

        Log("You said: " + text);

        if (lowerText.StartsWith("nevermind")  || lowerText.StartsWith("never-mind") ||
            lowerText.StartsWith("never mind") || lowerText.StartsWith("cancel") || lowerText.StartsWith("forget") ||
            lowerText.StartsWith("shut up")    || lowerText.StartsWith("fuck you") || lowerText.StartsWith("fuck off"))
        {
            VoiceManager.Get().AudioClip(MenuSoundsHandler.Instance.CancelSound);
            wakeRecognizer.Start();

            yield break;
        }

        if (lowerText.StartsWith("enable"))
        {
            string modName = lowerText.Replace("enable", "").Replace(" ", "").Trim();

            (Type, hamburburmod) chosenMod = (null, null);

            foreach ((Type, hamburburmod) mod in from tuples in Buttons.Categories
                                                 from mod in tuples.Value
                                                 where mod.Item2.ModName.ToLower().Replace(" ", "") == modName
                                                 where mod.Item2.AssociatedAttribute.ButtonType == ButtonType.Togglable
                                                 where !mod.Item2.Enabled
                                                 select mod)
            {
                chosenMod = mod;
                mod.Item2.Toggle(ButtonState.Normal);
            }

            if (chosenMod.Item2 != null)
                yield return TTSSpeak("Enabled " + chosenMod.Item2.ModName);
            else
                yield return TTSSpeak("I could not find a mod with that name, or it is already enabled.");

            hasYield = true;
        }
        else if (lowerText.StartsWith("disable"))
        {
            string modName = lowerText.Replace("disable", "").Replace(" ", "").Trim();

            (Type, hamburburmod) chosenMod = (null, null);

            foreach ((Type, hamburburmod) mod in from tuples in Buttons.Categories
                                                 from mod in tuples.Value
                                                 where mod.Item2.ModName.ToLower().Replace(" ", "") == modName
                                                 where mod.Item2.AssociatedAttribute.ButtonType == ButtonType.Togglable
                                                 where mod.Item2.Enabled
                                                 select mod)
            {
                chosenMod = mod;
                mod.Item2.Toggle(ButtonState.Normal);
            }

            if (chosenMod.Item2 != null)
                yield return TTSSpeak("Disabled " + chosenMod.Item2.ModName);
            else
                yield return TTSSpeak("I could not find a mod with that name, or it is already disabled.");

            hasYield = true;
        }
        else if (lowerText.StartsWith("join code") || lowerText.StartsWith("join room") ||
                 lowerText.StartsWith("join"))
        {
            string roomCode = lowerText
                             .Replace("join code", "")
                             .Replace("join room", "")
                             .Replace("join",      "")
                             .Replace(" ",         "")
                             .Trim();

            yield return TTSSpeak("Attempting to join code: " + roomCode.ToUpper());
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode.ToUpper(), JoinType.Solo);

            hasYield = true;
        }
        else if (lowerText.StartsWith("disconnect"))
        {
            if (NetworkSystem.Instance.InRoom)
            {
                yield return TTSSpeak("Leaving code: " + NetworkSystem.Instance.RoomName);
                NetworkSystem.Instance.ReturnToSinglePlayer();
            }

            else
            {
                yield return TTSSpeak("You are not in a room.");
            }

            hasYield = true;
        }
        else if (lowerText is "clip that" or "screenshot that" or "take a picture" or "screenshot" or "picture")
        {
            CameraCapture.Capture(Camera.main);

            yield return new WaitForSeconds(MenuSoundsHandler.Instance.CameraShutterSound.length + 0.2f);
            yield return TTSSpeak("Ive taken a screen shot and saved it to your hamburbur Pictures folder for you.");

            hasYield = true;
        }
        else if (lowerText.StartsWith("jerk") || lowerText is "tug it" or "goon me" or "fuck me")
        {
            yield return TTSSpeak("Ok, ill choke the chicken for you.");
            JerkOff.Instance.Toggle(ButtonState.Normal);

            hasYield = true;
        }
        else if (lowerText is "playtime" or "play time" or "how long have i been playing" or "duration")
        {
            yield return TTSSpeak($"You have been playing for {FormatTime(Time.time)}");

            hasYield = true;
        }
        else if (lowerText is "pause" or "play" or "skip" or "back" or "previous" or "previous track" or "next track"
                              or "skip track")
        {
            switch (lowerText)
            {
                case "play" or "pause":
                    WindowsMediaController.PlayPause();

                    break;

                case "skip" or "next track" or "skip track":
                    WindowsMediaController.NextTrack();

                    break;

                case "back" or "previous track" or "previous":
                    WindowsMediaController.PreviousTrack();

                    break;

                default:
                    yield return TTSSpeak("Uhh this shouldn't happen.");

                    break;
            }

            hasYield = true;
        }
        else if (lowerText is "how many people are online" or "users online" or "players online")
        {
            yield return TTSSpeak(
                    $"There are currently {NetworkSystem.Instance.GlobalPlayerCount().ToString()} players online.");

            hasYield = true;
        }
        else if (lowerText is "server data status" or "console status")
        {
            UnityWebRequest req = UnityWebRequest.Get("https://iidk.online/serverdata");

            yield return req.SendWebRequest();

            yield return TTSSpeak(req.result == UnityWebRequest.Result.Success
                                          ? "Console Server Data is online"
                                          : "Console Server Data is offline");

            hasYield = true;
        }
        else if (lowerText is "restart" or "restart game")
        {
            yield return TTSSpeak("Restarting Gorilla Tag now");

            Process.Start("steam://run/1533390");
            Application.Quit();

            hasYield = true;
        }
        else if (GPTJarvis.IsEnabled)
        {
            yield return ProcessVoiceCommand(text);

            hasYield = true;
        }

        if (!hasYield)
            yield return TTSSpeak("Pardon?");

        wakeRecognizer.Start();
    }

#region AI Stuff

    private IEpoopenator ProcessVoiceCommand(string input)
    {
        input = Uri.EscapeDataString(input);
        string prompt = Uri.EscapeDataString(string.Format(Constants.AIprompt));
        string api    = $"https://text.pollinations.ai/{input}?system={prompt}?private=true?model=openai";

        using UnityWebRequest request = UnityWebRequest.Get(api);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            VoiceManager.Get().AudioClip(MenuSoundsHandler.Instance.CancelSound);

            yield return TTSSpeak("Could not fetch a response from the ai");

            yield break;
        }

        string reply = request.downloadHandler.text;

        VoiceManager.Get().AudioClip(MenuSoundsHandler.Instance.GotResponseSound);

        yield return TTSSpeak(reply);
    }

#endregion

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void OnDictationComplete(DictationCompletionCause cause)
    {
        Log($"Dictation completed: {cause}");
        if (cause == DictationCompletionCause.TimeoutExceeded)
            VoiceManager.Get().AudioClip(MenuSoundsHandler.Instance.CancelSound);
    }

    private IEpoopenator TTSSpeak(string text)
    {
        yield return AudioLib.Instance.SpeakRoutine(text, 1f);
    }

    private void Log(string logMessage) => Debug.Log("[Hamburbur Voice Assistant] " + logMessage);

    private string FormatTime(float time)
    {
        TimeSpan t = TimeSpan.FromSeconds(time);

        string hours   = t.Hours > 0 ? $"{NumberToWords(t.Hours)} hour{(t.Hours > 1 ? "s" : "")}, " : "";
        string minutes = $"{NumberToWords(t.Minutes)} minute{(t.Minutes != 1 ? "s" : "")}";
        string seconds = $"{NumberToWords(t.Seconds)} second{(t.Seconds != 1 ? "s" : "")}";

        return $"{hours}{minutes} and {seconds}.";
    }

    //i love chat gpt 
    private string NumberToWords(int number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "minus " + NumberToWords(Math.Abs(number));

        string[] unitsMap =
        [
                "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
                "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen",
                "eighteen", "nineteen",
        ];

        string[] tensMap =
        [
                "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty",
                "ninety",
        ];

        if (number < 20)
            return unitsMap[number];

        if (number < 100)
        {
            int tens  = number / 10;
            int units = number % 10;

            return units == 0 ? tensMap[tens] : $"{tensMap[tens]} {unitsMap[units]}";
        }

        return number.ToString();
    }
}

public static class RecognizerExtensions
{
    public static bool IsRunning(this KeywordRecognizer recognizer) =>
            recognizer != null && PhraseRecognitionSystem.Status == SpeechSystemStatus.Running;
}