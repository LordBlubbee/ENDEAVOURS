using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Module : MonoBehaviour
{
    public Image IconBorder;
    public Image Icon;
    public TextMeshProUGUI MainTex;
    public TextMeshProUGUI StatusTex;
    public GameObject Button1;
    public GameObject Button2;

    [NonSerialized] public UIModuleModes Mode;
    public enum UIModuleModes
    {
        NONE,
        CREW,
        MODULE,
        MODULEWEAPON
    }
    [NonSerialized] public CREW Crew;
    [NonSerialized] public Module Module;
    [NonSerialized] public ModuleWeapon ModuleWeapon;

    public void SetOff()
    {
        gameObject.SetActive(false);
        Mode = UIModuleModes.NONE;
        Crew = null;
        Module = null; ModuleWeapon = null;
    }
    public void SetModuleTarget(Module wep)
    {
        gameObject.SetActive(true);
        Module = wep;
        Mode = UIModuleModes.MODULE;

        IconBorder.sprite = null;
        Icon.sprite = wep.IconSprite;
        Icon.color = new Color(1f - wep.GetHealthRelative(), wep.GetHealthRelative(), 0);
        Button1.SetActive(false);
        Button2.SetActive(false);

        switch (wep.ModuleType)
        {
            case Module.ModuleTypes.NAVIGATION:
                MainTex.text = "NAVIGATION";
                StatusTex.text = $"INTEGRITY {(wep.GetHealthRelative()*100).ToString("0")}";
                StatusTex.color = Icon.color;
                break;
            case Module.ModuleTypes.ENGINES:
                MainTex.text = "ENGINES";
                StatusTex.text = $"INTEGRITY {(wep.GetHealthRelative() * 100).ToString("0")}";
                StatusTex.color = Icon.color;
                break;
            case Module.ModuleTypes.ARMOR:
                MainTex.text = "ARMOR CORE";
                StatusTex.text = $"INTEGRITY {(wep.GetHealthRelative() * 100).ToString("0")} | ARMOR STATUS {((ModuleArmor)wep).GetArmor().ToString("") }/{((ModuleArmor)wep).MaxArmor.ToString("")}";
                StatusTex.color = Icon.color;
                break;
            case Module.ModuleTypes.MEDICAL:
                MainTex.text = "MEDICAL";
                StatusTex.text = $"INTEGRITY {(wep.GetHealthRelative() * 100).ToString("0")}";
                StatusTex.color = Icon.color;
                break;
        }
    }
    public void SetModuleTargetWeapon(ModuleWeapon wep)
    {
        gameObject.SetActive(true);
        ModuleWeapon = wep;
        Mode = UIModuleModes.MODULEWEAPON;

        IconBorder.sprite = null;
        Icon.sprite = wep.IconSprite;
        Icon.color = new Color(1f - wep.GetHealthRelative(), wep.GetHealthRelative(), 0);

        MainTex.text = "WEAPON";
        StatusTex.text = $"INTEGRITY {(wep.GetHealthRelative() * 100).ToString("0")} | <color=red>NO TARGET</color>";
        StatusTex.color = Icon.color;
    }
    public void SetCrewTarget(CREW crew)
    {
        gameObject.SetActive(true);
        Crew = crew;
        Mode = UIModuleModes.CREW;

        IconBorder.sprite = CO_SPAWNER.co.DefaultInventorySprite;
        Icon.sprite = crew.Spr.sprite;

        MainTex.text = $"{crew.CharacterName}";
        MainTex.color = new Color(crew.CharacterNameColor.Value.x, crew.CharacterNameColor.Value.y, crew.CharacterNameColor.Value.z);
        if (crew.isDead()) StatusTex.text = $"KNOCKED OUT";
        else StatusTex.text = $"HEALTH {(crew.GetHealthRelative() * 100).ToString("0")}";
        if (crew.IsPlayer()) StatusTex.text += " | <color=yellow>PLAYER</color>";
        else StatusTex.text += " | <color=white>NO ORDERS</color>";
        StatusTex.color = new Color(1f - crew.GetHealthRelative(), crew.GetHealthRelative(), 0);
    }
    public void PressModule()
    {

    }
    public void PressButton1()
    {

    }
    public void PressButton2()
    {

    }
}
