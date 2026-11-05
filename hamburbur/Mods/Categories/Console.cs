using System.Collections;
using System.Text;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using TMPro;
using UnityEngine;

namespace hamburbur.Mods.Categories;

[hamburburmod(                nameof(Console), "Go to the console category", ButtonType.Category, AccessSetting.AdminOnly,
        EnabledType.Disabled, 0)]
public class Console : hamburburmod
{
    private const int AnimationFrames = 48;
    private string animatedName;

    public override string ModName => animatedName ?? AssociatedAttribute.Name;

    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(Console));

    protected override void Start() => CoroutineManager.Instance.StartCoroutine(AnimateTitle(AssociatedAttribute.Name));

    private IEnumerator AnimateTitle(string name)
    {
        if (string.IsNullOrEmpty(name))
            yield break;

        Color32 colorA = new(129, 144, 253, 255);
        Color32 colorB = new(234, 84, 248, 255);
        const float Speed = 4f;

        string[] frames = CreateAnimationFrames(name, colorA, colorB);
        TMP_Text currentText = null;
        int currentRevision = -1;
        int currentFrame = -1;

        while (true)
        {
            ButtonHandler buttonHandler = ButtonHandler.Instance;
            TMP_Text text = MenuHandler.Instance?.MenuOpen == true
                                    ? buttonHandler?.GetVisibleMainButtonText(this)
                                    : null;

            if (!text)
            {
                currentText = null;
                currentFrame = -1;
                yield return null;

                continue;
            }

            int frame = Mathf.Min(AnimationFrames - 1,
                    Mathf.FloorToInt(Mathf.Repeat(Time.time * Speed, Mathf.PI * 2f) /
                                     (Mathf.PI * 2f) * AnimationFrames));

            if (text != currentText || currentRevision != ButtonHandler.UpdateRevision || frame != currentFrame)
            {
                animatedName = frames[frame];
                text.text = animatedName;
                currentText     = text;
                currentRevision = ButtonHandler.UpdateRevision;
                currentFrame    = frame;
            }

            yield return null;
        }
    }

    private static string[] CreateAnimationFrames(string name, Color32 colorA, Color32 colorB)
    {
        string[] frames = new string[AnimationFrames];
        StringBuilder builder = new(name.Length * 24);
        for (int frame = 0; frame < frames.Length; frame++)
        {
            float phase = (float)frame / frames.Length * Mathf.PI * 2f;
            builder.Clear();

            for (int i = 0; i < name.Length; i++)
            {
                float offset = (float)i / name.Length;
                float t = (Mathf.Sin(phase + (1f - offset) * Mathf.PI * 2f) + 1f) * 0.5f;
                string hex = ColorUtility.ToHtmlStringRGB(Color.Lerp(colorA, colorB, t));
                builder.Append("<color=#").Append(hex).Append('>')
                       .Append(name[i]).Append("</color>");
            }

            frames[frame] = builder.ToString();
        }

        return frames;
    }
}
