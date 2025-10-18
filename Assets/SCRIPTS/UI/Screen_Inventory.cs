using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_Inventory : MonoBehaviour
{
    public GameObject[] Subscreens;
    public GameObject SubscreenEquipment;
    public GameObject SubscreenInventory;
    public GameObject SubscreenDrifter;
    public GameObject SubscreenAttributes;
    public GameObject SubscreenDrifterWeapons;
    public InventorySlot[] InventoryWeapons;
    public InventorySlot InventoryArmor;
    public InventorySlot[] InventoryArtifacts;
    public InventorySlot[] InventoryCargo;

    private void OnEnable()
    {
        SkillRefresh(); 
        RefreshPlayerEquipment(); 
        RefreshShipEquipment();
        RefreshDrifterStats();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
        }
        if (HoldingItemTile)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HoldingItemTile.transform.position = new Vector3(mouse.x,mouse.y);
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Stop holding item!");
                StopHoldingItem();
            }
        }
        if (SubscreenEquipment.activeSelf)
        {
            RefreshPlayerEquipment();
        }
        if (SubscreenInventory.activeSelf)
        {
            RefreshShipEquipment();
        }
        if (SubscreenDrifter.activeSelf)
        {
            RefreshDrifterSubscreen();
        }
        if (SubscreenDrifterWeapons.activeSelf)
        {
            RefreshDrifterWeaponSubscreen();
        }
        if (SubscreenModuleEditor.activeSelf)
        {
            RefreshSubscreenModuleEditor();
        }
        RefreshDrifterStats();
    }
    public void PressCraftAmmo()
    {
        if (CO.co.Resource_Ammo.Value < 10) return;
        CO.co.PlayerMainDrifter.CreateAmmoCrateRpc(LOCALCO.local.GetPlayer().transform.position);
    }

    void RefreshPlayerEquipment()
    {
        for (int i = 0; i < 3; i++) InventoryWeapons[i].SetInventoryItem(LOCALCO.local.GetPlayer().EquippedWeapons[i]);
        for (int i = 0; i < 3; i++) InventoryArtifacts[i].SetInventoryItem(LOCALCO.local.GetPlayer().EquippedArtifacts[i]);
        InventoryArmor.SetInventoryItem(LOCALCO.local.GetPlayer().EquippedArmor);
    }

    void RefreshShipEquipment()
    {
        for (int i = 0; i < InventoryCargo.Length; i++)
        {
            if (CO.co.Drifter_Inventory.Count <= i) InventoryCargo[i].SetInventoryItem(null);
            else InventoryCargo[i].SetInventoryItem(CO.co.Drifter_Inventory[i]);
        }
    }

    void RefreshDrifterStats()
    {
        DrifterTexResources.text = $"MATERIALS: {CO.co.Resource_Materials.Value}";
        DrifterTexSupplies.text = $"SUPPLIES: {CO.co.Resource_Supplies.Value}";
        DrifterTexAmmunition.text = $"AMMUNITION: {CO.co.Resource_Ammo.Value}";
        DrifterTexTechnology.text = $"TECHNOLOGY: {CO.co.Resource_Tech.Value}";
    }
    [Header("Drifter Inventory")]
    public TextMeshProUGUI DrifterTexResources;
    public TextMeshProUGUI DrifterTexSupplies;
    public TextMeshProUGUI DrifterTexAmmunition;
    public TextMeshProUGUI DrifterTexTechnology;

    [Header("Skills")]
    public TextMeshProUGUI SkillPointTex;
    public string[] SkillName;
    public TextMeshProUGUI[] SkillTex;
    public Image[] SkillLevelButton;
    public TextMeshProUGUI[] SkillLevelTex;
    public void LevelSkill(int ID)
    {
        int LevelNeed;
        switch (LOCALCO.local.GetPlayer().GetATT(ID))
        {
            case 0:
                LevelNeed = 1;
                break;
            case 1:
                LevelNeed = 1;
                break;
            case 2:
                LevelNeed = 1;
                break;
            case 3:
                LevelNeed = 2;
                break;
            case 4:
                LevelNeed = 3;
                break;
            case 5:
                LevelNeed = 4;
                break;
            case 6:
                LevelNeed = 5;
                break;
            case 7:
                LevelNeed = 6;
                break;
            case 8:
                LevelNeed = 7;
                break;
            case 9:
                LevelNeed = 8;
                break;
            default:
                LevelNeed = 999;
                break;
        }
        if (LOCALCO.local.GetPlayer().SkillPoints.Value < LevelNeed) return;
        LOCALCO.local.GetPlayer().IncreaseATTRpc(ID);
    }
    public void SkillRefresh()
    {
        SkillPointTex.text = $"SKILL POINTS: ({LOCALCO.local.GetPlayer().SkillPoints.Value})";
        for (int i = 0; i < 8; i++)
        {
            if (LOCALCO.local.GetPlayer().CharacterBackground)
            {
                if (LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i] > 0)
                {
                    SkillTex[i].text = $"{SkillName[i]} <color=green>({LOCALCO.local.GetPlayer().GetATT(i) + LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i]})";
                }
                else if (LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i] < 0)
                {
                    SkillTex[i].text = $"{SkillName[i]} <color=red> ({LOCALCO.local.GetPlayer().GetATT(i) + LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i]})";
                }
                else
                {
                    SkillTex[i].text = $"{SkillName[i]} ({LOCALCO.local.GetPlayer().GetATT(i)})";
                }
            }
            else
            {
                SkillTex[i].text = $"{SkillName[i]} ({LOCALCO.local.GetPlayer().GetATT(i)})";
            }
            int LevelNeed;
            switch (LOCALCO.local.GetPlayer().GetATT(i))
            {
                case 0:
                    LevelNeed = 1;
                    SkillTex[i].color = new Color(1, 0, 0);
                    break;
                case 1:
                    LevelNeed = 1;
                    SkillTex[i].color = new Color(1, 0.5f, 0);
                    break;
                case 2:
                    LevelNeed = 1;
                    SkillTex[i].color = new Color(1f, 0.8f, 0);
                    break;
                case 3:
                    LevelNeed = 2;
                    SkillTex[i].color = new Color(0.8f, 0.9f, 0);
                    break;
                case 4:
                    LevelNeed = 3;
                    SkillTex[i].color = new Color(0.6f, 0.9f, 0);
                    break;
                case 5:
                    LevelNeed = 4;
                    SkillTex[i].color = new Color(0.4f, 0.9f, 0);
                    break;
                case 6:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0.2f, 0.9f, 0);
                    break;
                case 7:
                    LevelNeed = 6;
                    SkillTex[i].color = new Color(0, 1, 0);
                    break;
                case 8:
                    LevelNeed = 7;
                    SkillTex[i].color = new Color(0, 1, 0.5f);
                    break;
                case 9:
                    LevelNeed = 8;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
                default:
                    LevelNeed = 999;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
            }
            if (LevelNeed > LOCALCO.local.GetPlayer().SkillPoints.Value) SkillLevelButton[i].gameObject.SetActive(false);
            else
            {
                SkillLevelButton[i].gameObject.SetActive(true);
                SkillLevelTex[i].text = $"LEVEL ({LevelNeed})";
                if (LevelNeed > 1) SkillLevelTex[i].color = Color.yellow;
                else SkillLevelTex[i].color = Color.green;
            }
        }
    }
    public void OpenSubscreen(GameObject ob)
    {
        DeselectDrifterSlot();
        foreach (GameObject sub in Subscreens)
        {
            sub.SetActive(false);
        }
        ob.SetActive(true);
        if (ob == SubscreenEquipment || ob == SubscreenDrifter) SubscreenInventory.SetActive(true);
    }

    InventorySlot CurrentDraggingSlot;
    ScriptableEquippable HoldingItem = null;
    Image HoldingItemTile;
    public void GrabItem(InventorySlot slot, Image img, ScriptableEquippable eq)
    {
        if (eq == null) return;
        CurrentDraggingSlot = slot;
        HoldingItem = eq;
        HoldingItemTile = img;
        img.transform.SetParent(transform);
    }

    public void DepositItem(InventorySlot slot)
    {
        if (HoldingItemTile)
        {
            ScriptableEquippable slotSwap = slot.GetEquippedItem();
            if (!doesItemFitInSlot(HoldingItem, slot)) return;
            if (!doesItemFitInSlot(slotSwap, CurrentDraggingSlot)) return;
            slot.SetInventoryItem(HoldingItem);
            UpdateEquipmentBasedOnItem(slot);
            CurrentDraggingSlot.SetInventoryItem(slotSwap);
            UpdateEquipmentBasedOnItem(CurrentDraggingSlot);
            CO.co.RequestPeriodicInventoryUpdateRpc();
            Debug.Log($"We moved FROM {CurrentDraggingSlot} item: {CurrentDraggingSlot.GetEquippedItem()} (Supposed to be {slotSwap})");
            Debug.Log($"We moved TO {slot} item: {slot.GetEquippedItem()}  (Supposed to be {HoldingItem})");
        }
        StopHoldingItem();
    }

    private bool doesItemFitInSlot(ScriptableEquippable item, InventorySlot slot)
    {
        if (item == null) return true;
        if (slot.DefaultEquipState == InventorySlot.EquipStates.INVENTORY_ARMOR)
        {
            if (item is ScriptableEquippableArtifact)
            {
                if (((ScriptableEquippableArtifact)item).EquipType != ScriptableEquippableArtifact.EquipTypes.ARMOR)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        if (slot.DefaultEquipState == InventorySlot.EquipStates.INVENTORY_WEAPON)
        {
            if (item is ScriptableEquippableWeapon)
            {
            }
            else
            {
                return false;
            }
        }
        if (slot.DefaultEquipState == InventorySlot.EquipStates.INVENTORY_ARTIFACT)
        {
            if (item is ScriptableEquippableArtifact)
            {
                if (((ScriptableEquippableArtifact)item).EquipType != ScriptableEquippableArtifact.EquipTypes.ARTIFACT)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateEquipmentBasedOnItem(InventorySlot slot)
    {
        if (slot == InventoryArmor)
        {
            if (slot.GetEquippedItem() == null) LOCALCO.local.GetPlayer().EquipArmorRpc(null);
            else LOCALCO.local.GetPlayer().EquipArmorRpc(slot.GetEquippedItem().ItemResourceID);
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            if (slot == InventoryArtifacts[i])
            {
                if (slot.GetEquippedItem() == null) LOCALCO.local.GetPlayer().EquipArtifactRpc(i,null);
                else LOCALCO.local.GetPlayer().EquipArtifactRpc(i, slot.GetEquippedItem().ItemResourceID);
                return;
            }
            if (slot == InventoryWeapons[i])
            {
                if (slot.GetEquippedItem() == null) LOCALCO.local.GetPlayer().EquipWeaponRpc(i, null);
                else LOCALCO.local.GetPlayer().EquipWeaponRpc(i, slot.GetEquippedItem().ItemResourceID);
                return;
            }
        }
        for (int i = 0; i < InventoryCargo.Length; i++)
        {
            if (slot == InventoryCargo[i])
            {
                if (slot.GetEquippedItem() == null) CO.co.SetDrifterInventoryItemRpc(i, null);
                else CO.co.SetDrifterInventoryItemRpc(i, slot.GetEquippedItem().ItemResourceID);
                return;
            }
        }
    }
    public void StopHoldingItem()
    {
        if (HoldingItemTile)
        {
            HoldingItemTile.transform.SetParent(CurrentDraggingSlot.transform);
            HoldingItemTile.transform.localPosition = Vector3.zero;
        }
        HoldingItem = null;
        HoldingItemTile = null;
    }

    [Header("Drifter Modules")]
    public DrifterInventorySlot[] DrifterModuleSlots;
    public DrifterInventorySlot[] DrifterWeaponSlots;
    public GameObject SubscreenModuleEditor;
    public TextMeshProUGUI ModuleTitle;
    public TextMeshProUGUI ModuleDesc;
    public TextMeshProUGUI DrifterModuleBuy;
    public TextMeshProUGUI DrifterModuleSell;
    private DrifterInventorySlot SelectedDrifterSlot = null;

    private void DeselectDrifterSlot()
    {
        if (SelectedDrifterSlot != null) SelectedDrifterSlot.SelectedBox.gameObject.SetActive(false);
        SelectedDrifterSlot = null;
    }
    public void PressUpgradeDrifterSlot()
    {

    }
    public void PressDismantleDrifterSlot()
    {

    }

    private void RefreshDrifterSubscreen()
    {
        int ID = -1;
        int i = 0;
        while (i < DrifterModuleSlots.Length)
        {
            DrifterInventorySlot slot = DrifterModuleSlots[i];
            ID++;
            if (CO.co.PlayerMainDrifter.Interior.GetModules().Count > ID)
            {
                Module mod = CO.co.PlayerMainDrifter.Interior.GetModules()[ID];
                if (mod is ModuleWeapon) continue;
                if (!mod.ShowAsModule) continue;
                slot.SetInventoryItem(mod.ShowAsModule);
                i++;
            } else
            {
                i++;
                slot.SetInventoryItem(null);
            }
            slot.UpdateInventorySlot();
        }
    }
    private void RefreshDrifterWeaponSubscreen()
    {
        int ID = -1;
        int i = 0;
        while (i < DrifterWeaponSlots.Length)
        {
            DrifterInventorySlot slot = DrifterWeaponSlots[i];
            ID++;
            if (CO.co.PlayerMainDrifter.Interior.WeaponModules.Count > ID)
            {
                Module mod = CO.co.PlayerMainDrifter.Interior.WeaponModules[ID];
                if (!mod.ShowAsModule) continue;
                slot.SetInventoryItem(mod.ShowAsModule);
                i++;
            }
            else
            {
                i++;
                slot.SetInventoryItem(null);
            }
            slot.UpdateInventorySlot();
        }
    }
    public void PressDrifterModule(DrifterInventorySlot slot)
    {
        if (slot.GetEquippedItem() == null)
        {
            return;
        }
        DeselectDrifterSlot();
        SelectedDrifterSlot = slot;
        SelectedDrifterSlot.SelectedBox.gameObject.SetActive(false);
        SubscreenModuleEditor.gameObject.SetActive(true);
        RefreshSubscreenModuleEditor();
    }
    private void RefreshSubscreenModuleEditor()
    {
        if (!SelectedDrifterSlot) return;
        ModuleTitle.text = SelectedDrifterSlot.GetEquippedItem().ItemName;
        ModuleDesc.text = SelectedDrifterSlot.GetEquippedItem().ItemDesc;
    }
}
