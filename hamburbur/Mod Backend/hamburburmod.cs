using System;
using System.Collections.Generic;
using System.Linq;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mod_Backend;

public class hamburburmod
{
    public         hamburburmodAttribute AssociatedAttribute;
    public         GameObject            AssociatedGUIButton;
    public         bool                  Enabled;
    public         int                   IncrementalValue;
    public         bool                  LoadSavedDataWhenStartCalled;
    public         string                PreferencesKey   => AssociatedAttribute.Name;
    public virtual string                ModName          => AssociatedAttribute.Name;
    public virtual Type[]                Dependencies     => [];
    public virtual Type[]                IncompatibleMods => [];
    
    private readonly HashSet<Type> modsDisabledByCompatibilitySystem = [];
    private readonly HashSet<Type> modsEnabledByCompatibilitySystem  = [];

    public void InvokeStart()
    {
        Enabled          = AssociatedAttribute.EnabledType is EnabledType.AlwaysEnabled or EnabledType.Enabled;
        IncrementalValue = AssociatedAttribute.IncrementalValue;

        if (LoadSavedDataWhenStartCalled)
            LoadSavedData(ButtonHandler.SavedModInfo.TryGetValue(PreferencesKey, out ModSaveInfo modSaveInfo)
                                  ? modSaveInfo
                                  : new ModSaveInfo
                                  {
                                          Enabled = Enabled, IncrementalValue = IncrementalValue,
                                  });

        Start();

        if (Enabled)
            OnEnable();
    }

    protected virtual void Start()       { }
    protected virtual void OnEnable()    { }
    protected virtual void OnDisable()   { }
    protected virtual void Update()      { }
    protected virtual void LateUpdate()  { }
    protected virtual void FixedUpdate() { }
    protected virtual void OnGUI()       { }
    protected virtual void Pressed()     { }
    protected virtual void Increment()   { }
    protected virtual void Decrement()   { }

    public void LoadSavedData(ModSaveInfo savedModInfo)
    {
        if (AssociatedAttribute.EnabledType is EnabledType.AlwaysEnabled or EnabledType.AlwaysDisabled &&
            AssociatedAttribute.ButtonType == ButtonType.Togglable)
            return;

        if (Enabled != savedModInfo.Enabled && AssociatedAttribute.ButtonType == ButtonType.Togglable)
            Toggle(ButtonState.Normal, false, false);

        IncrementalValue = savedModInfo.IncrementalValue;
        OnIncrementalStateLoaded();
    }

    protected virtual void OnIncrementalStateLoaded() { }

    public void Toggle(ButtonState buttonState, bool playNotification = true,
                       bool        careAboutDependenciesAndIncompatibleMods = true)
    {
        switch (AssociatedAttribute.ButtonType)
        {
            case ButtonType.Togglable:
            {
                Enabled = !Enabled;

                if (careAboutDependenciesAndIncompatibleMods)
                    foreach ((Type modType, hamburburmod mod) in Buttons.Categories.Values.SelectMany(x => x))
                    {
                        if (mod.AssociatedAttribute.ButtonType != ButtonType.Togglable) 
                            continue;

                        switch (Enabled)
                        {
                            case true when (Dependencies?.Contains(modType) == true && !mod.Enabled):
                                mod.Toggle(ButtonState.Normal, false, false);
                                modsEnabledByCompatibilitySystem.Add(modType);

                                break;

                            case false when (Dependencies?.Contains(modType) == true && modsEnabledByCompatibilitySystem.Contains(modType)):
                                mod.Toggle(ButtonState.Normal, false, false);
                                modsEnabledByCompatibilitySystem.Remove(modType);

                                break;
                        }

                        switch (Enabled)
                        {
                            case true when (IncompatibleMods?.Contains(modType) == true && mod.Enabled):
                                mod.Toggle(ButtonState.Normal, false, false);
                                modsDisabledByCompatibilitySystem.Add(modType);

                                break;

                            case false when (IncompatibleMods?.Contains(modType) == true && modsDisabledByCompatibilitySystem.Contains(modType)):
                                mod.Toggle(ButtonState.Normal, false, false);
                                modsDisabledByCompatibilitySystem.Remove(modType);

                                break;
                        }
                    }

                if (Enabled)
                {
                    OnEnable();
                    Tools.Utils.OnUpdate      += Update;
                    Tools.Utils.OnLateUpdate  += LateUpdate;
                    Tools.Utils.OnFixedUpdate += FixedUpdate;
                    Tools.Utils.OnOnGUI       += OnGUI;
                }
                else
                {
                    OnDisable();
                    Tools.Utils.OnUpdate      -= Update;
                    Tools.Utils.OnLateUpdate  -= LateUpdate;
                    Tools.Utils.OnFixedUpdate -= FixedUpdate;
                    Tools.Utils.OnOnGUI       -= OnGUI;
                }

                if (playNotification && ModNotifications.Instance.Enabled)
                    NotificationManager.SendNotification(
                            Enabled
                                    ? "<color=green>Enabled</color>"
                                    : "<color=red>Disabled</color>",
                            Enabled
                                    ? $"{ModName}: {AssociatedAttribute.Description}"
                                    : $"{ModName}",
                            5f,
                            false,
                            false);

                break;
            }

            case ButtonType.Category:
            case ButtonType.Fixed:
            {
                Pressed();

                if (playNotification && ModNotifications.Instance.Enabled)
                    NotificationManager.SendNotification(
                            "<color=yellow>Pressed</color>",
                            $"{ModName}: {AssociatedAttribute.Description}",
                            5f,
                            false,
                            false);

                break;
            }

            case ButtonType.Incremental:
            {
                switch (buttonState)
                {
                    case ButtonState.Increment:
                        Increment();

                        if (playNotification && ModNotifications.Instance.Enabled)
                            NotificationManager.SendNotification(
                                    "<color=yellow>Incremented</color>",
                                    $"{ModName}: {AssociatedAttribute.Description}",
                                    5f,
                                    false,
                                    false);

                        break;

                    case ButtonState.Decrement:
                        Decrement();

                        if (playNotification && ModNotifications.Instance.Enabled)
                            NotificationManager.SendNotification(
                                    "<color=yellow>Decremented</color>",
                                    $"{ModName}: {AssociatedAttribute.Description}",
                                    5f,
                                    false,
                                    false);

                        break;

                    case ButtonState.Normal:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        ButtonHandler.Instance.UpdateButtons();
    }
}