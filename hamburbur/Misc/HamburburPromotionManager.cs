using hamburbur.Components;
using hamburbur.Tools;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace hamburbur.Misc;

public class HamburburPromotionManager : Singleton<HamburburPromotionManager>
{
    private bool        hasSetupFeaturedMapVideo;
    private VideoPlayer videoPlayer;

    private void Start()
    {
        GameObject.Find(
                           "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/SatelliteWardrobe/LCKWallCameraSpawner")
                  .Obliterate();

        GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fin.transform.localScale = new Vector3(0.8f,    0.9f, 0.0001f);
        fin.transform.position   = new Vector3(-64.72f, 12f,  -84.72f);
        fin.transform.rotation   = Quaternion.Euler(0f, 271.63f, 0f);
        
        if (fin.TryGetComponent(out Collider collider)) collider.Obliterate();

        if (!fin.TryGetComponent(out Renderer renderer))
            return;

        renderer.sharedMaterial.shader      = Plugin.Instance.UberShader;
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