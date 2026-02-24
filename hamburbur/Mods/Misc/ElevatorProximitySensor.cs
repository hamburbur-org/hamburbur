using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Elevator Proximity Sensor", "Automatically opens/closes elevator when rig is in camera view and close",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class ElevatorProximitySensor : hamburburmod
{
    private const float CheckDelay   = 0.1f;
    private const float TriggerRange = 3.0f;

    private static Camera     elevatorCamera;
    private static GameObject debugCameraCube;
    private static GameObject debugForwardCube;
    private static Transform  rig;

    private static bool  isVisible;
    private static float lastCheck;

    private static readonly Dictionary<GTZone, (Vector3 pos, Vector3 rot)> zoneCamPositions = new()
    {
            { GTZone.forest, (new Vector3(-64.9f, 13.1f, -85f), new Vector3(35f, 320f, 0f)) },

            { GTZone.monkeBlocks, (new Vector3(-131.2001f, 20.5f, -228.7999f), new Vector3(70f, 70f, 0f)) },

            { GTZone.city, (new Vector3(-60f, 18f, -110.2f), new Vector3(60f, 120f, 0f)) },

            {
                    GTZone.ghostReactorTunnel, (new Vector3(0f, 0f, 0f), new Vector3(0f, 10f, 0f))
            }, //not available as of now
    };

    protected override void LateUpdate()
    {
        if (!elevatorCamera || !rig) return;
        if (Time.time < lastCheck + CheckDelay) return;
        lastCheck = Time.time;

        float distance    = Vector3.Distance(elevatorCamera.transform.position, rig.position);
        bool  inView      = elevatorCamera.PointInCameraView(rig.position);
        bool  withinRange = distance <= TriggerRange;

        bool shouldBeVisible = inView && withinRange;

        debugForwardCube.GetComponent<Renderer>().material.color =
                shouldBeVisible ? Color.green : Color.red;

        if (shouldBeVisible && !isVisible)
        {
            isVisible = true;
            OnRigEnteredView();
        }
        else if (!shouldBeVisible && isVisible)
        {
            isVisible = false;
            OnRigLeftView();
        }
    }

    protected override void OnEnable()
    {
        GameObject camObj = GameObject.Find("ElevatorCamera");
        if (camObj == null)
        {
            camObj                    = new GameObject("ElevatorCamera");
            elevatorCamera            = camObj.AddComponent<Camera>();
            elevatorCamera.enabled    = false;
            camObj.transform.position = new Vector3(-64.9f, 13.1f, -85f);
            camObj.transform.rotation = Quaternion.Euler(35f, 320f, 0f);
        }
        else
        {
            elevatorCamera         = camObj.GetComponent<Camera>() ?? camObj.AddComponent<Camera>();
            elevatorCamera.enabled = false;
        }

        debugCameraCube      = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCameraCube.name = "ElevatorCamera_DebugCube";
        debugCameraCube.transform.SetParent(elevatorCamera.transform, false);
        debugCameraCube.transform.localScale                    = new Vector3(0.2f, 0.2f, 0.2f);
        debugCameraCube.transform.localPosition                 = Vector3.zero;
        debugCameraCube.GetComponent<Renderer>().material.color = Color.yellow;

        debugForwardCube      = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugForwardCube.name = "ElevatorCamera_ForwardIndicator";
        debugForwardCube.transform.SetParent(elevatorCamera.transform, false);
        debugForwardCube.transform.localScale                    = new Vector3(0.05f, 0.05f, 0.3f);
        debugForwardCube.transform.localPosition                 = new Vector3(0f,    0f,    0.4f);
        debugForwardCube.GetComponent<Renderer>().material.color = Color.red;

        rig       = VRRig.LocalRig?.transform;
        isVisible = false;

        ZoneManagement.OnZoneChange += ZoneData;
    }

    protected override void OnDisable()
    {
        ZoneManagement.OnZoneChange += ZoneData;
        SetElevatorDoorState(false);
        isVisible = false;

        if (debugCameraCube) debugCameraCube.Obliterate();
        if (debugForwardCube) debugForwardCube.Obliterate();
    }

    public void ZoneData(ZoneData[] zones)
    {
        IEnumerable<GTZone> activeZones = zones.Where(zone => zone.active).Select(zone => zone.zone);
        ChangeCamPosition(activeZones.ToArray());
    }

    private void ChangeCamPosition(GTZone[] zones)
    {
        foreach (GTZone zone in zones)
            if (zoneCamPositions.TryGetValue(zone, out (Vector3 pos, Vector3 rot) camData))
            {
                elevatorCamera.transform.position = camData.pos;
                elevatorCamera.transform.rotation = Quaternion.Euler(camData.rot);

                return;
            }
    }

    private static void OnRigEnteredView() => SetElevatorDoorState(true);

    private static void OnRigLeftView() => SetElevatorDoorState(false);

    public static void SetElevatorDoorState(bool state)
    {
        GRElevatorManager.ElevatorButtonPressed(
                state ? GRElevator.ButtonType.Open : GRElevator.ButtonType.Close,
                GRElevatorManager._instance.currentLocation
        );

        Tools.Utils.RPCProtection();
    }
}

/* https://github.com/developer9998/MonkePhone/tree/main/MonkePhone/Extensions
   MIT License

   Copyright (c) 2025 Dane "Dev

   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:

   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.

   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
*/
public static class CameraAndMathExtensions
{
    public static bool Is01(this float value) => value > 0f && value < 1f;

    public static bool PointInCameraView(this Camera camera,          Vector3 point, bool useLayers = false,
                                         UnityLayer  targetLayer = 0, int     layerMask = -5)
    {
        Vector3 viewport        = camera.WorldToViewportPoint(point);
        bool    inCameraFrustum = viewport.x.Is01() && viewport.y.Is01();
        bool    inFrontOfCamera = viewport.z > 0;

        bool  objectBlockingPoint = false;
        float distance            = Vector3.Distance(camera.transform.position, point);

        if (useLayers)
        {
            Vector3 directionBetween = (point - camera.transform.position).normalized;
            if (Physics.Raycast(camera.transform.position, directionBetween, out RaycastHit hit, distance + 0.05f,
                        layerMask, QueryTriggerInteraction.Collide)
             && hit.transform.gameObject.layer != (int)targetLayer)
                objectBlockingPoint = true;
        }

        return inCameraFrustum && inFrontOfCamera && !objectBlockingPoint && distance < 30f;
    }
}