using System;
using System.Linq;
using System.Reflection;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Managers;

public static class PatchManager
{
    private static Harmony instance;
    private static bool    IsPatched     { get; set; }
    private static int     FailedPatches { get; set; }

    private static bool IsLockedDown { get; set; }

    private static string LockdownReason { get; set; }

    public static bool PatchAll()
    {
        if (IsPatched)
            return !IsLockedDown;

        instance ??= new Harmony(Constants.PluginGuid);

        bool criticalPatchFailed = false;

        Type[] types;

        try
        {
            types = Assembly.GetExecutingAssembly().GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(type => type != null).ToArray();
        }

        foreach (Type type in types.Where(type =>
                                                  type is { IsClass: true, } &&
                                                  type.GetCustomAttributes<HarmonyPatch>().Any()))
        {
            try
            {
                instance.CreateClassProcessor(type).Patch();
            }
            catch (Exception ex)
            {
                FailedPatches++;

                bool isCritical =
                        type.GetCustomAttributes<CriticalPatchAttribute>().Any() ||
                        type.DeclaringType?.GetCustomAttributes<CriticalPatchAttribute>().Any() == true;

                Debug.LogError($"Failed to patch {type.FullName}: {ex}");

                if (isCritical)
                {
                    criticalPatchFailed = true;
                    EnableLockdown(type.Name);
                }
            }
        }

        IsPatched = true;

        return !criticalPatchFailed;
    }

    private static void EnableLockdown(string patchName)
    {
        if (IsLockedDown)
            return;

        IsLockedDown = true;

        LockdownReason =
                $"""
                 An important Harmony Patch has failed!
                 The hamburbur menu and Game's NetworkSystem have been disabled for your safety.

                 Failed Patch: {patchName}
                 """;

        Debug.LogError(LockdownReason);

        // GeneralFailureMessage calls NetworkSystem.Instance.SetWrongVersion() which prevents you from connecting to lobbies
        GorillaComputer.instance.GeneralFailureMessage(LockdownReason.ToUpper());
    }

    public static void UnpatchAll()
    {
        if (instance == null)
            return;

        instance.UnpatchSelf();

        instance  = null;
        IsPatched = false;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CriticalPatchAttribute : Attribute { }
}