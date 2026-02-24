using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod(                "Right Handed",       "Puts the menu in your right hand",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RightHanded : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()
    {
        IsEnabled = true;

        MenuHandler.Instance.ButtonPresser.transform.SetParent(Tools.Utils.RealLeftController, false);
        MenuHandler.Instance.ButtonPresser.transform.localPosition              = new Vector3(0f, -0.01f, 0.13f);
        MenuHandler.Instance.ButtonPresser.GetComponent<ButtonPresser>().isLeft = true;

        MenuHandler.Instance.Menu.transform.parent.SetParent(Tools.Utils.RealRightController, false);
        MenuHandler.Instance.Menu.transform.parent.transform.localRotation = Plugin.Instance.MenuLocalRotationRight;
        MenuHandler.Instance.Menu.transform.parent.localPosition           = Plugin.Instance.MenuLocalPositionRight;

        CoroutineManager.Instance.StartCoroutine(MenuHandler.Instance.CloseMenu());
    }

    protected override void OnDisable()
    {
        IsEnabled = false;

        MenuHandler.Instance.ButtonPresser.transform.SetParent(Tools.Utils.RealRightController, false);
        MenuHandler.Instance.ButtonPresser.transform.localPosition              = new Vector3(0f, -0.01f, 0.13f);
        MenuHandler.Instance.ButtonPresser.GetComponent<ButtonPresser>().isLeft = false;

        MenuHandler.Instance.Menu.transform.parent.SetParent(Tools.Utils.RealLeftController, false);
        MenuHandler.Instance.Menu.transform.parent.transform.localRotation = Plugin.Instance.MenuLocalRotationLeft;
        MenuHandler.Instance.Menu.transform.parent.localPosition           = Plugin.Instance.MenuLocalPositionLeft;

        CoroutineManager.Instance.StartCoroutine(MenuHandler.Instance.CloseMenu());
    }
}