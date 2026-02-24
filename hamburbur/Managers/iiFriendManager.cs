using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaExtensions;
using hamburbur.Components;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Managers;

public class iiFriendManager : Singleton<iiFriendManager>
{
    private const byte  FriendByte     = 53;
    private const float RigDespawnTime = 0.3f;

    private readonly Dictionary<VRRig, FakeRig> fakeRigs = [];

    private readonly Dictionary<VRRig, GameObject> leftPlatforms  = new();
    private readonly Dictionary<VRRig, GameObject> rightPlatforms = new();

    private void Start()
    {
        PhotonNetwork.NetworkingClient.EventReceived += EventReceived;
        RigUtils.OnRigUnloaded += rig =>
                                  {
                                      if (leftPlatforms.TryGetValue(rig, out GameObject left))
                                      {
                                          Destroy(left);
                                          leftPlatforms.Remove(rig);
                                      }

                                      if (rightPlatforms.TryGetValue(rig, out GameObject right))
                                      {
                                          Destroy(right);
                                          rightPlatforms.Remove(rig);
                                      }
                                  };
    }

    private void Update()
    {
        List<VRRig> toRemove = [];

        foreach ((VRRig rig, FakeRig fakeRig) in fakeRigs)
        {
            if (Time.time - fakeRig.LastUpdateTime > RigDespawnTime)
            {
                toRemove.Add(rig);

                continue;
            }

            fakeRig.Tick();
        }

        foreach (VRRig rig in toRemove)
        {
            fakeRigs[rig].Destroy();
            fakeRigs.Remove(rig);
        }
    }

    private void EventReceived(EventData eventData)
    {
        try
        {
            NetPlayer sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(eventData.Sender);

            if (eventData.Code != FriendByte)
                return;

            VRRig    senderRig = sender.Rig();
            object[] args      = eventData.CustomData == null ? [] : (object[])eventData.CustomData;
            string   command   = args.Length          > 0 ? (string)args[0] : "";

            switch (command)
            {
                case "rig":
                {
                    Vector3    headPos = (Vector3)((object[])args[1])[0];
                    Quaternion headRot = (Quaternion)((object[])args[1])[1];

                    Vector3    leftPos = (Vector3)((object[])args[2])[0];
                    Quaternion leftRot = (Quaternion)((object[])args[2])[1];

                    Vector3    rightPos = (Vector3)((object[])args[3])[0];
                    Quaternion rightRot = (Quaternion)((object[])args[3])[1];

                    if (fakeRigs.TryGetValue(senderRig, out FakeRig fakeRig))
                        fakeRig.UpdateTargets(headPos, headRot, leftPos, leftRot, rightPos, rightRot);
                    else
                        fakeRigs[senderRig] = new FakeRig(senderRig.playerColor, headPos, headRot, leftPos, leftRot,
                                rightPos, rightRot, Plugin.Instance.DiloWorldFont, true,
                                senderRig.Creator.SanitizedNickName);

                    break;
                }

                case "platformSpawn":
                {
                    bool       leftHand = (bool)args[1];
                    Vector3    position = (Vector3)args[2];
                    Quaternion rotation = (Quaternion)args[3];

                    Vector3       scale     = ((Vector3)args[4]).ClampMagnitudeSafe(1f);
                    PrimitiveType spawnType = (PrimitiveType)(int)args[5];

                    if (!position.IsValid() || !scale.IsValid())
                        break;

                    Dictionary<VRRig, GameObject> targetDictionary = leftHand ? leftPlatforms : rightPlatforms;
                    if (targetDictionary.TryGetValue(senderRig, out GameObject platform))
                    {
                        Destroy(platform);
                        targetDictionary.Remove(senderRig);
                    }

                    platform                      = GameObject.CreatePrimitive(spawnType);
                    platform.transform.position   = position;
                    platform.transform.rotation   = rotation;
                    platform.transform.localScale = scale;

                    platform.GetComponent<Renderer>().material.color = senderRig.playerColor;
                    Destroy(platform.GetComponent<Collider>());

                    targetDictionary.Add(senderRig, platform);

                    break;
                }

                case "platformDespawn":
                {
                    bool leftHand = (bool)args[1];

                    Dictionary<VRRig, GameObject> targetDictionary = leftHand ? leftPlatforms : rightPlatforms;
                    if (targetDictionary.TryGetValue(senderRig, out GameObject platform))
                    {
                        Destroy(platform);
                        targetDictionary.Remove(senderRig);
                    }

                    break;
                }
            }
        }
        catch
        {
            // ignored
        }
    }
}