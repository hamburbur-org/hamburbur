using hamburbur.Components;
using hamburbur.Tools;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace hamburbur.Misc;

public class HamburburPromotionManager : Singleton<HamburburPromotionManager>
{
    private bool        hasSetupFeaturedMapVideo;
    private VideoPlayer videoPlayer;
    public  GameObject  Fin;
    private GameObject  stumpObj;


    private void Start()
    {
        GameObject.Find(
                           "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/SatelliteWardrobe/LCKWallCameraSpawner")
                  .Obliterate();

        Fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Fin.transform.localScale = new Vector3(0.8f,    0.9f, 0.0001f);
        Fin.transform.position   = new Vector3(-64.72f, 12f,  -84.72f);
        Fin.transform.rotation   = Quaternion.Euler(0f, 271.63f, 0f);
        
        if (Fin.TryGetComponent(out Collider collider)) collider.Obliterate();

        if (!Fin.TryGetComponent(out Renderer renderer))
            return;

        renderer.sharedMaterial.shader      = Shaders.UberShader;
        renderer.sharedMaterial.mainTexture = Tools.Utils.LoadEmbeddedImage("fin.png");
        renderer.sharedMaterial.EnableKeyword("_USE_TEXTURE");
        renderer.sharedMaterial.color = Color.white;
    }

    private void Update()
    {
        if (hasSetupFeaturedMapVideo && !videoPlayer.isPlaying && videoPlayer.gameObject.activeInHierarchy &&
            videoPlayer.enabled)
            videoPlayer.Play();

        if (hasSetupFeaturedMapVideo)
            return;

        GameObject loadingText = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/LoadingText");

        GameObject mapInfoText =
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/MapInfo_TMP");

        GameObject featuredMaps =
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/ModIOFeaturedMapsDisplay/");

        GameObject displayTextObj =
                GameObject.Find(
                        "Environment Objects/LocalObjects_Prefab/TreeRoom/ModIOFeaturedMapsDisplay/DisplayText");

        if (displayTextObj != null)
            foreach (Transform child in displayTextObj.transform)
                if (child.name.ToLower().EndsWith("tmp"))
                        // Safely gets destroyed by new maps display and for some reason lets this work, idk why but yeah
                    child.gameObject.SetActive(!child.gameObject.activeSelf);

        if (mapInfoText == null || featuredMaps == null)
            return;

        try
        {
            TextMeshPro featuredMapText = mapInfoText.GetComponent<TextMeshPro>();
            if (featuredMapText != null)
                featuredMapText.text = "<color=black>HAMBURBUR ON TOP!</color>";

            //Lazy fix
            if (loadingText != null)
                loadingText.Obliterate();

            GameObject featuredMapImage = featuredMaps.transform.Find("FeaturedMapImage")?.gameObject;

            if (featuredMapImage == null)
                return;

            if (featuredMapImage.TryGetComponent(out SpriteRenderer spriteRenderer))
                spriteRenderer.Obliterate();

            MeshFilter mf = featuredMapImage.GetOrAddComponent<MeshFilter>();
            mf.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            MeshRenderer mr = featuredMapImage.GetOrAddComponent<MeshRenderer>();

            Material videoMat = new(Shader.Find("Unlit/Texture"));
            mr.material = videoMat;

            videoPlayer                 = featuredMapImage.AddComponent<VideoPlayer>();
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.url             = "https://files.hamburbur.org/hamburger.mp4";

            RenderTexture rt = new(512, 512, 0);
            videoPlayer.targetTexture = rt;
            mr.material.mainTexture   = rt;

            featuredMapImage.transform.localScale = new Vector3(0.845f, 0.445f, 1f);

            videoPlayer.isLooping = true;
            videoPlayer.Play();

            featuredMapImage.SetActive(true);

            hasSetupFeaturedMapVideo = true;
        }
        catch
        {
            //fine it threw ONE null reference exception without the try block
        }
    }
    
    public void CreateStumpStatus(string text, Texture2D icon)
    {
        if (stumpObj != null)
            return;

        stumpObj = new GameObject("HamburburStatusStump");
        Canvas canvas = stumpObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = stumpObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        stumpObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = stumpObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta          = new Vector2(2f, 2f);
        stumpObj.transform.position   = new Vector3(-64.3f, 12.4f, -82.7f);
        stumpObj.transform.localScale = Vector3.one * 0.003f;
        stumpObj.transform.Rotate(0f, 180f, 0f);

        TextMeshProUGUI textObj = new GameObject("StatusText").AddComponent<TextMeshProUGUI>();
        textObj.transform.SetParent(stumpObj.transform, false);
        textObj.text = $"<mark=#{ColorUtility.ToHtmlStringRGB(Plugin.Instance.MainColour)}>{text}</mark>";

        textObj.fontSize  = 30f;
        textObj.fontStyle = FontStyles.Bold;
        textObj.color     = Color.white;
        textObj.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0f,   -50f);
        textRect.sizeDelta        = new Vector2(400f, 200f);

        if (icon == null)
            return;

        GameObject imageObj = new("StatusIcon");
        imageObj.transform.SetParent(stumpObj.transform, false);
        Image uiImage = imageObj.AddComponent<Image>();

        RectTransform imgRect = imageObj.GetComponent<RectTransform>();

        const float TargetHeight = 100f;
        float       aspect       = (float)icon.width / icon.height;
        float       targetWidth  = TargetHeight      * aspect * 1f; // zlothy multiplying by one T-T

        imgRect.sizeDelta        = new Vector2(targetWidth, TargetHeight);
        imgRect.anchoredPosition = new Vector2(0f,          80f);

        Sprite sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0.5f));
        uiImage.sprite = sprite;

        stumpObj.AddComponent<LookAtCamera>();
    }
}

[HarmonyPatch(typeof(NewMapsDisplay), nameof(NewMapsDisplay.UpdateSlideshow))]
public static class NewMapsDisplay_UpdateSlideshow_Patch
{
    private static bool Prefix(NewMapsDisplay __instance)
    {
        if (__instance == null)
            return true;

        return __instance.mapImage != null && __instance.mapImage.gameObject != null;
    }
}