using System.Collections;
using System.Text;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Categories;

[hamburburmod(                nameof(Console), "Go to the console category", ButtonType.Category, AccessSetting.AdminOnly,
        EnabledType.Disabled, 0)]
public class Console : hamburburmod
{
    private            string animatedName;
    public override    string ModName   => animatedName ?? AssociatedAttribute.Name;
    protected override void   Pressed() => ButtonHandler.Instance.SetCategory(nameof(Console));

    protected override void Start() => CoroutineManager.Instance.StartCoroutine(AnimateTitle(AssociatedAttribute.Name));

    private IEnumerator AnimateTitle(string name)
    {
        Color32     colorA = new(129, 144, 253, 255);
        Color32     colorB = new(234, 84, 248, 255);
        const float Speed  = 4f;

        while (true)
        {
            if (string.IsNullOrEmpty(name) || !MenuHandler.Instance.MenuOpen || MenuHandler.Instance.Category != nameof(Main))
            {
                yield return null;

                continue;
            }

            StringBuilder sb  = new();
            int           len = name.Length;

            for (int i = 0; i < len; i++)
            {
                float  offset  = (float)i                                                           / len;
                float  t       = (Mathf.Sin(Time.time * Speed + (1f - offset) * Mathf.PI * 2) + 1f) * 0.5f;
                Color  blended = Color.Lerp(colorA, colorB, t);
                string hex     = ColorUtility.ToHtmlStringRGB(blended);
                sb.Append($"<color=#{hex}>{name[i]}</color>");
            }

            animatedName = sb.ToString();
            ButtonHandler.Instance.RefreshButtonText(this);

            yield return null;
        }
    }
}