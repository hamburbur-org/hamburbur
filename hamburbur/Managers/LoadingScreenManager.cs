using System.Collections;
using GorillaLocomotion;
using hamburbur.Components;
using hamburbur.Tools;
using UnityEngine;

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
}