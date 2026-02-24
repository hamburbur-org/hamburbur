using HarmonyLib;
using JetBrains.Annotations;
using Liv.Lck.Telemetry;
using PlayFab.EventsModels;

namespace hamburbur.Patches;

public static class TelemetryPatches
{
    [HarmonyPatch(typeof(GorillaTelemetry), "EnqueueTelemetryEvent")]
    public class TelemetryPatch1
    {
        private static bool Prefix(string eventName, object content, [CanBeNull] string[] customTags = null) =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), "EnqueueTelemetryEventPlayFab")]
    public class TelemetryPatch2
    {
        private static bool Prefix(EventContents eventContent) =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), "FlushPlayFabTelemetry")]
    public class TelemetryPatch3
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), "FlushMothershipTelemetry")]
    public class TelemetryPatch4
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(LckTelemetryClient), "SendTelemetry")]
    public class TelemetryPatch5
    {
        private static bool Prefix(LckTelemetryEvent lckTelemetryEvent) =>
                false;
    }
}