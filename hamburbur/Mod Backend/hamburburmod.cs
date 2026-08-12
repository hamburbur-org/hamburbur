using System;
using System.Collections.Generic;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mod_Backend;

// ReSharper disable once InconsistentNaming
public class hamburburmod
{
    public static int EnabledStateRevision { get; private set; }

    public hamburburmodAttribute AssociatedAttribute;
    public GameObject AssociatedGUIButton;

    public bool Enabled { get; private set; }
    public int IncrementalValue;
    public bool LoadSavedDataWhenStartCalled;

    public            string PreferencesKey   => AssociatedAttribute?.Name ?? GetType().Name;
    public virtual    string ModName          => AssociatedAttribute.Name;
    protected bool    IsUserInitiatedToggle { get; private set; }
    protected virtual Type[] Dependencies     => [];
    protected virtual Type[] IncompatibleMods => [];

    internal ModTickPhase TickPhases { get; private set; }

    private readonly HashSet<Type> modsDisabledByCompatibilitySystem = [];
    private readonly HashSet<Type> modsEnabledByCompatibilitySystem = [];

    private bool hasStarted;
    private ModSaveInfo pendingSavedData;

    public void InvokeStart()
    {
        if (hasStarted)
            return;

        if (AssociatedAttribute == null)
        {
            Debug.LogError($"[hamburbur] Missing hamburburmodAttribute on {GetType().FullName}");
            return;
        }

        Enabled = false;
        IncrementalValue = AssociatedAttribute.IncrementalValue;
        TickPhases = ModRuntime.GetTickPhases(GetType());

        Start();

        hasStarted = true;

        if (LoadSavedDataWhenStartCalled &&
            ButtonHandler.SavedModInfo.TryGetValue(PreferencesKey, out ModSaveInfo savedModInfo))
        {
            pendingSavedData = savedModInfo;
        }

        if (pendingSavedData != null)
        {
            LoadSavedData(pendingSavedData);
            pendingSavedData = null;
        }
        else
        {
            SetEnabled(GetDefaultEnabledState(), false, false);
        }
    }

    public void LoadSavedData(ModSaveInfo savedModInfo)
    {
        if (savedModInfo == null)
            return;

        if (!hasStarted)
        {
            pendingSavedData = savedModInfo;
            return;
        }

        IncrementalValue = savedModInfo.IncrementalValue;
        OnIncrementalStateLoaded();

        if (!CanLoadSavedEnabledState())
            return;

        SetEnabled(savedModInfo.Enabled, false, false);
    }

    public void Toggle(
            ButtonState buttonState,
            bool playNotification = true,
            bool careAboutDependenciesAndIncompatibleMods = true)
    {
        int buttonUpdateRevision = ButtonHandler.UpdateRevision;

        switch (AssociatedAttribute.ButtonType)
        {
            case ButtonType.Togglable:
                IsUserInitiatedToggle = playNotification;

                try
                {
                    SetEnabled(!Enabled, playNotification, careAboutDependenciesAndIncompatibleMods);
                }
                finally
                {
                    IsUserInitiatedToggle = false;
                }

                break;

            case ButtonType.Category:
            case ButtonType.Fixed:
                Pressed();
                Notify("Pressed", "yellow", playNotification);
                break;

            case ButtonType.Incremental:
                HandleIncrementalButton(buttonState, playNotification);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Avoid immediately rebuilding a page that was already updated inside the button action.
        if (ButtonHandler.UpdateRevision == buttonUpdateRevision)
            ButtonHandler.Instance?.UpdateButtons();
    }

    internal void SetEnabledFromSystem(bool enabled) => SetEnabled(enabled, false, false);

    private void SetEnabled(
            bool enabled,
            bool playNotification = true,
            bool careAboutDependenciesAndIncompatibleMods = true)
    {
        if (AssociatedAttribute.ButtonType != ButtonType.Togglable)
            return;

        if (Enabled == enabled)
            return;

        switch (enabled)
        {
            case true when careAboutDependenciesAndIncompatibleMods:
                EnableRequiredMods();

                break;

            case false when careAboutDependenciesAndIncompatibleMods:
                RestoreCompatibilityChanges();

                break;
        }

        Enabled = enabled;
        EnabledStateRevision++;

        if (Enabled)
        {
            OnEnable();
            ModRuntime.Register(this);
        }
        else
        {
            ModRuntime.Unregister(this);
            OnDisable();
        }

        Notify(
                Enabled ? "Enabled" : "Disabled",
                Enabled ? "green" : "red",
                playNotification);
    }

    private void HandleIncrementalButton(ButtonState buttonState, bool playNotification)
    {
        switch (buttonState)
        {
            case ButtonState.Increment:
                Increment();
                Notify("Incremented", "yellow", playNotification);
                break;

            case ButtonState.Decrement:
                Decrement();
                Notify("Decremented", "yellow", playNotification);
                break;

            case ButtonState.Normal:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
        }
    }

    private void EnableRequiredMods()
    {
        foreach (Type dependencyType in Dependencies)
        {
            if (!ModRegistry.TryGet(dependencyType, out hamburburmod mod))
                continue;

            if (mod.Enabled)
                continue;

            mod.SetEnabled(true, false, false);
            modsEnabledByCompatibilitySystem.Add(dependencyType);
        }

        foreach (Type incompatibleType in IncompatibleMods)
        {
            if (!ModRegistry.TryGet(incompatibleType, out hamburburmod mod))
                continue;

            if (!mod.Enabled)
                continue;

            mod.SetEnabled(false, false, false);
            modsDisabledByCompatibilitySystem.Add(incompatibleType);
        }
    }

    private void RestoreCompatibilityChanges()
    {
        foreach (Type dependencyType in modsEnabledByCompatibilitySystem)
            if (ModRegistry.TryGet(dependencyType, out hamburburmod mod))
                mod.SetEnabled(false, false, false);

        foreach (Type incompatibleType in modsDisabledByCompatibilitySystem)
            if (ModRegistry.TryGet(incompatibleType, out hamburburmod mod))
                mod.SetEnabled(true, false, false);

        modsEnabledByCompatibilitySystem.Clear();
        modsDisabledByCompatibilitySystem.Clear();
    }

    private bool GetDefaultEnabledState() =>
            AssociatedAttribute.EnabledType is EnabledType.Enabled or EnabledType.AlwaysEnabled;

    private bool CanLoadSavedEnabledState() =>
            AssociatedAttribute.ButtonType == ButtonType.Togglable &&
            AssociatedAttribute.EnabledType is not EnabledType.AlwaysEnabled and not EnabledType.AlwaysDisabled;

    private void Notify(string state, string colour, bool playNotification)
    {
        if (!playNotification || ModNotifications.Instance?.Enabled != true)
            return;

        NotificationManager.SendNotification(
                $"<color={colour}>{state}</color>",
                state == "Disabled"
                        ? ModName
                        : $"{ModName}: {AssociatedAttribute.Description}",
                5f,
                false,
                false);
    }

    protected virtual void Start() { }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void Update() { }
    protected virtual void LateUpdate() { }
    protected virtual void FixedUpdate() { }
    protected virtual void OnGUI() { }
    protected virtual void Pressed() { }
    protected virtual void Increment() { }
    protected virtual void Decrement() { }
    protected virtual void OnIncrementalStateLoaded() { }

    internal void InvokeUpdate() => Update();
    internal void InvokeLateUpdate() => LateUpdate();
    internal void InvokeFixedUpdate() => FixedUpdate();
    internal void InvokeOnGUI() => OnGUI();
}
