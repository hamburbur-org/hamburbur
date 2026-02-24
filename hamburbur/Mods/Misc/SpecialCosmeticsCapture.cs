using System;
using System.Collections;
using System.IO;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Server_API;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hamburbur.Mods.Misc;

[hamburburmod("Special Cosmetics Capture", "Takes a screenshot, you can change the camera in settings",
        ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpecialCosmeticsCapture : hamburburmod
{
    /*protected override void OnEnable() => TelemetryHandler.OnRoomDataReceived += TakePhoto;

    protected override void OnDisable() => TelemetryHandler.OnRoomDataReceived -= TakePhoto;

    private void TakePhoto(JToken data) => CoroutineManager.Instance.StartCoroutine(CaptureScreenshot(data));

    private IEnumerator CaptureScreenshot(JToken data)
    {
        if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.RoomName != data["roomCode"].ToObject<string>())
            yield break;

        yield return new WaitForSeconds(1);

        VRRig rig = data["userId"].ToObject<string>().Rig();

        if (rig == null)
            yield break;

        yield return new WaitForSeconds(1);

        string folderPath = Path.Combine(FileManager.Instance.RootHamburburFolder, "Pictures");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string timestamp = DateTime.Now.ToString(@"dd-MM-yyyy_HH-mm-ss");
        string fileName =
                $"{(data["isUserKnown"].ToObject<bool>() ? data["username"].ToObject<string>() : rig.OwningNetPlayer().SanitizedNickName)}-{data["userId"].ToObject<string>()}-{timestamp}.png";

        string filePath = Path.Combine(folderPath, fileName);

        Transform targetHead = rig.head.rigTarget.transform;

        GameObject cameraObj = new("TempRigCam");
        Camera     cam       = cameraObj.AddComponent<Camera>();

        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.fieldOfView     = 60f;
        cam.nearClipPlane   = 0.7f;
        cam.farClipPlane    = 1.12f;

        cam.transform.position = targetHead.transform.TransformPoint(0f, 0f, 1f);
        cam.transform.LookAt(targetHead.position + new Vector3(0f, 0f, 0f));

        const int Width  = 4096;
        const int Height = 4096;

        RenderTexture rt = new(Width, Height, 24);
        cam.targetTexture = rt;

        Texture2D screenshot = new(Width, Height, TextureFormat.RGBA32, false);

        cam.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        RenderTexture.active = null;
        cam.targetTexture    = null;

        Object.Destroy(rt);
        Object.Destroy(screenshot);
        Object.Destroy(cameraObj);
    }*/
}