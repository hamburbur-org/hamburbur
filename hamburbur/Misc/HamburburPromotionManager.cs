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
    public  GameObject  Fin;
    private GameObject  stumpObj;

    private void Start()
    {
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