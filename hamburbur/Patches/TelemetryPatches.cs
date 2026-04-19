using HarmonyLib;
using JetBrains.Annotations;
using Liv.Lck.Telemetry;

namespace hamburbur.Patches;

public static class TelemetryPatches
{
    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.EnqueueTelemetryEvent))]
    public class TelemetryPatch1
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.EnqueueZoneEvent))]
    public class TelemetryPatch2
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.FlushMothershipTelemetry))]
    public class TelemetryPatch3
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.FlushMothershipTelemetry))]
    public class TelemetryPatch4
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(LckTelemetryClient), nameof(LckTelemetryClient.SendTelemetry))]
    public class TelemetryPatch5
    {
        private static bool Prefix() =>
                false;
    }
}