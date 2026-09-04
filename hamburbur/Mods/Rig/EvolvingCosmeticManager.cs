using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Rig;

public class EvolvingCosmeticManager : MonoBehaviour
{
    public const string CategoryName = "Evolving Cosmetic";

    private const string DetailCategoryPrefix = "Evolving Cosmetic ";

    private readonly List<EvolvingCosmeticInfo> cosmetics = [];
    private readonly HashSet<string> detailCategories = [];

    private CosmeticsController subscribedController;
    private Coroutine refreshCoroutine;
    private int refreshVersion;

    public static EvolvingCosmeticManager Instance { get; private set; }

    private IEnumerator Start()
    {
        Instance = this;
        RigUtils.OnRigCosmeticsLoaded += OnRigCosmeticsLoaded;

        while (hamburbur.Plugin.Instance == null || !hamburbur.Plugin.Instance.MenuLoaded)
            yield return null;

        yield return new WaitForSeconds(0.25f);

        SubscribeToCosmeticChanges();
        RefreshCosmetics();
    }

    private void Update()
    {
        if (subscribedController != CosmeticsController.instance)
            SubscribeToCosmeticChanges();
    }

    private void OnDestroy()
    {
        if (subscribedController != null)
            subscribedController.OnCosmeticsUpdated -= RefreshCosmetics;

        RigUtils.OnRigCosmeticsLoaded -= OnRigCosmeticsLoaded;

        if (Instance == this)
            Instance = null;
    }

    public void RefreshCosmetics()
    {
        refreshVersion++;

        if (refreshCoroutine != null)
            StopCoroutine(refreshCoroutine);

        refreshCoroutine = StartCoroutine(RefreshCosmeticsRoutine(refreshVersion));
    }

    public void OpenDetails(EvolvingCosmeticInfo cosmeticInfo)
    {
        if (cosmeticInfo?.Cosmetic == null)
            return;

        string category = GetDetailCategory(cosmeticInfo.ItemId);
        BuildDetailCategory(cosmeticInfo, category);
        ButtonHandler.Instance.SetCategory(category);
    }

    public static void SetStage(EvolvingCosmetic cosmetic, int stage)
    {
        if (cosmetic == null || stage < 0 || stage >= cosmetic.ageAwareGameObjects.Length)
            return;

        int minimumDays = cosmetic.ageAwareGameObjects[stage].minActiveDays;
        cosmetic._daysAccrued = Mathf.Max(cosmetic._daysAccrued.GetValueOrDefault(), minimumDays);
        cosmetic.SelectedObjectIndex = stage;
        cosmetic.ActivateSelectedIndex();
        VRRig.LocalRig?.reliableState?.SetIsDirty();
    }

    public static void SetLocalAvailabilityAge(EvolvingCosmetic cosmetic, int days)
    {
        if (cosmetic == null)
            return;

        cosmetic._daysAccrued = Mathf.Max(0, days);
        cosmetic.ActivateSelectedIndex();
    }

    public static void ResetLocalData(EvolvingCosmetic cosmetic)
    {
        if (cosmetic == null)
            return;

        cosmetic.UpdateDaysAccrued();
        VRRig.LocalRig?.reliableState?.SetIsDirty();
    }

    private void SubscribeToCosmeticChanges()
    {
        if (subscribedController != null)
            subscribedController.OnCosmeticsUpdated -= RefreshCosmetics;

        subscribedController = CosmeticsController.instance;

        if (subscribedController != null)
            subscribedController.OnCosmeticsUpdated += RefreshCosmetics;
    }

    private void OnRigCosmeticsLoaded(VRRig rig)
    {
        if (rig == VRRig.LocalRig || rig?.isLocal == true)
            RefreshCosmetics();
    }

    private IEnumerator RefreshCosmeticsRoutine(int version)
    {
        while (CosmeticsController.instance == null ||
               VRRig.LocalRig == null ||
               VRRig.LocalRig.cosmeticsObjectRegistry == null ||
               !CosmeticsV2Spawner_Dirty.isPrepared)
        {
            yield return null;

            if (version != refreshVersion)
                yield break;
        }

        yield return null;

        List<EvolvingCosmeticInfo> discoveredCosmetics = [];
        HashSet<string> discoveredIds = [];
        CosmeticsController controller = CosmeticsController.instance;
        CosmeticItemRegistry registry = VRRig.LocalRig.cosmeticsObjectRegistry;
        CosmeticsController.CosmeticItem[] wornItems = controller.currentWornSet.items;

        foreach (CosmeticsController.CosmeticItem item in wornItems)
        {
            if (version != refreshVersion)
                yield break;

            if (item.isNullItem || string.IsNullOrEmpty(item.itemName) || item.itemName == "null")
                continue;

            CosmeticItemInstance itemInstance = null;

            for (int attempt = 0; attempt < 10 && itemInstance == null; attempt++)
            {
                itemInstance = registry.Cosmetic(item.itemName);

                if (itemInstance == null)
                    yield return null;
            }

            if (itemInstance == null)
                continue;

            EvolvingCosmetic evolvingCosmetic = FindEvolvingCosmetic(itemInstance);
            if (evolvingCosmetic == null || !discoveredIds.Add(item.itemName))
                continue;

            string displayName = string.IsNullOrEmpty(item.overrideDisplayName)
                                         ? item.displayName
                                         : item.overrideDisplayName;

            if (string.IsNullOrEmpty(displayName))
                displayName = item.itemName;

            discoveredCosmetics.Add(new EvolvingCosmeticInfo(item.itemName, displayName, evolvingCosmetic));
        }

        cosmetics.Clear();
        cosmetics.AddRange(discoveredCosmetics.OrderBy(info => info.DisplayName, StringComparer.OrdinalIgnoreCase));
        RebuildCosmeticCategory();
        refreshCoroutine = null;
    }

    private static EvolvingCosmetic FindEvolvingCosmetic(CosmeticItemInstance itemInstance)
    {
        IEnumerable<GameObject> cosmeticObjects = itemInstance.objects
                                                              .Concat(itemInstance.leftObjects)
                                                              .Concat(itemInstance.rightObjects)
                                                              .Concat(itemInstance.holdableObjects);

        foreach (GameObject cosmeticObject in cosmeticObjects)
        {
            if (cosmeticObject == null)
                continue;

            EvolvingCosmetic evolvingCosmetic = cosmeticObject.GetComponentInChildren<EvolvingCosmetic>(true);
            if (evolvingCosmetic != null)
                return evolvingCosmetic;
        }

        return null;
    }

    private void RebuildCosmeticCategory()
    {
        string currentCategory = MenuHandler.Instance?.Category;
        string selectedItemId = GetItemIdFromDetailCategory(currentCategory);

        ClearCategory(CategoryName);

        EvolvingCosmeticRefreshButton refreshButton = new();
        refreshButton.ConfigKey = "EvolvingCosmetic_Refresh";
        ButtonHandler.AddButton(CategoryName, refreshButton, false, loadSavedData: false);

        foreach (EvolvingCosmeticInfo cosmeticInfo in cosmetics)
        {
            EvolvingCosmeticEntry button = new(cosmeticInfo);
            button.ConfigKey = $"EvolvingCosmetic_{cosmeticInfo.ItemId}_Open";
            ButtonHandler.AddButton(CategoryName, button, false, loadSavedData: false);
        }

        foreach (string category in detailCategories.Where(category => category != currentCategory).ToArray())
            ClearCategory(category);

        detailCategories.Clear();

        if (!string.IsNullOrEmpty(selectedItemId))
        {
            EvolvingCosmeticInfo selectedCosmetic = cosmetics.FirstOrDefault(info => info.ItemId == selectedItemId);

            if (selectedCosmetic != null)
            {
                BuildDetailCategory(selectedCosmetic, currentCategory);
            }
            else if (MenuHandler.Instance != null)
            {
                ClearCategory(currentCategory);
                ButtonHandler.Instance.SetCategory(CategoryName, false);
            }
        }

        ButtonHandler.Instance?.UpdateButtons();
    }

    private void BuildDetailCategory(EvolvingCosmeticInfo cosmeticInfo, string category)
    {
        bool cycleWasEnabled = Buttons.Categories.TryGetValue(category, out (Type, hamburburmod)[] existingButtons) &&
                               existingButtons.Any(entry => entry.Item2 is EvolvingCosmeticCycle { Enabled: true, });

        ClearCategory(category);
        detailCategories.Add(category);

        EvolvingCosmeticLocalAge localAge = new(cosmeticInfo.Cosmetic);
        localAge.ConfigKey = $"EvolvingCosmetic_{cosmeticInfo.ItemId}_LocalAvailabilityAge";
        ButtonHandler.AddButton(category, localAge, false, loadSavedData: false);

        EvolvingCosmeticReset reset = new(cosmeticInfo.Cosmetic, localAge);
        reset.ConfigKey = $"EvolvingCosmetic_{cosmeticInfo.ItemId}_ResetLocalData";
        ButtonHandler.AddButton(category, reset, false, loadSavedData: false);

        for (int stage = 0; stage < cosmeticInfo.Cosmetic.ageAwareGameObjects.Length; stage++)
        {
            EvolvingCosmeticStage button = new(cosmeticInfo.Cosmetic, stage);
            button.ConfigKey = $"EvolvingCosmetic_{cosmeticInfo.ItemId}_Stage_{stage}";
            ButtonHandler.AddButton(category, button, false, loadSavedData: false);
        }

        EvolvingCosmeticCycle cycle = new(cosmeticInfo.Cosmetic);
        cycle.ConfigKey = $"EvolvingCosmetic_{cosmeticInfo.ItemId}_CycleStages";
        ButtonHandler.AddButton(category, cycle, false);

        if (cycleWasEnabled && !cycle.Enabled)
            cycle.SetEnabledFromSystem(true);
    }

    private static void ClearCategory(string category)
    {
        if (!Buttons.Categories.TryGetValue(category, out (Type, hamburburmod)[] buttons))
        {
            Buttons.Categories[category] = [];
            return;
        }

        foreach (hamburburmod button in buttons.Select(entry => entry.Item2).Where(button => button != null).ToArray())
            ButtonHandler.RemoveButton(button);

        Buttons.Categories[category] = [];
    }

    private static string GetDetailCategory(string itemId) => DetailCategoryPrefix + itemId;

    private static string GetItemIdFromDetailCategory(string category) =>
            category != null && category.StartsWith(DetailCategoryPrefix, StringComparison.Ordinal)
                    ? category.Substring(DetailCategoryPrefix.Length)
                    : null;
}

public sealed class EvolvingCosmeticInfo
{
    public EvolvingCosmeticInfo(string itemId, string displayName, EvolvingCosmetic cosmetic)
    {
        ItemId = itemId;
        DisplayName = displayName;
        Cosmetic = cosmetic;
    }

    public string ItemId { get; }
    public string DisplayName { get; }
    public EvolvingCosmetic Cosmetic { get; }
}

internal sealed class EvolvingCosmeticRefreshButton : hamburburmod
{
    internal EvolvingCosmeticRefreshButton() =>
            AssociatedAttribute = new hamburburmodAttribute("Refresh Cosmetics",
                    "Reload the evolving cosmetics from your currently worn set", ButtonType.Fixed,
                    AccessSetting.Public, EnabledType.Disabled, 0);

    protected override void Pressed() => EvolvingCosmeticManager.Instance?.RefreshCosmetics();
}

internal sealed class EvolvingCosmeticEntry : hamburburmod
{
    private readonly EvolvingCosmeticInfo cosmeticInfo;

    internal EvolvingCosmeticEntry(EvolvingCosmeticInfo cosmeticInfo)
    {
        this.cosmeticInfo = cosmeticInfo;
        AssociatedAttribute = new hamburburmodAttribute(cosmeticInfo.DisplayName,
                $"Open controls for {cosmeticInfo.ItemId}", ButtonType.Category,
                AccessSetting.Public, EnabledType.Disabled, 0);
    }

    protected override void Pressed() => EvolvingCosmeticManager.Instance?.OpenDetails(cosmeticInfo);
}

internal sealed class EvolvingCosmeticStage : hamburburmod
{
    private readonly EvolvingCosmetic cosmetic;
    private readonly int stage;

    internal EvolvingCosmeticStage(EvolvingCosmetic cosmetic, int stage)
    {
        this.cosmetic = cosmetic;
        this.stage = stage;
        int minimumDays = cosmetic.ageAwareGameObjects[stage].minActiveDays;
        AssociatedAttribute = new hamburburmodAttribute($"Stage {stage + 1}",
                $"Send stage {stage + 1} as cosmetic state; remote clients can reject it if their age check fails. Local minimum: {minimumDays} days",
                ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0);
    }

    public override string ModName => cosmetic != null && cosmetic.SelectedObjectIndex == stage
                                              ? $"Stage {stage + 1} [Selected]"
                                              : $"Stage {stage + 1}";

    protected override void Pressed() => EvolvingCosmeticManager.SetStage(cosmetic, stage);
}

internal sealed class EvolvingCosmeticLocalAge : hamburburmod
{
    private readonly EvolvingCosmetic cosmetic;

    internal EvolvingCosmeticLocalAge(EvolvingCosmetic cosmetic)
    {
        this.cosmetic = cosmetic;
        AssociatedAttribute = new hamburburmodAttribute("Local Availability Age",
                "Changes only this client's age value used for stage availability; it is not sent to other players",
                ButtonType.Incremental, AccessSetting.Public, EnabledType.Disabled, 0);
    }

    public override string ModName => $"Local Availability Age: {IncrementalValue} days";

    protected override void Start() => RefreshValue();

    internal void RefreshValue() =>
            IncrementalValue = Mathf.Max(0, cosmetic?._daysAccrued.GetValueOrDefault() ?? 0);

    protected override void Increment()
    {
        IncrementalValue++;
        EvolvingCosmeticManager.SetLocalAvailabilityAge(cosmetic, IncrementalValue);
    }

    protected override void Decrement()
    {
        IncrementalValue = Mathf.Max(0, IncrementalValue - 1);
        EvolvingCosmeticManager.SetLocalAvailabilityAge(cosmetic, IncrementalValue);
    }
}

internal sealed class EvolvingCosmeticReset : hamburburmod
{
    private readonly EvolvingCosmetic cosmetic;
    private readonly EvolvingCosmeticLocalAge localAge;

    internal EvolvingCosmeticReset(EvolvingCosmetic cosmetic, EvolvingCosmeticLocalAge localAge)
    {
        this.cosmetic = cosmetic;
        this.localAge = localAge;
        AssociatedAttribute = new hamburburmodAttribute("Reset Local Data",
                "Recalculate the real local age and stage from item and subscription data",
                ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0);
    }

    protected override void Pressed()
    {
        EvolvingCosmeticManager.ResetLocalData(cosmetic);
        localAge.RefreshValue();
    }
}

internal sealed class EvolvingCosmeticCycle : hamburburmod
{
    private readonly EvolvingCosmetic cosmetic;

    private float nextCycleTime;

    internal EvolvingCosmeticCycle(EvolvingCosmetic cosmetic)
    {
        this.cosmetic = cosmetic;
        AssociatedAttribute = new hamburburmodAttribute("Cycle Stages",
                "Cycle the networked selected stage once per second; remote age validation still applies",
                ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0);
    }

    protected override void OnEnable() => nextCycleTime = Time.time + 1f;

    protected override void Update()
    {
        if (cosmetic == null || cosmetic.ageAwareGameObjects.Length == 0)
            return;

        if (Time.time < nextCycleTime)
            return;

        int nextStage = (cosmetic.SelectedObjectIndex + 1) % cosmetic.ageAwareGameObjects.Length;
        EvolvingCosmeticManager.SetStage(cosmetic, nextStage);
        nextCycleTime = Time.time + 1f;
    }
}
