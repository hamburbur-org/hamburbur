using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Menu Animation: ",   "Changes how the menu opens and closes.", ButtonType.Incremental,
        AccessSetting.Public, EnabledType.Disabled, 1)]
public class MenuOpenCloseAnimation : hamburburmod
{
    private static readonly (int Index, string Name)[] Animations =
    [
            (5, "None"),
            (0, "Scale"),
            (1, "Pop"),
            (6, "Book"),
            (7, "Expand"),
            (8, "Squeeze"),
            (9, "Stretch"),
    ];

    public static MenuOpenCloseAnimation Instance { get; private set; }

    public static int CurrentIndex => NormalizeIndex(Instance?.IncrementalValue ?? 0);

    public override string ModName => AssociatedAttribute.Name + GetAnimationName(CurrentIndex);

    protected override void Start() => Instance = this;

    protected override void Increment()
    {
        int current = GetAnimationPosition(CurrentIndex);
        IncrementalValue = Animations[(current + 1) % Animations.Length].Index;
    }

    protected override void Decrement()
    {
        int current = GetAnimationPosition(CurrentIndex);
        IncrementalValue = Animations[(current - 1 + Animations.Length) % Animations.Length].Index;
    }

    protected override void OnIncrementalStateLoaded() => IncrementalValue = NormalizeIndex(IncrementalValue);

    private static int NormalizeIndex(int index) => GetAnimationPosition(index) >= 0 ? index : 0;

    private static string GetAnimationName(int index)
    {
        int position = GetAnimationPosition(index);

        return position >= 0 ? Animations[position].Name : Animations[0].Name;
    }

    private static int GetAnimationPosition(int index)
    {
        for (int i = 0; i < Animations.Length; i++)
            if (Animations[i].Index == index)
                return i;

        return -1;
    }
}