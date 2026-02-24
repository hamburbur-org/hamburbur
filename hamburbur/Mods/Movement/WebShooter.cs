using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Web Shooters", "Gives your spider mans web shooters", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class WebShooter : hamburburmod
{
    private static Vector3     rightGrapplePoint;
    private static Vector3     leftGrapplePoint;
    private static SpringJoint rightJoint;
    private static SpringJoint leftJoint;
    private static bool        isLeftGrappling;
    private static bool        isRightGrappling;

    protected override void LateUpdate()
    {
        bool leftGrab  = InputManager.Instance.LeftGrip.IsPressed;
        bool rightGrab = InputManager.Instance.RightGrip.IsPressed;

        if (leftGrab)
        {
            if (!isLeftGrappling)
            {
                isLeftGrappling = true;
                GorillaTagger.Instance.rigidbody.linearVelocity +=
                        GorillaTagger.Instance.leftHandTransform.forward * 5f;

                if (PhotonNetwork.InRoom)
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, true, 999999f);
                else
                    VRRig.LocalRig.PlayHandTapLocal(89, true, 999999f);

                Tools.Utils.RPCProtection();

                leftGrapplePoint = GorillaTagger.Instance.leftHandTransform.position +
                                   GorillaTagger.Instance.leftHandTransform.forward * 16f;

                leftJoint                              = GorillaTagger.Instance.gameObject.AddComponent<SpringJoint>();
                leftJoint.autoConfigureConnectedAnchor = false;
                leftJoint.connectedAnchor              = leftGrapplePoint;

                float leftDistanceFromPoint =
                        Vector3.Distance(GorillaTagger.Instance.rigidbody.position, leftGrapplePoint);

                leftJoint.maxDistance = leftDistanceFromPoint * 0.8f;
                leftJoint.minDistance = leftDistanceFromPoint * 0.25f;

                leftJoint.spring    = 10f;
                leftJoint.damper    = 50f;
                leftJoint.massScale = 12f;
            }

            GameObject   line  = new("Line");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.numCapVertices    = 10;
            liner.numCornerVertices = 5;

            liner.startColor    = Plugin.Instance.MainColour;
            liner.endColor      = Plugin.Instance.MainColour;
            liner.startWidth    = 0.025f;
            liner.endWidth      = 0.025f;
            liner.positionCount = 2;
            liner.useWorldSpace = true;
            liner.SetPosition(0, GorillaTagger.Instance.leftHandTransform.position);
            liner.SetPosition(1, leftGrapplePoint);
            liner.material.shader = Shader.Find("Sprites/Default");
            line.Obliterate(Time.deltaTime);
        }
        else
        {
            Vector3 endPosition = GorillaTagger.Instance.leftHandTransform.position +
                                  GorillaTagger.Instance.leftHandTransform.forward * 16f;

            GameObject   line  = new("Line");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.numCapVertices    = 10;
            liner.numCornerVertices = 5;

            liner.material.shader = Shader.Find("Sprites/Default");
            liner.startColor      = Plugin.Instance.SecondaryColour;
            liner.endColor        = Plugin.Instance.SecondaryColour;
            liner.startWidth      = 0.025f;
            liner.endWidth        = 0.025f;
            liner.positionCount   = 2;
            liner.useWorldSpace   = true;
            liner.SetPosition(0, GorillaTagger.Instance.leftHandTransform.position);
            liner.SetPosition(1, endPosition);
            line.Obliterate(Time.deltaTime);

            isLeftGrappling = false;
            leftJoint.Obliterate();
        }

        if (rightGrab)
        {
            if (!isRightGrappling)
            {
                isRightGrappling = true;
                GorillaTagger.Instance.rigidbody.linearVelocity +=
                        GorillaTagger.Instance.rightHandTransform.forward * 5f;

                if (PhotonNetwork.InRoom)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, false, 999999f);
                    Tools.Utils.RPCProtection();
                }
                else
                {
                    VRRig.LocalRig.PlayHandTapLocal(89, false, 999999f);
                }

                rightGrapplePoint = GorillaTagger.Instance.rightHandTransform.position +
                                    GorillaTagger.Instance.rightHandTransform.forward * 16f;

                if (rightGrapplePoint == Vector3.zero)
                    rightGrapplePoint = GorillaTagger.Instance.rightHandTransform.position +
                                        GorillaTagger.Instance.rightHandTransform.forward * 512f;

                rightJoint                              = GorillaTagger.Instance.gameObject.AddComponent<SpringJoint>();
                rightJoint.autoConfigureConnectedAnchor = false;
                rightJoint.connectedAnchor              = rightGrapplePoint;

                float rightdistanceFromPoint =
                        Vector3.Distance(GorillaTagger.Instance.rigidbody.position, rightGrapplePoint);

                rightJoint.maxDistance = rightdistanceFromPoint * 0.8f;
                rightJoint.minDistance = rightdistanceFromPoint * 0.25f;

                rightJoint.spring    = 10f;
                rightJoint.damper    = 50f;
                rightJoint.massScale = 12f;
            }

            GameObject   line  = new("Line");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.numCapVertices    = 10;
            liner.numCornerVertices = 5;

            liner.startColor    = Plugin.Instance.MainColour;
            liner.endColor      = Plugin.Instance.MainColour;
            liner.startWidth    = 0.025f;
            liner.endWidth      = 0.025f;
            liner.positionCount = 2;
            liner.useWorldSpace = true;
            liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
            liner.SetPosition(1, rightGrapplePoint);
            liner.material.shader = Shader.Find("Sprites/Default");
            liner.Obliterate(Time.deltaTime);
        }
        else
        {
            Vector3 EndPosition = GorillaTagger.Instance.rightHandTransform.position +
                                  GorillaTagger.Instance.rightHandTransform.forward * 16f;

            GameObject   line  = new("Line");
            LineRenderer liner = line.AddComponent<LineRenderer>();
            liner.numCapVertices    = 10;
            liner.numCornerVertices = 5;

            liner.material.shader = Shader.Find("Sprites/Default");
            liner.startColor      = Plugin.Instance.SecondaryColour;
            liner.endColor        = Plugin.Instance.SecondaryColour;
            liner.startWidth      = 0.025f;
            liner.endWidth        = 0.025f;
            liner.positionCount   = 2;
            liner.useWorldSpace   = true;
            liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
            liner.SetPosition(1, EndPosition);
            line.Obliterate(Time.deltaTime);

            isRightGrappling = false;
            rightJoint.Obliterate();
        }
    }

    protected override void OnDisable()
    {
        isLeftGrappling  = false;
        isRightGrappling = false;

        leftJoint.Obliterate();
        rightJoint.Obliterate();
    }
}