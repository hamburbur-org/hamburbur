using System.Collections;
using GorillaLocomotion;
using hamburbur.Components;
using hamburbur.Tools;
using UnityEngine;
using UnityEngine.Video;

namespace hamburbur.Managers;

public class LoadingScreenManager : Singleton<LoadingScreenManager>
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);

        GameObject canvasPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("LoadingScreenCanvas");
        GameObject worldCanvasPrefab =
                Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("LoadingScreenCanvasWorldSpace");

        AudioClip clip = MenuSoundsHandler.LoadWavFromResource("hamburbur.Resources.StartupSound.wav");

        GameObject canvasInstance = Instantiate(canvasPrefab);

        GameObject worldCanvasInstance = Instantiate(worldCanvasPrefab, GTPlayer.Instance.headCollider.transform);
        worldCanvasInstance.transform.localPosition = new Vector3(0f, 0f, 1f);
        worldCanvasInstance.transform.localScale    = Vector3.one * 0.001f;
        worldCanvasInstance.SetLayer(UnityLayer.FirstPersonOnly);

        CanvasGroup group      = canvasInstance.GetComponentInChildren<CanvasGroup>();
        CanvasGroup worldGroup = worldCanvasInstance.GetComponentInChildren<CanvasGroup>();

        group.alpha      = 0f;
        worldGroup.alpha = 0f;

        Plugin.Instance.PlaySound(clip);

        while (worldGroup.alpha < 1f)
        {
            group.alpha      += Time.deltaTime * 1f;
            worldGroup.alpha += Time.deltaTime * 1f;

            yield return null;
        }

        group.alpha      = 1f;
        worldGroup.alpha = 1f;

        yield return new WaitForSeconds(clip.length - 2f);

        while (worldGroup.alpha > 0f)
        {
            group.alpha      -= Time.deltaTime * 1f;
            worldGroup.alpha -= Time.deltaTime * 1f;

            yield return null;
        }

        canvasInstance.Obliterate();
        worldCanvasInstance.Obliterate();

        Plugin.Instance.DelayedStart();
        gameObject.Obliterate();
    }

    public IEnumerator TutorialScreen()
    {
        Debug.Log("Im tutorial screening it");
        
        GTPlayer.Instance.disableMovement = true;
        
        GameObject tutorialScreen = Instantiate(Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("TutorialScreen"));
        
        //Already contains the video url and play on awake is true in the asset bundle
        VideoPlayer player = tutorialScreen.GetComponent<VideoPlayer>();

        bool shouldClose = false;
        
        //Exit Button
        tutorialScreen.transform.GetChild(0).AddComponent<ButtonCollider>().OnPress += () => shouldClose = true;

        player.loopPointReached += _ => shouldClose = true;

        while (!shouldClose)
        {
            if (tutorialScreen == null)
                yield break;
            
            float distance = Vector3.Distance(GTPlayer.Instance.bodyCollider.transform.position, tutorialScreen.transform.position);
            
            if (distance > 10f)
                shouldClose = true;
            
            yield return null;
        }
        
        GTPlayer.Instance.disableMovement = false;

        CoroutineManager.Instance.StartCoroutine(ShrinkAndDestroy(tutorialScreen));
    }
    
    private IEnumerator ShrinkAndDestroy(GameObject obj)
    {
        const float Duration = 0.25f;
        float       time     = 0f;
        
        Vector3 startScale = obj.transform.localScale;

        while (time < Duration)
        {
            if (obj == null)
                yield break;
            
            time += Time.deltaTime;

            float t = time / Duration;
            
            obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }
        
        GTPlayer.Instance.disableMovement = false;
        obj.Obliterate();
    }
}