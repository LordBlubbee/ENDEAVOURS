using System;
using TMPro;
using Unity.Networking.Transport;
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
    public TextMeshProUGUI Button1Tex;
    public TextMeshProUGUI Button2Tex;

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
    [NonSerialized] public UI_OrderMarker OrderMarker;

    public void SetModuleMarker(UI_OrderMarker Mark)
    {
        OrderMarker = Mark;
        OrderMarker.SetNumber(GetUseNumber());
    }

    int NumID = -1;
    private string GetUseNumber()
    {
        switch (NumID)
        {
            case < 10: return $"[{NumID + 1}] ";
            case 10: return "[0] ";
            default: return "";
        }
    }
    public void SetNumberID(int ID)
    {
        NumID = ID;
    }
    public void SetOff()
    {
        gameObject.SetActive(false);
        Mode = UIModuleModes.NONE;
        Crew = null;
        Module = null; ModuleWeapon = null;
        if (OrderMarker) OrderMarker.gameObject.SetActive(false);
    }
    public void SetModuleTarget(Module wep)
    {
        gameObject.SetActive(true);
        Module = wep;
        Mode = UIModuleModes.MODULE;

        IconBorder.sprite = CO_SPAWNER.co.DefaultInventorySprite;
        Icon.sprite = wep.IconSprite;
        Icon.color = new Color(1f - wep.GetHealthRelative(), wep.GetHealthRelative(), 0);
        Button1.SetActive(false);
        Button2.SetActive(false);

        switch (wep.ModuleType)
        {
            case Module.ModuleTypes.NAVIGATION:
                MainTex.text = $"{GetUseNumber()}NAVIGATION";
                StatusTex.text = $"INTEGRITY {(wep.GetHealth()).ToString("0")}";
                StatusTex.color = Icon.color;
                break;
            case Module.ModuleTypes.ENGINES:
                MainTex.text = $"{GetUseNumber()}ENGINES";
                Color EngineColor = new Color(1f - wep.GetHealthRelative(), wep.GetHealthRelative(), 0);
                int Dodge = Mathf.RoundToInt((10 + wep.ModuleLevel.Value * 5) * ((0.8f * wep.GetHealthRelative()) + 0.2f));
                if (wep.IsDisabled())
                {
                    EngineColor = Color.red;
                    Dodge = 0;
                }
                StatusTex.text = $"INTEGRITY {(wep.GetHealth()).ToString("0")} | <color=#{ColorUtility.ToHtmlStringRGB(EngineColor)}>DODGE CHANCE: {Dodge}%";
                StatusTex.color = Icon.color;
                //Button1.SetActive(true);
                //Button1Tex.text = "BOARD";
                //Button2.SetActive(true);
                //Button2Tex.text = "EVADE";
                break;
            case Module.ModuleTypes.ARMOR:
                MainTex.text = $"{GetUseNumber()}ARMOR CORE";
                float getArmorRelative = ((ModuleArmor)wep).GetArmor() / ((ModuleArmor)wep).MaxArmor;
                Color ArmorColor = new Color(1f - getArmorRelative, getArmorRelative, 0);
                StatusTex.text = $"INTEGRITY {(wep.GetHealth()).ToString("0")} | <color=#{ColorUtility.ToHtmlStringRGB(ArmorColor)}>ARMOR STATUS {((ModuleArmor)wep).GetArmor().ToString("0") }/{((ModuleArmor)wep).GetMaxArmor().ToString("0")}</color>";
                StatusTex.color = Icon.color;
                break;
            case Module.ModuleTypes.MEDICAL:
                MainTex.text = $"{GetUseNumber()}MEDICAL";
                StatusTex.text = $"INTEGRITY {(wep.GetHealth()).ToString("0")}";
                StatusTex.color = Icon.color;
                break;
        }
    }
    public void SetModuleTargetWeapon(ModuleWeapon wep)
    {
        gameObject.SetActive(true);
        ModuleWeapon = wep;
        Mode = UIModuleModes.MODULEWEAPON;

        IconBorder.sprite = CO_SPAWNER.co.DefaultInventorySprite;
        Icon.sprite = wep.IconSprite;
        Icon.color = new Color(1f - wep.GetHealthRelative(), wep.GetHealthRelative(), 0);
        Button1.SetActive(true);
        if (wep.AutofireActive.Value) Button1Tex.text = "<color=yellow>AUTO ON";
        else Button1Tex.text = "<color=#888888>AUTO OFF";
        Button2.SetActive(false);

        MainTex.text = $"{GetUseNumber()}{wep.WeaponName}";
        StatusTex.text = $"INTEGRITY {(wep.GetHealthRelative() * 100).ToString("0")}";
        
        if (wep.GetAmmoRatio() > 0) StatusTex.text += $" | <color=yellow>AMMO {wep.GetAmmo()}/{wep.MaxAmmo}</color>";
        else StatusTex.text += " | <color=red>NO AMMO</color>";

        if (wep.GetOrderPoint() == Vector3.zero) StatusTex.text += " | <color=red>NO TARGET</color>";
        else StatusTex.text += " | <color=yellow>TARGET SET</color>";

        if (wep.IsOnCooldown()) StatusTex.text += $" | <color=yellow>LOADING {(wep.CurCooldown.Value).ToString("0.0")}</color>";
        else StatusTex.text += " | <color=green>READY</color>";


        StatusTex.color = Icon.color;

        if (OrderMarker)
        {
            if (UI_CommandInterface.co.CurrentOrderMarker == OrderMarker)
            {
                CommandFollowCursor();
            }
            else if (wep.GetOrderPoint() != Vector3.zero && UI_CommandInterface.co.Tabs[1].activeSelf)
            {
                OrderMarker.Spr.sprite = wep.CrosshairSprite;
                OrderMarker.gameObject.SetActive(true);
                OrderMarker.transform.position = wep.GetOrderPoint();
                OrderMarker.transform.SetParent(CO.co.GetTransformAtPoint(wep.GetOrderPoint()));
            }
            else
            {
                DisconnectOrderMarker();
            }
        }
    }

    public void DisconnectOrderMarker()
    {
        OrderMarker.gameObject.SetActive(false);
        OrderMarker.transform.SetParent(null);
    }
    public void SetCrewTarget(CREW crew)
    {
        gameObject.SetActive(true);
        Crew = crew;
        Mode = UIModuleModes.CREW;

        IconBorder.sprite = CO_SPAWNER.co.DefaultInventorySprite;
        Icon.sprite = crew.Spr.sprite;
        Button1.SetActive(false);
        Button2.SetActive(false);

        MainTex.text = $"{GetUseNumber()}{crew.CharacterName.Value}";
        MainTex.color = new Color(crew.CharacterNameColor.Value.x, crew.CharacterNameColor.Value.y, crew.CharacterNameColor.Value.z);
        if (crew.isDeadForever()) StatusTex.text = $"<color=#880000>KIA";
        else if (crew.isDead()) StatusTex.text = $"<color=red>BLEEDING OUT: {crew.BleedingTime.Value.ToString("0")}";
        else StatusTex.text = $"HEALTH {(crew.GetHealthRelative() * 100).ToString("0")}";
        if (crew.IsPlayer()) StatusTex.text += " | <color=yellow>PLAYER</color>";
        else if (crew.GetOrderPoint() == Vector3.zero) StatusTex.text += " | <color=white>NO ORDERS</color>";
        else StatusTex.text += " | <color=yellow>ORDER SET</color>";
        StatusTex.color = new Color(1f - crew.GetHealthRelative(), crew.GetHealthRelative(), 0);


        if (OrderMarker)
        {
            if (UI_CommandInterface.co.CurrentOrderMarker == OrderMarker)
            {
                CommandFollowCursor();
            }
            else if (crew.GetOrderPoint() != Vector3.zero && UI_CommandInterface.co.Tabs[2].activeSelf)
            {
                OrderMarker.gameObject.SetActive(true);
                OrderMarker.transform.position = crew.GetOrderPoint();
                OrderMarker.transform.SetParent(CO.co.GetTransformAtPoint(crew.GetOrderPoint()));
            }
            else
            {
                DisconnectOrderMarker();
            }
        }
    }

    public void CommandFollowCursor()
    {
        OrderMarker.gameObject.SetActive(true);
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        OrderMarker.transform.position = new Vector3(mouse.x, mouse.y);
    }
    public void PressModule()
    {
        switch (Mode)
        {
            case UIModuleModes.CREW:
                UI_CommandInterface.co.SetSelection(Crew.transform);
                UI_CommandInterface.co.BeginOrdering(this);
                break;
            case UIModuleModes.MODULE:
                break;
            case UIModuleModes.MODULEWEAPON:
                UI_CommandInterface.co.SetSelection(ModuleWeapon.transform);
                UI_CommandInterface.co.BeginOrdering(this);
                break;
        }
    }
    public void PressButton1()
    {
        switch (Mode)
        {
            case UIModuleModes.CREW:
                break;
            case UIModuleModes.MODULE:
                switch (Module.ModuleType)
                {
                    case Module.ModuleTypes.ENGINES:
                        CO.co.BoardingManeuverRpc();
                        break;
                }
                break;
            case UIModuleModes.MODULEWEAPON:
                ModuleWeapon.SetAutofireRpc(!ModuleWeapon.AutofireActive.Value);
                break;
        }
    }
    public void PressButton2()
    {
        switch (Mode)
        {
            case UIModuleModes.CREW:
                break;
            case UIModuleModes.MODULE:
                switch (Module.ModuleType)
                {
                    case Module.ModuleTypes.ENGINES:
                        CO.co.EvasiveManeuverRpc();
                        break;
                }
                break;
            case UIModuleModes.MODULEWEAPON:
                break;
        }
    }
}
