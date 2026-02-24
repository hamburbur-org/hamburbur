using HarmonyLib;
using UnityEngine;

namespace hamburbur.Patches;

[HarmonyPatch(typeof(GameObject))]
[HarmonyPatch("CreatePrimitive", 0)]
public class CreatePrimitiveShaderPatch : MonoBehaviour
{
    private static void Postfix(GameObject __result)
    {
        __result.GetComponent<Renderer>().material.shader = Plugin.Instance.UberShader;
        __result.GetComponent<Renderer>().material.color  = Plugin.Instance.MainColour;
    }
}