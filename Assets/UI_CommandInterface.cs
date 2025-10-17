
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_CommandInterface : MonoBehaviour
{
    public UI_Module PrefabModule;
    public UI_OrderMarker PrefabOrderMarker;
    private UI_OrderMarker CurrentOrderMarker;
    List<UI_OrderMarker> OrderMarkers = new();
    public TextMeshProUGUI[] TabButtonTexts;
    public GameObject[] Tabs;
    [NonSerialized] public List<UI_Module> Modules = new();
    [NonSerialized] public List<UI_Module> Crews = new();
    [NonSerialized] public List<UI_Module> Weapons = new();
    [NonSerialized] public bool IsCommanding = false;
    int SelectedTab = -1;
    private void Start()
    {
        for (int i = 0; i < 12; i++)
        {
            UI_Module module = Instantiate(PrefabModule, Tabs[0].transform);
            Modules.Add(module);
            module.SetOff();
            module = Instantiate(PrefabModule, Tabs[1].transform);
            Weapons.Add(module);
            module.SetOff();
            module = Instantiate(PrefabModule, Tabs[2].transform);
            Crews.Add(module);
            module.SetOff();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateCrew());
    }
    IEnumerator UpdateCrew()
    {
        while (true)
        {
            if (SelectedTab > -1) RefreshCommandInterface();
            yield return new WaitForSeconds(0.25f);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PressTabButton(0);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            PressTabButton(1);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            PressTabButton(2);
        }
        switch ( SelectedTab)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.Alpha1)) Modules[0].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha2)) Modules[1].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha3)) Modules[2].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha4)) Modules[3].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha5)) Modules[4].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha6)) Modules[5].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha7)) Modules[6].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha8)) Modules[7].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha9)) Modules[8].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha0)) Modules[9].PressModule();
                break;
            case 1:
                if (Input.GetKeyDown(KeyCode.Alpha1)) Weapons[0].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha2)) Weapons[1].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha3)) Weapons[2].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha4)) Weapons[3].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha5)) Weapons[4].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha6)) Weapons[5].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha7)) Weapons[6].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha8)) Weapons[7].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha9)) Weapons[8].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha0)) Weapons[9].PressModule();
                break;
            case 2:
                if (Input.GetKeyDown(KeyCode.Alpha1)) Crews[0].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha2)) Crews[1].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha3)) Crews[2].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha4)) Crews[3].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha5)) Crews[4].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha6)) Crews[5].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha7)) Crews[6].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha8)) Crews[7].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha9)) Crews[8].PressModule();
                if (Input.GetKeyDown(KeyCode.Alpha0)) Crews[9].PressModule();
                break;
        }
    }
    public void PressTabButton(int ID)
    {
        for (int i = 0; i < TabButtonTexts.Length; i++)
        {
            if (i == ID && SelectedTab != ID)
            {
                TabButtonTexts[i].color = Color.cyan;
                Tabs[i].SetActive(true);
            }
            else
            {
                TabButtonTexts[i].color = Color.gray;
                Tabs[i].SetActive(false);
            }
        }
        if (SelectedTab == ID)
        {
            SelectedTab = -1;
            IsCommanding = false;
            foreach (UI_OrderMarker spr in OrderMarkers)
            {
                spr.gameObject.SetActive(false);
            }
            LOCALCO.local.SetCameraToPlayer();
        }
        else
        {
            SelectedTab = ID;
            IsCommanding = true;
            foreach (UI_OrderMarker spr in OrderMarkers)
            {
                spr.gameObject.SetActive(true);
            }
            RefreshCommandInterface();
            CAM.cam.SetCameraMode(CO.co.PlayerMainDrifter.transform, 150f, 30f, 150f);
        }
    }
    public void RefreshCommandInterface()
    {
        CrewReferences = new();
        ModuleReferences = new();
        WeaponReferences = new();

        foreach (Module mod in CO.co.GetPlayerSpace().WeaponModules)
        {
            WeaponReferences.Add((ModuleWeapon)mod);
        }
        foreach (Module mod in CO.co.GetPlayerSpace().CoreModules)
        {
            if (mod.ModuleType == Module.ModuleTypes.INVENTORY) continue;
            if (mod.ModuleType == Module.ModuleTypes.MAPCOMMS) continue;
            ModuleReferences.Add(mod);
        }
        foreach (Module mod in CO.co.GetPlayerSpace().SystemModules)
        {
            ModuleReferences.Add(mod);
        }
        foreach (AI_UNIT crew in CO.co.PlayerMainDrifter.CrewGroup.GetUnits())
        {
            CrewReferences.Add(crew.Unit);
        }
        for (int i = 0; i < 12; i++)
        {
            if (WeaponReferences.Count >= i) Weapons[i].SetModuleTargetWeapon(WeaponReferences[i]);
            else Weapons[i].SetOff();
            if (ModuleReferences.Count >= i) Modules[i].SetModuleTarget(ModuleReferences[i]);
            else Modules[i].SetOff();
            if (CrewReferences.Count >= i) Crews[i].SetCrewTarget(CrewReferences[i]);
            else Crews[i].SetOff();
        }
    }
    private void StopAiming()
    {
        Destroy(CurrentOrderMarker.gameObject);
        CurrentOrderMarker = null;
    }
    private void PressAim()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool Hit = false;
        foreach (Collider2D col in Physics2D.OverlapCircleAll(mouse,0.1f))
        {
            //
        }
        if (!Hit) return;
    }
    private void BeginAim()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CurrentOrderMarker = Instantiate(PrefabOrderMarker, mouse, Quaternion.identity);
    }

    private List<CREW> CrewReferences;
    private List<Module> ModuleReferences;
    private List<ModuleWeapon> WeaponReferences;
    public void AimModule(UI_Module UI_Mod)
    {
        switch (UI_Mod.Mode)
        {
            case UI_Module.UIModuleModes.CREW:
                break;
            case UI_Module.UIModuleModes.MODULEWEAPON:
                break;
        }
    }
}
