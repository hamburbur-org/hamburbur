using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Ender Pearl", "Throw one and teleport to where ever it lands", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class EnderPearl : hamburburmod
{
    private static GameObject pearl;
    private static Material   pearlMat;
    private static bool       isRightHandedPearl;

    protected override void LateUpdate()
    {
        bool rightGrab = InputManager.Instance.RightGrip.IsPressed;
        bool leftGrab  = InputManager.Instance.LeftGrip.IsPressed;

        if (rightGrab || leftGrab)
        {
            if (pearl == null)
            {
                pearl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pearl.GetComponent<Collider>().Obliterate();

                pearl.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                if (pearlMat == null)
                {
                    pearlMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                    {
                            color = Plugin.Instance.MainColour,
                    };

                    pearlMat.SetFloat("_Surface",  1);
                    pearlMat.SetFloat("_Blend",    0);
                    pearlMat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    pearlMat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    pearlMat.SetFloat("_ZWrite",   0);
                    pearlMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    pearlMat.renderQueue = (int)RenderQueue.Transparent;
                }

                pearl.GetComponent<Renderer>().material = pearlMat;
            }

            if (pearl.GetComponent<Rigidbody>() != null)
                pearl.GetComponent<Rigidbody>().Obliterate();

            isRightHandedPearl = rightGrab;
            pearl.transform.position = rightGrab
                                               ? GorillaTagger.Instance.rightHandTransform.position
                                               : GorillaTagger.Instance.leftHandTransform.position;
        }
        else
        {
            if (pearl != null)
            {
                if (pearl.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody comp = pearl.AddComponent(typeof(Rigidbody)) as Rigidbody;
                    comp.linearVelocity = isRightHandedPearl
                                                  ? GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true,
                                                          0)
                                                  : GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true,
                                                          0);
                }

                Physics.Raycast(pearl.transform.position, pearl.GetComponent<Rigidbody>().linearVelocity,
                        out RaycastHit Ray, 0.25f, GTPlayer.Instance.locomotionEnabledLayers);

                if (Ray.collider != null)
                {
                    Tools.Utils.TeleportPlayer(pearl.transform.position);

                    if (PhotonNetwork.InRoom)
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 84, true, 999999f);
                    else
                        VRRig.LocalRig.PlayHandTapLocal(84, true, 999999f);

                    Tools.Utils.RPCProtection();
                    pearl.Obliterate();
                }
            }
        }

        if (pearl == null)
            return;

        pearl.GetComponent<Rigidbody>()?.AddForce(Vector3.up * (Time.deltaTime * (6.66f / Time.deltaTime)),
                ForceMode.Acceleration);
    }

    protected override void OnDisable()
    {
        if (pearl != null)
            pearl.Obliterate();
    }
}