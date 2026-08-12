using System;
using System.Collections.Generic;
using hamburbur.Mod_Backend;
using UnityEngine;
using ArrayListSetting = hamburbur.Mods.Settings.ArrayList;

namespace hamburbur.GUI;

public class ArrayListOverlay : MonoBehaviour
{
    private const float EntryAnimationSpeed      = 0.14f;
    private const float PositionAnimationSpeed   = 0.16f;
    private const float VisibilityAnimationSpeed = 0.15f;

    private const float BaseRowHeight = 26f;
    private const float BaseRowGap    = 3f;
    private const float BaseColumnGap = 8f;

    private const float TopMargin    = 22f;
    private const float RightMargin  = 18f;
    private const float BottomMargin = 18f;

    private const    int             MaxColumns   = 3;
    private readonly HashSet<string> enabledNames = new();
    private readonly float[]         columnRightEdges = new float[MaxColumns];
    private readonly float[]         columnWidths     = new float[MaxColumns];

    private readonly Dictionary<string, ArrayListEntry> entries      = new();
    private readonly List<string>                       removalCache = new();

    private readonly List<string> sortedNames = new();
    private          GUIContent   countContent;
    private          GUIStyle     countStyle;
    private          GUIStyle     modStyle;

    private GUIContent titleContent;
    private GUIStyle titleStyle;

    private int cachedButtonRevision = -1;
    private int cachedEnabledRevision = -1;
    private bool cachedEnabledState;
    private bool entriesDirty = true;
    private bool headerMeasurementsDirty = true;
    private int lastScreenHeight;
    private int lastScreenWidth;

    private float         badgeWidth;
    private LayoutMetrics cachedLayout;
    private float         headerWidth;

    private float visibility;
    private float visibilityVelocity;

    private Texture2D whiteTexture;

    public static ArrayListOverlay Instance { get; private set; }

    public static void Show()
    {
        if (Instance == null)
            return;

        Instance.enabled      = true;
        Instance.entriesDirty = true;
        Instance.visibilityVelocity = 0f;
    }

    private void Awake()
    {
        Instance = this;

        whiteTexture = new Texture2D(1, 1)
        {
                name       = "HamburburArrayListPixel",
                wrapMode   = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
        };

        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();

        titleContent = new GUIContent("hamburbur");
        countContent = new GUIContent("0 ACTIVE");

        titleStyle = new GUIStyle
        {
                fontSize  = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                clipping  = TextClipping.Clip,
        };

        countStyle = new GUIStyle
        {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping  = TextClipping.Clip,
        };

        modStyle = new GUIStyle
        {
                fontSize  = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                clipping  = TextClipping.Clip,
        };

        enabled = ArrayListSetting.IsEnabled;
    }

    private void Update()
    {
        bool arrayListEnabled = ArrayListSetting.IsEnabled;

        visibility = Mathf.SmoothDamp(
                visibility,
                arrayListEnabled ? 1f : 0f,
                ref visibilityVelocity,
                VisibilityAnimationSpeed,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

        bool needsEntryRefresh = arrayListEnabled != cachedEnabledState ||
                                 hamburburmod.EnabledStateRevision != cachedEnabledRevision ||
                                 ButtonHandler.UpdateRevision != cachedButtonRevision;

        if (needsEntryRefresh)
        {
            UpdateEntries(arrayListEnabled);
            cachedEnabledState   = arrayListEnabled;
            cachedEnabledRevision = hamburburmod.EnabledStateRevision;
            cachedButtonRevision = ButtonHandler.UpdateRevision;
        }

        if (!arrayListEnabled && visibility < 0.005f)
        {
            entries.Clear();
            sortedNames.Clear();
            enabledNames.Clear();
            entriesDirty = true;
            visibilityVelocity = 0f;
            this.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (whiteTexture != null)
            Destroy(whiteTexture);
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        if (visibility < 0.005f)
            return;

        DrawArrayList();
    }

    private void UpdateEntries(bool arrayListEnabled)
    {
        bool membershipChanged = false;
        enabledNames.Clear();

        if (arrayListEnabled)
        {
            foreach (KeyValuePair<string, ValueTuple<Type, hamburburmod>[]> category in Buttons.Categories)
            {
                foreach ((Type _, hamburburmod mod) in category.Value)
                {
                    if (mod == null ||
                        !mod.Enabled ||
                        mod.AssociatedAttribute.ButtonType != ButtonType.Togglable)
                        continue;

                    string name = mod.ModName;
                    enabledNames.Add(name);

                    if (entries.TryGetValue(name, out ArrayListEntry existingEntry))
                    {
                        if (!existingEntry.Active)
                            membershipChanged = true;

                        existingEntry.Active = true;

                        continue;
                    }

                    entries.Add(name, new ArrayListEntry
                    {
                            Active  = true,
                            Content = new GUIContent(name),
                    });
                    membershipChanged = true;
                }
            }
        }

        foreach (KeyValuePair<string, ArrayListEntry> pair in entries)
            if (pair.Value.Active && !enabledNames.Contains(pair.Key))
            {
                pair.Value.Active = false;
                membershipChanged = true;
            }

        if (membershipChanged)
            entriesDirty = true;

        countContent.text = $"{enabledNames.Count} ACTIVE";
        headerMeasurementsDirty = true;
    }

    private void DrawArrayList()
    {
        if (headerMeasurementsDirty)
        {
            UpdateHeaderMeasurements();
            headerMeasurementsDirty = false;
        }

        bool resolutionChanged = lastScreenWidth  != Screen.width ||
                                 lastScreenHeight != Screen.height;
        if (entriesDirty || resolutionChanged)
        {
            SortEntries();
            cachedLayout = CalculateLayout();
            UpdateEntryTargets(cachedLayout);
            lastScreenWidth  = Screen.width;
            lastScreenHeight = Screen.height;
            entriesDirty     = false;
        }

        LayoutMetrics layout = cachedLayout;

        Color mainColor      = Plugin.Instance.MainColour;
        Color secondaryColor = Plugin.Instance.SecondaryColour;

        modStyle.normal.textColor = WithAlpha(
                Color.Lerp(mainColor, Color.white, 0.92f),
                visibility);

        float globalOffset = (1f - visibility) * 35f;

        DrawHeader(
                mainColor,
                secondaryColor,
                globalOffset);

        removalCache.Clear();

        foreach (KeyValuePair<string, ArrayListEntry> pair in entries)
        {
            string         name  = pair.Key;
            ArrayListEntry entry = pair.Value;

            if (!entry.Active)
                entry.TargetX = Screen.width + 50f;

            if (!entry.Initialized)
            {
                entry.X = Screen.width + 50f;
                entry.Y = entry.TargetY;

                entry.Initialized = true;
            }

            entry.Update(
                    EntryAnimationSpeed,
                    PositionAnimationSpeed);

            if (!entry.Active &&
                entry.X > Screen.width + 20f)
            {
                removalCache.Add(name);

                continue;
            }

            DrawEntry(
                    entry,
                    layout,
                    mainColor,
                    secondaryColor,
                    globalOffset);
        }

        foreach (string name in removalCache)
            entries.Remove(name);
    }

    private void DrawHeader(
            Color mainColor,
            Color secondaryColor,
            float globalOffset)
    {
        const float HeaderHeight = 48f;

        titleStyle.normal.textColor = WithAlpha(
                Color.Lerp(mainColor, Color.white, 0.8f),
                visibility);

        countStyle.normal.textColor = WithAlpha(
                Color.Lerp(mainColor, Color.white, 0.9f),
                visibility);

        float right = Screen.width - RightMargin + globalOffset;

        Rect headerRect = new(
                right - headerWidth,
                TopMargin,
                headerWidth,
                HeaderHeight);

        Color backgroundColor = Color.Lerp(
                secondaryColor,
                Color.black,
                0.2f);

        backgroundColor.a = 0.94f * visibility;

        DrawRoundedRect(
                headerRect,
                backgroundColor,
                9f);

        Rect accentRect = new(
                headerRect.x,
                headerRect.y,
                5f,
                headerRect.height);

        DrawRoundedRect(
                accentRect,
                WithAlpha(mainColor, visibility),
                9f);

        Rect countRect = new(
                headerRect.xMax - badgeWidth - 12f,
                headerRect.y    + 14f,
                badgeWidth,
                20f);

        Color countBackground = mainColor;
        countBackground.a = 0.22f * visibility;

        DrawRoundedRect(
                countRect,
                countBackground,
                5f);

        UnityEngine.GUI.Label(
                countRect,
                countContent,
                countStyle);

        Rect titleRect = new(
                headerRect.x      + 15f,
                headerRect.y      + 3f,
                countRect.x       - headerRect.x - 24f,
                headerRect.height - 6f);

        UnityEngine.GUI.Label(
                titleRect,
                titleContent,
                titleStyle);
    }

    private LayoutMetrics CalculateLayout()
    {
        const float HeaderHeight = 48f;

        int count = sortedNames.Count;

        float startY =
                TopMargin    +
                HeaderHeight +
                8f;

        float availableHeight =
                Screen.height -
                startY        -
                BottomMargin;

        if (count == 0)
            return new LayoutMetrics
            {
                    Scale         = 1f,
                    Columns       = 1,
                    RowsPerColumn = 1,
                    RowHeight     = BaseRowHeight,
                    RowGap        = BaseRowGap,
                    ColumnGap     = BaseColumnGap,
                    StartY        = startY,
            };

        int columns       = 1;
        int rowsPerColumn = count;

        float scale = 1f;

        for (int currentColumns = 1;
             currentColumns <= MaxColumns;
             currentColumns++)
        {
            int rows = Mathf.CeilToInt(
                    count / (float)currentColumns);

            float requiredHeight =
                    rows                   * BaseRowHeight +
                    Mathf.Max(0, rows - 1) * BaseRowGap;

            float candidateScale = Mathf.Min(
                    1f,
                    availableHeight / requiredHeight);

            columns       = currentColumns;
            rowsPerColumn = rows;
            scale         = candidateScale;

            if (candidateScale >= 0.78f)
                break;
        }

        scale = Mathf.Clamp(
                scale,
                0.62f,
                1f);

        modStyle.fontSize = Mathf.RoundToInt(
                Mathf.Lerp(11f, 15f, scale));

        return new LayoutMetrics
        {
                Scale         = scale,
                Columns       = columns,
                RowsPerColumn = rowsPerColumn,
                RowHeight     = BaseRowHeight * scale,
                RowGap        = BaseRowGap    * scale,
                ColumnGap     = BaseColumnGap * scale,
                StartY        = startY,
        };
    }

    private void UpdateEntryTargets(LayoutMetrics layout)
    {
        if (sortedNames.Count == 0)
            return;

        Array.Clear(columnWidths, 0, columnWidths.Length);
        Array.Clear(columnRightEdges, 0, columnRightEdges.Length);

        for (int i = 0; i < sortedNames.Count; i++)
        {
            string name = sortedNames[i];

            if (!entries.TryGetValue(name, out ArrayListEntry entry))
                continue;

            int column = i / layout.RowsPerColumn;

            if (column >= layout.Columns)
                column = layout.Columns - 1;

            entry.SortIndex = i;
            entry.TextWidth = modStyle.CalcSize(entry.Content).x;

            float width = Mathf.Max(
                    100f * layout.Scale,
                    entry.TextWidth + 34f * layout.Scale);

            entry.Width = width;

            columnWidths[column] = Mathf.Max(
                    columnWidths[column],
                    width);
        }

        columnRightEdges[0] =
                Screen.width -
                RightMargin  -
                7f;

        for (int i = 1; i < layout.Columns; i++)
            columnRightEdges[i] =
                    columnRightEdges[i - 1] -
                    columnWidths[i     - 1] -
                    layout.ColumnGap;

        for (int i = 0; i < sortedNames.Count; i++)
        {
            string name = sortedNames[i];

            if (!entries.TryGetValue(name, out ArrayListEntry entry))
                continue;

            int column = i / layout.RowsPerColumn;
            int row    = i % layout.RowsPerColumn;

            if (column >= layout.Columns)
                column = layout.Columns - 1;

            entry.Height = layout.RowHeight;

            entry.TargetX =
                    columnRightEdges[column] -
                    entry.Width;

            entry.TargetY =
                    layout.StartY +
                    row * (layout.RowHeight + layout.RowGap);
        }
    }

    private void DrawEntry(
            ArrayListEntry entry,
            LayoutMetrics  layout,
            Color          mainColor,
            Color          secondaryColor,
            float          globalOffset)
    {
        Rect rowRect = new(
                entry.X + globalOffset,
                entry.Y,
                entry.Width,
                entry.Height);

        Color backgroundColor = Color.Lerp(
                secondaryColor,
                Color.black,
                0.32f);

        backgroundColor.a = 0.87f * visibility;

        DrawRoundedRect(
                rowRect,
                backgroundColor,
                6f * layout.Scale);

        float pulse = Mathf.PingPong(
                Time.time         * 0.7f +
                entry.SortIndex   * 0.12f,
                1f);

        Color accentColor = Color.Lerp(
                secondaryColor,
                mainColor,
                0.45f + pulse * 0.55f);

        accentColor.a *= visibility;

        float markerWidth = Mathf.Max(
                2f,
                3f * layout.Scale);

        float markerPadding = Mathf.Max(
                4f,
                6f * layout.Scale);

        Rect markerRect = new(
                rowRect.xMax - 8f * layout.Scale,
                rowRect.y    + markerPadding,
                markerWidth,
                rowRect.height - markerPadding * 2f);

        DrawRoundedRect(
                markerRect,
                accentColor,
                2f);

        Rect textRect = new(
                rowRect.x + 11f * layout.Scale,
                rowRect.y,
                rowRect.width - 25f * layout.Scale,
                rowRect.height);

        UnityEngine.GUI.Label(
                textRect,
                entry.Content,
                modStyle);
    }

    private void UpdateHeaderMeasurements()
    {
        badgeWidth = countStyle.CalcSize(countContent).x + 16f;
        headerWidth = Mathf.Max(
                230f,
                titleStyle.CalcSize(titleContent).x + badgeWidth + 48f);
    }

    private void SortEntries()
    {
        sortedNames.Clear();

        foreach (KeyValuePair<string, ArrayListEntry> pair in entries)
            if (pair.Value.Active)
                sortedNames.Add(pair.Key);

        sortedNames.Sort((left, right) =>
                                 string.Compare(
                                         left,
                                         right,
                                         StringComparison.OrdinalIgnoreCase));
    }

    private void DrawRoundedRect(
            Rect  rect,
            Color color,
            float radius)
    {
        UnityEngine.GUI.DrawTexture(
                rect,
                whiteTexture,
                ScaleMode.StretchToFill,
                true,
                0f,
                color,
                0f,
                radius);
    }

    private static Color WithAlpha(
            Color color,
            float alpha)
    {
        color.a *= alpha;

        return color;
    }

    private class ArrayListEntry
    {
        public bool  Active;
        public GUIContent Content;
        public float Height;
        public bool  Initialized;
        public int   SortIndex;
        public float TextWidth;

        public float TargetX;
        public float TargetY;

        public float Width;

        public float X;

        private float xVelocity;
        public  float Y;
        private float yVelocity;

        public void Update(
                float horizontalSpeed,
                float verticalSpeed)
        {
            X = Mathf.SmoothDamp(
                    X,
                    TargetX,
                    ref xVelocity,
                    horizontalSpeed,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

            Y = Mathf.SmoothDamp(
                    Y,
                    TargetY,
                    ref yVelocity,
                    verticalSpeed,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);
        }
    }

    private struct LayoutMetrics
    {
        public float Scale;

        public int Columns;
        public int RowsPerColumn;

        public float RowHeight;
        public float RowGap;
        public float ColumnGap;

        public float StartY;
    }
}
