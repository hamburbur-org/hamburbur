using System;
using System.IO;
using hamburbur.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hamburbur.Misc;

public static class CameraCapture
{
    public static void Capture(Camera cam, int width = 1920, int height = 1080)
    {
        RenderTexture rt = new(width, height, 24);
        cam.targetTexture = rt;

        Texture2D screenShot = new(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture    = null;
        RenderTexture.active = null;
        Object.Destroy(rt);

        byte[] bytes      = screenShot.EncodeToPNG();
        string folderPath = Path.Combine(FileManager.Instance.RootHamburburFolder, "Pictures");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string timestamp = DateTime.Now.ToString(@"dd-MM-yyyy_HH-mm-ss");
        string filePath  = Path.Combine(folderPath, $"{timestamp}.png");

        VoiceManager.Get().AudioClip(MenuSoundsHandler.Instance.CameraShutterSound);

        File.WriteAllBytes(filePath, bytes);
    }
}