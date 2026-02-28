using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using GorillaNetworking;
using hamburbur.Components;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Movement;
using hamburbur.Mods.Settings;
using hamburbur.Mods.SoundBoard;
using hamburbur.Tools;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace hamburbur.GUI;

public class GUIHandler : Singleton<GUIHandler>
{
    public TextMeshProUGUI NotificationText;

    public GameObject Canvas;

    public GameObject CategoryButtonPrefab;
    public GameObject ModButtonPrefab;

    public GameObject Menu;

    public bool Open;

    private TextMeshProUGUI fpsText;

    private float lastFpsUpdate;
    private float lastTimeDataStreamed;

    private void Start()
    {
        GameObject canvasPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("HamburburGUICanvas");
        Canvas = Instantiate(canvasPrefab);
        canvasPrefab.Obliterate();
        Canvas.name = "HamburburGUICanvas";

        Canvas.transform.TakeChild(2, 1).GetComponent<TextMeshProUGUI>().text =
                $"Build {Constants.PluginVersion}";

        CategoryButtonPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("CategoryButton");
        ModButtonPrefab      = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("ModButton");

        NotificationText      = Canvas.transform.Find("NotificationText").GetComponent<TextMeshProUGUI>();
        NotificationText.text = "";

        Menu = Canvas.transform.TakeChild(1).gameObject;
        Menu.AddComponent<DragObject>();
        fpsText = Menu.transform.TakeChild(1, 1).GetComponent<TextMeshProUGUI>();
        Menu.transform.TakeChild(4).GetComponent<Button>().onClick
            .AddListener(() => ButtonHandler.Instance.SetCategory("Main"));

        Menu.transform.TakeChild(3, 0, 0, 0).AddComponent<GreetingHandler>();

        //Text input field
        Menu.transform.TakeChild(3, 0, 0, 1, 0).GetComponent<TMP_InputField>().onSelect
            .AddListener(_ => WASDFly.DisableMovement = true);

        Menu.transform.TakeChild(3, 0, 0, 1, 0).GetComponent<TMP_InputField>().onDeselect
            .AddListener(_ => WASDFly.DisableMovement = false);

        //Jarvis Speak Button
        Menu.transform.TakeChild(3, 0, 0, 1, 1).GetComponent<Button>().onClick
            .AddListener(() =>
                         {
                             string text = Menu.transform.TakeChild(3, 0, 0, 1, 0)
                                               .GetComponent<TMP_InputField>().text;

                             if (text.IsNullOrEmpty())
                                 return;

                             IEnumerator SpeakRoutine(string textSpeak)
                             {
                                 string currentMicMode = PlayerPrefs.GetString("pttType", "PUSH TO TALK");

                                 string fileName = $"{AudioLib.GetSHA256(text)}.mp3";
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
                                         Debug.LogError("[AudioLib] Invalid TTS JSON, missing audio_url: " +
                                                        jsonResponse);

                                         yield break;
                                     }

                                     if (!responseData.ContainsKey("audio_url") ||
                                         responseData["audio_url"] == null)
                                     {
                                         Debug.LogError("[AudioLib] Streamlabs JSON missing or null audio_url: " +
                                                        jsonResponse);

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

                                 GorillaComputer.instance.pttType = "OPEN MIC";
                                 PlayerPrefs.SetString("pttType", "OPEN MIC");
                                 PlayerPrefs.Save();

                                 yield return AudioLib.Instance.SpeakRoutine(textSpeak, 1f);

                                 GorillaComputer.instance.pttType = currentMicMode;
                                 PlayerPrefs.SetString("pttType", currentMicMode);
                                 PlayerPrefs.Save();
                             }

                             StartCoroutine(SpeakRoutine(text));
                         });

        //Join Button
        Menu.transform.TakeChild(3, 0, 0, 1, 2).GetComponent<Button>().onClick.AddListener(() =>
            {
                string text = Menu.transform.TakeChild(3, 0, 0, 1, 0)
                                  .GetComponent<TMP_InputField>().text;

                if (text.IsNullOrEmpty())
                    return;

                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(text, JoinType.Solo);
            });

        SetUpButtons();
        Menu.SetActive(false);

        Plugin.Instance.MenuLoaded = true;
        SoundBoardLoader.LoadSoundButtons();
    }

    private void Update()
    {
        if (KeyboardManager.Instance.KeyboardOpen)
            return;

        if (lastFpsUpdate + 0.05f < Time.time)
        {
            lastFpsUpdate = Time.time;
            int    fps    = Mathf.FloorToInt(1f / Time.unscaledDeltaTime);
            string colour = fps > 60 ? fps > 72 ? "green" : "yellow" : "red";
            fpsText.text = $"<color={colour}>{fps}</color> FPS";
        }

        if (UnityInput.Current.GetKeyDown(KeyCode.Tab))
            Menu.SetActive(!Menu.activeSelf);

        if (UnityInput.Current.GetKeyDown(KeyCode.H))
        {
            if (!Open)
            {
                GameObject menu        = MenuHandler.Instance.Menu;
                Camera     cameraToUse = Tools.Utils.GetActiveCamera();

                menu.transform.parent.SetParent(cameraToUse.transform);
                menu.transform.parent.localRotation = Quaternion.Euler(270f, 270f, 0f);
                menu.transform.parent.localPosition = new Vector3(0f, 0f, 0.5f);

                StartCoroutine(MenuHandler.Instance.OpenMenu());
            }
            else
            {
                StartCoroutine(CloseMenu());
            }

            Open = !Open;
        }

        if (!Open)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera cameraToUse = Tools.Utils.GetActiveCamera();

            if (Physics.Raycast(cameraToUse.ScreenPointToRay(Mouse.current.position.ReadValue()),
                        out RaycastHit hit,
                        0.6f, 1 << 2, QueryTriggerInteraction.Collide))
                if (hit.collider.TryGetComponent(out ButtonCollider buttonCollider))
                    buttonCollider.OnPress?.Invoke();
        }

        if (lastTimeDataStreamed + 0.1f > Time.time)
            return;

        lastTimeDataStreamed = Time.time;
    }

    public IEnumerator CloseMenu()
    {
        yield return StartCoroutine(MenuHandler.Instance.CloseMenu());

        GameObject menu = MenuHandler.Instance.Menu;

        menu.transform.parent.SetParent(RightHanded.IsEnabled
                                                ? Tools.Utils.RealRightController
                                                : Tools.Utils.RealLeftController);

        menu.transform.parent.localPosition = RightHanded.IsEnabled
                                                      ? Plugin.Instance.MenuLocalPositionRight
                                                      : Plugin.Instance.MenuLocalPositionLeft;

        menu.transform.parent.localRotation = RightHanded.IsEnabled
                                                      ? Plugin.Instance.MenuLocalRotationRight
                                                      : Plugin.Instance.MenuLocalRotationLeft;
    }

    private void SetUpButtons()
    {
        Transform categoryContent = Menu.transform.TakeChild(1).TakeChild(0).TakeChild(0).TakeChild(0);
        Transform modContent      = Menu.transform.TakeChild(2, 0).TakeChild(0);

        foreach (KeyValuePair<string, ValueTuple<Type, hamburburmod>[]> category in Buttons.Categories)
        {
            foreach (ValueTuple<Type, hamburburmod> mod in category.Value)
                if (category.Key == "Main")
                {
                    GameObject categoryButton = Instantiate(CategoryButtonPrefab, categoryContent);
                    categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = mod.Item2.ModName;
                    categoryButton.GetComponent<Button>().onClick
                                  .AddListener(() => mod.Item2.Toggle(ButtonState.Normal));

                    mod.Item2.AssociatedGUIButton = categoryButton;
                }
                else
                {
                    GameObject modButton = Instantiate(ModButtonPrefab, modContent);
                    modButton.transform.TakeChild(0).GetComponent<TextMeshProUGUI>().text = mod.Item2.ModName;
                    modButton.transform.TakeChild(1).GetComponent<TextMeshProUGUI>().text =
                            mod.Item2.AssociatedAttribute.Description;

                    modButton.transform.TakeChild(2, 0).gameObject
                             .SetActive(mod.Item2.AssociatedAttribute.ButtonType != ButtonType.Incremental);

                    modButton.transform.TakeChild(2, 1).gameObject
                             .SetActive(mod.Item2.AssociatedAttribute.ButtonType == ButtonType.Incremental);

                    modButton.transform.TakeChild(2, 0).GetComponent<Button>().onClick
                             .AddListener(() => mod.Item2.Toggle(ButtonState.Normal));

                    modButton.transform.TakeChild(2, 1).TakeChild(0).GetComponent<Button>().onClick
                             .AddListener(() => mod.Item2.Toggle(ButtonState.Decrement));

                    modButton.transform.TakeChild(2, 1).TakeChild(1).GetComponent<Button>().onClick
                             .AddListener(() => mod.Item2.Toggle(ButtonState.Increment));

                    mod.Item2.AssociatedGUIButton = modButton;
                }
        }

        UpdateButtons();
    }

    public void UpdateButtons()
    {
        foreach ((string category, (Type, hamburburmod)[] buttons) in Buttons.Categories)
        {
            if (category == "Main")
            {
                foreach ((Type _, hamburburmod modComp) in buttons)
                    modComp.AssociatedGUIButton.GetComponentInChildren<TextMeshProUGUI>().text = modComp.ModName;

                continue;
            }

            foreach ((Type _, hamburburmod modComp) in buttons)
                if (MenuHandler.Instance.Category != "Enabled Mods")
                {
                    modComp.AssociatedGUIButton.SetActive(category == MenuHandler.Instance.Category);

                    modComp.AssociatedGUIButton.transform.TakeChild(0).GetComponent<TextMeshProUGUI>().text =
                            modComp.ModName;

                    modComp.AssociatedGUIButton.transform.TakeChild(1).GetComponent<TextMeshProUGUI>().text =
                            modComp.AssociatedAttribute.Description;

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType != ButtonType.Incremental);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 1).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType == ButtonType.Incremental);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0, 0).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType == ButtonType.Fixed);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0, 1).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType != ButtonType.Fixed && !modComp.Enabled);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0, 2).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType != ButtonType.Fixed && modComp.Enabled);
                }
                else
                {
                    Dictionary<Type, hamburburmod> enabledMods =
                            Buttons.GetEnabledMods().ToDictionary(mod => mod.Item1, mod => mod.Item2);

                    modComp.AssociatedGUIButton.SetActive(enabledMods.ContainsValue(modComp));
                    modComp.AssociatedGUIButton.transform.TakeChild(0).GetComponent<TextMeshProUGUI>().text =
                            modComp.ModName;

                    modComp.AssociatedGUIButton.transform.TakeChild(1).GetComponent<TextMeshProUGUI>().text =
                            modComp.AssociatedAttribute.Description;

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType != ButtonType.Incremental);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 1).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType == ButtonType.Incremental);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0, 0).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType == ButtonType.Fixed);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0, 1).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType != ButtonType.Fixed && !modComp.Enabled);

                    modComp.AssociatedGUIButton.transform.TakeChild(2, 0, 2).gameObject
                           .SetActive(modComp.AssociatedAttribute.ButtonType != ButtonType.Fixed && modComp.Enabled);
                }

            Menu.transform.TakeChild(2).gameObject.SetActive(MenuHandler.Instance.Category != "Main");
            Menu.transform.TakeChild(3).gameObject.SetActive(MenuHandler.Instance.Category == "Main");
        }
    }
}