using System.Collections.Generic;
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using TMPro;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Info Nametags", "Custom nametags with information above their head", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class InfoNametags : hamburburmod
{
    public static GameObject NametagPrefab;

    private readonly Dictionary<VRRig, InfoNametagsComp> nametags = new();

    protected override void Start() =>
            NametagPrefab = Plugin.Instance.HamburburBundle.LoadAsset<GameObject>("NametagCanvas");

    protected override void OnEnable()
    {
        if (NetworkSystem.Instance.InRoom)
            foreach (VRRig rig in NetworkSystem.Instance.Rigs())
                AddNametag(rig);

        RigUtils.OnRigLoaded   += AddNametag;
        RigUtils.OnRigUnloaded += RemoveNametag;
    }

    protected override void OnDisable()
    {
        if (NetworkSystem.Instance.InRoom)
            foreach (VRRig rig in NetworkSystem.Instance.Rigs())
                RemoveNametag(rig);

        RigUtils.OnRigLoaded   -= AddNametag;
        RigUtils.OnRigUnloaded -= RemoveNametag;
    }

    private void AddNametag(VRRig rig)
    {
        if (rig == null || nametags.ContainsKey(rig))
            return;

        InfoNametagsComp nametagComp = rig.gameObject.AddComponent<InfoNametagsComp>();

        nametags.Add(rig, nametagComp);
    }

    private void RemoveNametag(VRRig rig)
    {
        if (rig == null)
            return;

        if (!nametags.Remove(rig, out InfoNametagsComp nametagComp))
            return;
 
        if (nametagComp != null)
            Object.Destroy(nametagComp);
    }
}

public class InfoNametagsComp : MonoBehaviour
{
    public GameObject Nametag;

    public TextMeshProUGUI NametagText, FpsText, PingText, VelocityText, PlatformText, CreationText, IdText;

    private string lastNickname;

    private NetPlayer player;

    private void Start()
    {
        player = GetComponent<VRRig>().creator;

        CreateNametag();
        RefreshName();
        RefreshCreationDate();
    }

    private void Update()
    {
        if (Nametag == null)
            return;

        Nametag.transform.LookAt(GTPlayer.Instance.mainCamera.transform);
        Nametag.transform.Rotate(0f, 180f, 0f);

        RefreshName();
        UpdateInfo();
    }

    private void OnDestroy()
    {
        if (Nametag != null)
            Destroy(Nametag);
    }

    private void UpdateInfo()
    {
        if (player       == null ||
            FpsText      == null ||
            PingText     == null ||
            VelocityText == null ||
            PlatformText == null ||
            IdText       == null)
            return;

        VRRig rig = player.Rig();

        if (rig == null)
            return;

        FpsText.text      = $"{rig.fps} FPS";
        PingText.text     = $"{rig.Ping()}ms";
        VelocityText.text = $"{rig.LatestVelocity().magnitude:F1} M/s";

        if (player.GetPlayerRef().CustomProperties.TryGetValue(PlayerConfig.Player_Platform, out object platform))
            PlatformText.text = platform?.ToString() ?? "Unknown";
        else
            PlatformText.text = "Unknown";

        IdText.text = player.UserId;
    }

    private async void RefreshCreationDate()
    {
        if (player == null || CreationText == null)
            return;

        VRRig rig = player.Rig();

        if (rig == null)
            return;

        try
        {
            object creationDate = await rig.GetCreationDate();

            if (CreationText != null)
                CreationText.text = creationDate?.ToString() ?? "Unknown";
        }
        catch
        {
            if (CreationText != null)
                CreationText.text = "Unknown";
        }
    }

    private void RefreshName()
    {
        player ??= GetComponent<VRRig>().creator;

        if (player == null || NametagText == null)
            return;

        string nickname = player.SanitizedNickName;

        if (nickname == lastNickname)
            return;

        lastNickname     = nickname;
        NametagText.text = nickname;
    }

    private void CreateNametag()
    {
        Nametag                         = Instantiate(InfoNametags.NametagPrefab, transform, true);
        Nametag.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        NametagText  = Nametag.transform.Find("Background").GetComponentInChildren<TextMeshProUGUI>();
        FpsText      = Nametag.transform.Find("Fps").GetComponentInChildren<TextMeshProUGUI>();
        PingText     = Nametag.transform.Find("Ping").GetComponentInChildren<TextMeshProUGUI>();
        VelocityText = Nametag.transform.Find("Velocity").GetComponentInChildren<TextMeshProUGUI>();
        PlatformText = Nametag.transform.Find("Platform").GetComponentInChildren<TextMeshProUGUI>();
        CreationText = Nametag.transform.Find("Creation").GetComponentInChildren<TextMeshProUGUI>();
        IdText       = Nametag.transform.Find("Id").GetComponentInChildren<TextMeshProUGUI>();
    }
}