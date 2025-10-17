
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_CommandInterface : MonoBehaviour
{
    public static UI_CommandInterface co;
    public UI_Module PrefabModule;
    public UI_OrderMarker PrefabOrderMarker;
    [NonSerialized] public UI_OrderMarker CurrentOrderMarker;
    private UI_Module CurrentModuleSelected;
    List<UI_OrderMarker> OrderMarkersCrew = new();
    List<UI_OrderMarker> OrderMarkersWeapons = new();
    public TextMeshProUGUI[] TabButtonTexts;
    public GameObject[] Tabs;
    [NonSerialized] public List<UI_Module> Modules = new();
    [NonSerialized] public List<UI_Module> Crews = new();
    [NonSerialized] public List<UI_Module> Weapons = new();
    [NonSerialized] public bool IsCommanding = false;
    int SelectedTab = -1;
    private void Start()
    {
        co = this;
        for (int i = 0; i < 12; i++)
        {
            UI_Module module = Instantiate(PrefabModule, Tabs[0].transform);
            Modules.Add(module);
            module.SetNumberID(i);
            module.SetOff();
           

            //Weapons
            module = Instantiate(PrefabModule, Tabs[1].transform);
            Weapons.Add(module);
            module.SetNumberID(i);
            module.SetOff();
            UI_OrderMarker mark = Instantiate(PrefabOrderMarker, Vector3.zero, Quaternion.identity);
            OrderMarkersWeapons.Add(mark);
            module.SetModuleMarker(mark);
            //Crew
            module = Instantiate(PrefabModule, Tabs[2].transform);
            Crews.Add(module);
            module.SetNumberID(i);
            module.SetOff();
            mark = Instantiate(PrefabOrderMarker, Vector3.zero, Quaternion.identity);
            OrderMarkersCrew.Add(mark);
            module.SetModuleMarker(mark);
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
        if (IsOrdering())
        {
            CurrentModuleSelected.CommandFollowCursor();
            if (Input.GetMouseButtonDown(0)) SelectOrderTarget();
            else if (Input.GetMouseButtonDown(1)) StopOrdering();
        }
    }
    public void PressTabButton(int ID)
    {
        if (IsOrdering()) StopOrdering();
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
            RefreshCommandInterface();
            LOCALCO.local.SetCameraToPlayer();
        }
        else
        {
            SelectedTab = ID;
            IsCommanding = true;
            RefreshCommandInterface();
            LOCALCO.local.SetCameraToCommand();
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
        foreach (CREW crew in CO.co.GetAlliedCrew())
        {
            CrewReferences.Add(crew);
        }
        for (int i = 0; i < 12; i++)
        {
            if (WeaponReferences.Count > i) Weapons[i].SetModuleTargetWeapon(WeaponReferences[i]);
            else Weapons[i].SetOff();
            if (ModuleReferences.Count > i) Modules[i].SetModuleTarget(ModuleReferences[i]);
            else Modules[i].SetOff();
            if (CrewReferences.Count > i) Crews[i].SetCrewTarget(CrewReferences[i]);
            else Crews[i].SetOff();
        }
    }
    private void StopOrdering()
    {
        switch (CurrentModuleSelected.Mode)
        {
            case UI_Module.UIModuleModes.MODULE:
                CurrentModuleSelected.Module.SetOrderPointRpc(Vector3.zero);
                break;
            case UI_Module.UIModuleModes.MODULEWEAPON:
                CurrentModuleSelected.ModuleWeapon.SetOrderPointRpc(Vector3.zero);
                break;
            case UI_Module.UIModuleModes.CREW:
                CurrentModuleSelected.Crew.SetOrderPointRpc(Vector3.zero);
                break;
        }
        CurrentOrderMarker.DeselectOrderMarker();
        CurrentOrderMarker = null;
        CurrentOrderMarker.gameObject.SetActive(false);
        RefreshCommandInterface();
        
    }
    private void SelectOrderTarget()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse = new Vector3(mouse.x, mouse.y);
        bool Hit = false;
        foreach (Collider2D col in Physics2D.OverlapCircleAll(mouse,0.1f))
        {
            //
            if (col.GetComponent<SPACE>() != null)
            {
                Hit = true;
                break;
            }
            if (CurrentModuleSelected.Mode == UI_Module.UIModuleModes.MODULEWEAPON)
            {
                if (col.GetComponent<CREW>() != null)
                {
                    Hit = true;
                    break;
                }
            }
        }

        if (!Hit) return;
        switch (CurrentModuleSelected.Mode)
        {
            case UI_Module.UIModuleModes.MODULE:
                CurrentModuleSelected.Module.SetOrderPointRpc(mouse);
                break;
            case UI_Module.UIModuleModes.MODULEWEAPON:
                CurrentModuleSelected.ModuleWeapon.SetOrderPointRpc(mouse);
                break;
            case UI_Module.UIModuleModes.CREW:
                CurrentModuleSelected.Crew.SetOrderPointRpc(mouse);
                break;
        }
        CurrentOrderMarker.DeselectOrderMarker();
        CurrentOrderMarker = null;
        RefreshCommandInterface();
    }

    private bool IsOrdering()
    {
        return CurrentOrderMarker != null;
    }
    public void BeginOrdering(UI_Module mod)
    {
        CurrentOrderMarker = mod.OrderMarker;
        CurrentOrderMarker.SelectOrderMarker();
        CurrentModuleSelected = mod;
        RefreshCommandInterface();
    }

    private List<CREW> CrewReferences;
    private List<Module> ModuleReferences;
    private List<ModuleWeapon> WeaponReferences;
}
