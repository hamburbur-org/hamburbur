using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Ban Duration: ", "Change how long people get banned for with ban hammer", ButtonType.Incremental,
        AccessSetting.AdminOnly,
        EnabledType.Disabled, 0)]
public class BanDuration : hamburburmod
{
    private const int MinIndex = 0;
    private const int MaxIndex = 10;

    private static readonly long[] Steps =
    [
            100L    * 1000L,
            500L    * 1000L,
            1000L   * 1000L,
            2000L   * 1000L,
            5000L   * 1000L,
            10000L  * 1000L,
            15000L  * 1000L,
            20000L  * 1000L,
            25000L  * 1000L,
            100000L * 1000L,
            500000L * 1000L,
    ];

    private static BanDuration Instance { get; set; }

    public override string ModName =>
            AssociatedAttribute.Name + FormatTime(Steps[IncrementalValue]);

    public static long CurrentValueLong =>
            Steps[Instance.IncrementalValue];

    public static float CurrentValueFloat =>
            Steps[Instance.IncrementalValue];

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue > MaxIndex)
            IncrementalValue = MinIndex;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < MinIndex)
            IncrementalValue = MaxIndex;
    }

    private static string FormatTime(long ms)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(ms);

        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours}h {time.Minutes}m {time.Seconds}s";

        if (time.TotalMinutes >= 1)
            return $"{time.Minutes}m {time.Seconds}s";

        return time.TotalSeconds >= 1 ? $"{time.Seconds}s" : $"{time.Milliseconds}ms";
    }
}