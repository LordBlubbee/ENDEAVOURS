
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

    public bool IsCommandingTabOpen()
    {
        return SelectedTab > -1;
    }
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
            yield return new WaitForSeconds(0.1f);
        }
    }
    UI_Module lastSelectedMod = null;

    private void DeselectModule()
    {
        if (lastSelectedMod)
        {
            lastSelectedMod = null;
        }
    }
    private void PressModule(UI_Module mod)
    {
        DeselectModule();
        lastSelectedMod = mod;
        mod.PressModule();
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
        switch (SelectedTab)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.Alpha1)) PressModule(Modules[0]);
                if (Input.GetKeyDown(KeyCode.Alpha2)) PressModule(Modules[1]);
                if (Input.GetKeyDown(KeyCode.Alpha3)) PressModule(Modules[2]);
                if (Input.GetKeyDown(KeyCode.Alpha4)) PressModule(Modules[3]);
                if (Input.GetKeyDown(KeyCode.Alpha5)) PressModule(Modules[4]);
                if (Input.GetKeyDown(KeyCode.Alpha6)) PressModule(Modules[5]);
                if (Input.GetKeyDown(KeyCode.Alpha7)) PressModule(Modules[6]);
                if (Input.GetKeyDown(KeyCode.Alpha8)) PressModule(Modules[7]);
                if (Input.GetKeyDown(KeyCode.Alpha9)) PressModule(Modules[8]);
                if (Input.GetKeyDown(KeyCode.Alpha0)) PressModule(Modules[9]);
                break;

            case 1:
                if (Input.GetKeyDown(KeyCode.Alpha1)) PressModule(Weapons[0]);
                if (Input.GetKeyDown(KeyCode.Alpha2)) PressModule(Weapons[1]);
                if (Input.GetKeyDown(KeyCode.Alpha3)) PressModule(Weapons[2]);
                if (Input.GetKeyDown(KeyCode.Alpha4)) PressModule(Weapons[3]);
                if (Input.GetKeyDown(KeyCode.Alpha5)) PressModule(Weapons[4]);
                if (Input.GetKeyDown(KeyCode.Alpha6)) PressModule(Weapons[5]);
                if (Input.GetKeyDown(KeyCode.Alpha7)) PressModule(Weapons[6]);
                if (Input.GetKeyDown(KeyCode.Alpha8)) PressModule(Weapons[7]);
                if (Input.GetKeyDown(KeyCode.Alpha9)) PressModule(Weapons[8]);
                if (Input.GetKeyDown(KeyCode.Alpha0)) PressModule(Weapons[9]);
                break;

            case 2:
                if (Input.GetKeyDown(KeyCode.Alpha1)) PressModule(Crews[0]);
                if (Input.GetKeyDown(KeyCode.Alpha2)) PressModule(Crews[1]);
                if (Input.GetKeyDown(KeyCode.Alpha3)) PressModule(Crews[2]);
                if (Input.GetKeyDown(KeyCode.Alpha4)) PressModule(Crews[3]);
                if (Input.GetKeyDown(KeyCode.Alpha5)) PressModule(Crews[4]);
                if (Input.GetKeyDown(KeyCode.Alpha6)) PressModule(Crews[5]);
                if (Input.GetKeyDown(KeyCode.Alpha7)) PressModule(Crews[6]);
                if (Input.GetKeyDown(KeyCode.Alpha8)) PressModule(Crews[7]);
                if (Input.GetKeyDown(KeyCode.Alpha9)) PressModule(Crews[8]);
                if (Input.GetKeyDown(KeyCode.Alpha0)) PressModule(Crews[9]);
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
        CurrentOrderMarker.gameObject.SetActive(false);
        CurrentOrderMarker = null;
        CurrentModuleSelected.SetDeselect();
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
        CurrentModuleSelected.SetDeselect();
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
        if (CurrentModuleSelected) CurrentModuleSelected.SetDeselect();
        CurrentModuleSelected = mod;
        CurrentModuleSelected.SetSelect();
        RefreshCommandInterface();
    }

    public void HandleDespawningOfDrifter(DRIFTER drifter)
    {
        foreach (CREW orderMarker in CrewReferences)
        {
            if (orderMarker.GetOrderTransform() == drifter.Space)
            {
                orderMarker.SetOrderPointRpc(Vector3.zero);
            }
        }
        foreach (ModuleWeapon orderMarker in WeaponReferences)
        {
            if (orderMarker.GetOrderTransform() == drifter.Space)
            {
                orderMarker.SetOrderPointRpc(Vector3.zero);
            }
        }
        foreach (UI_Module orderMarker in Crews)
        {
            if (orderMarker.OrderMarker.transform.parent == drifter.transform) orderMarker.DisconnectOrderMarker();
        }
        foreach (UI_Module orderMarker in Weapons)
        {
            if (orderMarker.OrderMarker.transform.parent == drifter.transform) orderMarker.DisconnectOrderMarker();
        }
    }

    private List<CREW> CrewReferences = new();
    private List<Module> ModuleReferences = new();
    private List<ModuleWeapon> WeaponReferences = new();


    public SpriteRenderer SelectionBox;
    public void SetSelection(Transform trans)
    {
        SelectionBoxFollow = trans;
        SelectionBoxTargetScale = 1.5f;
        SelectionBoxFade = 1.1f;
        if (!SelectionBoxActive)
        {
            StartCoroutine(SelectionBoxFadeNum());
        }
    }
    bool SelectionBoxActive = false;
    Transform SelectionBoxFollow;
    float SelectionBoxTargetScale = 1f;
    float SelectionBoxFade = 1f;
    IEnumerator SelectionBoxFadeNum()
    {
        SelectionBoxActive = true;
        while (SelectionBoxFade > 0)
        {
            SelectionBox.transform.position = SelectionBoxFollow.position;

            SelectionBoxFade = Mathf.Max(SelectionBoxFade - Time.deltaTime * 2f, 0);
            SelectionBox.color = new Color(0, 1, 0, SelectionBoxFade);

            SelectionBoxTargetScale = Mathf.Max(SelectionBoxTargetScale - Time.deltaTime * 4f, 1);
            SelectionBox.transform.localScale = new Vector3(SelectionBoxTargetScale * 25f, SelectionBoxTargetScale *25f, 1);
            yield return null;
        }
        SelectionBoxActive = false;
    }
}
