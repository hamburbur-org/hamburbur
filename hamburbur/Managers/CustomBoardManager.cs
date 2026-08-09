using System;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using hamburbur.Components;
using hamburbur.Mods.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace hamburbur.Managers;

public class CustomBoardManager : Singleton<CustomBoardManager>
{
    private const int StumpLeaderboardIndex = 5;

    private static readonly Dictionary<string, BoardInformation> BoardInformations =
            new()
            {
                    ["Canyon2"] = new BoardInformation(
                            "Canyon/CanyonScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-24.5019f, -28.7746f, 0.1f),
                            new Vector3(270f,      0f,        0f),
                            new Vector3(21.5946f,  1f,        22.1782f)
                    ),
                    ["Skyjungle"] = new BoardInformation(
                            "skyjungle/UI/Scoreboard/GorillaScoreBoard",
                            new Vector3(-21.2764f, -32.1928f, 0f),
                            new Vector3(270.2987f, 0.2f,      359.9f),
                            new Vector3(21.6f,     0.1f,      20.4909f)
                    ),
                    ["Mountain"] = new BoardInformation(
                            "Mountain/MountainScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-21.2764f, -32.1928f, 0f),
                            new Vector3(270.2987f, 0.2f,      359.9f),
                            new Vector3(21.6f,     0.1f,      20.4909f)
                    ),
                    ["Metropolis"] = new BoardInformation(
                            "MetroMain/ComputerArea/Scoreboard/GorillaScoreBoard",
                            new Vector3(-25.1f,    -31f,      0.1502f),
                            new Vector3(270.1958f, 0.2086f,   0f),
                            new Vector3(21f,       102.9727f, 21.4f)
                    ),
                    ["Bayou"] = new BoardInformation(
                            "BayouMain/ComputerArea/GorillaScoreBoardPhysical",
                            new Vector3(-28.3419f, -26.851f, 0.3f),
                            new Vector3(270f,      0f,       0f),
                            new Vector3(21.3636f,  38f,      21f)
                    ),
                    ["Beach"] = new BoardInformation(
                            "BeachScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-22.1964f, -33.7126f, 0.1f),
                            new Vector3(270.056f,  0f,        0f),
                            new Vector3(21.2f,     2f,        21.6f)
                    ),
                    ["Cave"] = new BoardInformation(
                            "Cave_Main_Prefab/CrystalCaveScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-22.1964f, -33.7126f, 0.1f),
                            new Vector3(270.056f,  0f,        0f),
                            new Vector3(21.2f,     2f,        21.6f)
                    ),
                    ["Rotating"] = new BoardInformation(
                            "RotatingPermanentEntrance/UI (1)/RotatingScoreboard/RotatingScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-22.1964f, -33.7126f, 0.1f),
                            new Vector3(270.056f,  0f,        0f),
                            new Vector3(21.2f,     2f,        21.6f)
                    ),
                    ["MonkeBlocks"] = new BoardInformation(
                            "Environment Objects/MonkeBlocksRoomPersistent/AtticScoreBoard/AtticScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-22.1964f, -24.5091f, 0.57f),
                            new Vector3(270.1856f, 0.1f,      0f),
                            new Vector3(21.6f,     1.2f,      20.8f)
                    ),
                    ["Basement"] = new BoardInformation(
                            "Basement/BasementScoreboardAnchor/GorillaScoreBoard/",
                            new Vector3(-22.1964f, -24.5091f, 0.57f),
                            new Vector3(270.1856f, 0.1f,      0f),
                            new Vector3(21.6f,     1.2f,      20.8f)
                    ),
                    ["City"] = new BoardInformation(
                            "City_Pretty/CosmeticsScoreboardAnchor/GorillaScoreBoard",
                            new Vector3(-22.1964f, -34.9f, 0.57f),
                            new Vector3(270f,      0f,     0f),
                            new Vector3(21.6f,     2.4f,   22f)
                    ),
            };

    private readonly Dictionary<string, GameObject> objectBoards = new();
    private          GameObject                     board;

    private Renderer computerMonitor;

    private void Start()
    {
        SceneManager.sceneLoaded += SceneLoaded;

        Transform forestBoardParent = GameObject
                                     .Find(
                                              "Environment Objects/LocalObjects_Prefab/Forest/ForestScoreboardAnchor/GorillaScoreBoard")
                                    ?.transform;

        if (forestBoardParent != null)
        {
            board = GameObject.CreatePrimitive(PrimitiveType.Plane);
            board.transform.SetParent(forestBoardParent);
            board.transform.localPosition = new Vector3(-22.1964f, -34.9f, 0.57f);
            board.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            board.transform.localScale    = new Vector3(21.2f, 2f, 21.6f);

            Destroy(board.GetComponent<Collider>());
        }

        ReloadAllBoards();
    }

    private void Update()
    {
        FindComputerMonitor();

        if (computerMonitor != null && computerMonitor.sharedMaterial != CustomBoardMaterial.Current)
            ApplyMaterial(computerMonitor);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= SceneLoaded;

    public void ReloadAllBoards()
    {
        try
        {
            if (board != null)
                ApplyMaterial(board.GetComponent<Renderer>());

            foreach (GameObject objectBoard in objectBoards.Values)
            {
                if (objectBoard == null)
                    continue;

                ApplyMaterial(objectBoard.GetComponent<Renderer>());
            }

            Transform treeRoom = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom")?.transform;

            if (treeRoom != null)
            {
                Transform[] stumpChildren = treeRoom.GetComponentsInChildren<Transform>(true)
                                                    .Where(transform => transform.name.Contains("UnityTempFile"))
                                                    .ToArray();

                if (StumpLeaderboardIndex >= 0 && StumpLeaderboardIndex < stumpChildren.Length)
                {
                    Renderer stumpRenderer = stumpChildren[StumpLeaderboardIndex].GetComponent<Renderer>();
                    ApplyMaterial(stumpRenderer);
                }
            }

            if (PhotonNetworkController.Instance != null)
            {
                foreach (GorillaNetworkJoinTrigger joinTrigger in PhotonNetworkController.Instance.allJoinTriggers)
                    ApplyJoinTriggerMaterial(joinTrigger);

                PhotonNetworkController.Instance.UpdateTriggerScreens();
            }

            FindComputerMonitor();
            ApplyMaterial(computerMonitor);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    private static void ApplyMaterial(Renderer renderer)
    {
        if (renderer == null)
            return;

        renderer.sharedMaterial = CustomBoardMaterial.Current;
    }

    private static void ApplyJoinTriggerMaterial(GorillaNetworkJoinTrigger joinTrigger)
    {
        try
        {
            JoinTriggerUITemplate template = joinTrigger?.ui?.template;

            if (template == null)
                return;

            Material material = CustomBoardMaterial.Current;
            template.ScreenBG_AbandonPartyAndSoloJoin  = material;
            template.ScreenBG_AlreadyInRoom            = material;
            template.ScreenBG_ChangingGameModeSoloJoin = material;
            template.ScreenBG_Error                    = material;
            template.ScreenBG_InPrivateRoom            = material;
            template.ScreenBG_LeaveRoomAndGroupJoin    = material;
            template.ScreenBG_LeaveRoomAndSoloJoin     = material;
            template.ScreenBG_NotConnectedSoloJoin     = material;
        }
        catch
        {
            // ignored
        }
    }

    private void FindComputerMonitor()
    {
        if (computerMonitor != null)
            return;

        computerMonitor = GameObject
                         .Find(
                                  "Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen")
                        ?.GetComponent<Renderer>();
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (BoardInformations.TryGetValue(scene.name, out BoardInformation boardInformation))
            CreateObjectBoard(scene.name,      boardInformation.GameObjectPath, boardInformation.Position,
                    boardInformation.Rotation, boardInformation.Scale);

        ReloadAllBoards();
    }

    private void CreateObjectBoard(string scene, string gameObject, Vector3? position = null, Vector3? rotation = null,
                                   Vector3? scale = null)
    {
        try
        {
            if (objectBoards.TryGetValue(scene, out GameObject existingBoard))
            {
                if (existingBoard != null)
                    Destroy(existingBoard);

                objectBoards.Remove(scene);
            }

            Transform parent = GameObject.Find(gameObject)?.transform;

            if (parent == null)
                return;

            GameObject objectBoard = GameObject.CreatePrimitive(PrimitiveType.Plane);
            objectBoard.transform.SetParent(parent);
            objectBoard.transform.localPosition = position ?? new Vector3(-22.1964f, -34.9f, 0.57f);
            objectBoard.transform.localRotation = Quaternion.Euler(rotation ?? new Vector3(270f, 0f, 0f));
            objectBoard.transform.localScale    = scale ?? new Vector3(21.6f, 2.4f, 22f);

            Destroy(objectBoard.GetComponent<Collider>());
            ApplyMaterial(objectBoard.GetComponent<Renderer>());

            objectBoards.Add(scene, objectBoard);
        }
        catch
        {
            // ignored
        }
    }

    private readonly struct BoardInformation(string path, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        public readonly string  GameObjectPath = path;
        public readonly Vector3 Position       = pos;
        public readonly Vector3 Rotation       = rot;
        public readonly Vector3 Scale          = scale;
    }
}