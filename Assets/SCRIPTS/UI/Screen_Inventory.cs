using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
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
    public GameObject SubscreenRest;
    public GameObject SubscreenCrew;
    public InventorySlot[] InventoryWeapons;
    public InventorySlot InventoryArmor;
    public InventorySlot[] InventoryArtifacts;
    public InventorySlot[] InventoryCargo;
    public TextMeshProUGUI SubscreenCharacterButtonTex;
    public Slider DrifterHealthSlider;
    public TextMeshProUGUI DrifterHealthTex;
    public TextMeshProUGUI DrifterRepairTex;
    public GameObject PlayerCrewButton;
    private void OnEnable()
    {
        SkillRefresh(); 
        RefreshPlayerEquipment(); 
        RefreshShipEquipment();
        RefreshDrifterStats();

        PlayerCrewButton.gameObject.SetActive(CO.co.GetLOCALCO().Count > 1);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(KeyCode.I) && !UI.ui.ChatUI.ChatScreen.activeSelf))
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
        if (SubscreenRest.activeSelf)
        {
            RefreshRest();
        }
        if (SubscreenAttributes.activeSelf)
        {
            SkillRefresh();
        }
        if (SubscreenCrew.activeSelf)
        {
            UpdateCrewScreen();
        }
        RefreshDrifterStats();
    }

   
    /*public void PressCraftAmmo()
    {
        if (CO.co.Resource_Ammo.Value < 10) return;
        CO.co.PlayerMainDrifter.CreateAmmoCrateRpc(LOCALCO.local.GetPlayer().transform.position);
    }*/

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
        if (CO.co.AreWeResting.Value)
        {
            SubscreenRestButton.gameObject.SetActive(true);
            SubscreenRestButtonTex.text = "REST";
            SubscreenRestButtonTex.color = Color.cyan;
        }
        else if (CO.co.GetShopItems().Count > 0)
        {
            SubscreenRestButton.gameObject.SetActive(true);
            SubscreenRestButtonTex.text = "DEALS";
            SubscreenRestButtonTex.color = Color.yellow;
        } else
        {
            SubscreenRestButton.gameObject.SetActive(false);
        }
        DrifterTexResources.text = $"MATERIALS: {CO.co.Resource_Materials.Value}";
        DrifterTexSupplies.text = $"SUPPLIES: {CO.co.Resource_Supplies.Value}";
        DrifterTexAmmunition.text = $"AMMUNITION: {CO.co.Resource_Ammo.Value}";
        DrifterTexTechnology.text = $"TECHNOLOGY: {CO.co.Resource_Tech.Value}";

        if (LOCALCO.local.GetPlayer().SkillPoints.Value > 2)
        {
            SubscreenCharacterButtonTex.text = "LEVEL UP";
            SubscreenCharacterButtonTex.color = Color.cyan;
        } else
        {
            SubscreenCharacterButtonTex.text = "CHARACTER";
            SubscreenCharacterButtonTex.color = Color.yellow;
        }
    }
    [Header("Drifter Inventory")]
    public TextMeshProUGUI DrifterTexResources;
    public TextMeshProUGUI DrifterTexSupplies;
    public TextMeshProUGUI DrifterTexAmmunition;
    public TextMeshProUGUI DrifterTexTechnology;

    [Header("Skills")]
    public TextMeshProUGUI SkillPointTex;
    public TextMeshProUGUI ExperienceTex;
    public Slider ExperienceSlider;
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
                LevelNeed = 2;
                break;
            case 5:
                LevelNeed = 3;
                break;
            case 6:
                LevelNeed = 3;
                break;
            case 7:
                LevelNeed = 4;
                break;
            default:
                LevelNeed = 5;
                break;
        }
        if (LOCALCO.local.GetPlayer().SkillPoints.Value < LevelNeed) return;
        LOCALCO.local.GetPlayer().IncreaseATTRpc(ID);
    }
    public void SkillRefresh()
    {
        SkillPointTex.text = $"SKILL POINTS: ({LOCALCO.local.GetPlayer().SkillPoints.Value})";
        SkillPointTex.color = (LOCALCO.local.GetPlayer().SkillPoints.Value > 0) ? Color.cyan : Color.white;
        ExperienceTex.text = $"XP: ({LOCALCO.local.GetPlayer().XPPoints.Value}/100)";
        ExperienceSlider.value = (float)LOCALCO.local.GetPlayer().XPPoints.Value / 100f;
        for (int i = 0; i < 8; i++)
        {
            int Bonus = LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i] + LOCALCO.local.GetPlayer().ModifyAttributes[i];
            if (Bonus > 0)
            {
                SkillTex[i].text = $"{SkillName[i]} <color=green>({LOCALCO.local.GetPlayer().GetATT(i) + Bonus})";
            }
            else if (Bonus < 0)
            {
                SkillTex[i].text = $"{SkillName[i]} <color=red> ({LOCALCO.local.GetPlayer().GetATT(i) + Bonus})";
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
                    LevelNeed = 2;
                    SkillTex[i].color = new Color(0.6f, 0.9f, 0);
                    break;
                case 5:
                    LevelNeed = 3;
                    SkillTex[i].color = new Color(0.4f, 0.9f, 0);
                    break;
                case 6:
                    LevelNeed = 3;
                    SkillTex[i].color = new Color(0.2f, 0.9f, 0);
                    break;
                case 7:
                    LevelNeed = 4;
                    SkillTex[i].color = new Color(0, 1, 0);
                    break;
                case 8:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0, 1, 0.5f);
                    break;
                case 9:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
                default:
                    LevelNeed = 5;
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
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        DeselectDrifterSlot();
        foreach (GameObject sub in Subscreens)
        {
            sub.SetActive(false);
        }
        ob.SetActive(true);
        if (ob == SubscreenEquipment || ob == SubscreenDrifter || ob == SubscreenDrifterWeapons) SubscreenInventory.SetActive(true);
    }

    public GameObject SellButton;
    public TextMeshProUGUI SellTex;
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

        SellButton.gameObject.SetActive(true);
        SellTex.text = $"SCRAP\n";
        if (HoldingItem.SellMaterials > 0) SellTex.text += $"<color=yellow>[+{HoldingItem.SellMaterials}M]</color> ";
        if (HoldingItem.SellSupplies > 0) SellTex.text += $"<color=green>[+{HoldingItem.SellSupplies}S]</color> ";
        if (HoldingItem.SellTech > 0) SellTex.text += $"<color=#00AAFF>[+{HoldingItem.SellTech}T]</color> ";
    }
    public void DepositItem(InventorySlot slot)
    {
        if (HoldingItemTile)
        {
            ScriptableEquippable slotSwap = slot.GetEquippedItem();
            if (CurrentDraggingSlot.GetEquippedItem() != HoldingItem)
            {
                StopHoldingItem();
                return;
            }
            if (!doesItemFitInSlot(HoldingItem, slot)) return;
            for (int i = 0; i < HoldingItem.MinimumAttributes.Length; i++)
            {
                if (HoldingItem.MinimumAttributes[i] > LOCALCO.local.GetPlayer().GetATT_Total(i))
                {
                    AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
                    return;
                }
            }
            if (HoldingItem is ScriptableEquippableModule)
            {
                ScriptableEquippableModule module = (ScriptableEquippableModule)HoldingItem;
                if (module.EquipType == ScriptableEquippableModule.EquipTypes.WEAPON)
                {
                    if (CO.co.PlayerMainDrifter.Interior.WeaponModuleLocations.Count <= CO.co.PlayerMainDrifter.Interior.WeaponModules.Count)
                    {
                        AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
                        return;
                    }
                } else
                {
                    if (CO.co.PlayerMainDrifter.Interior.SystemModuleLocations.Count <= CO.co.PlayerMainDrifter.Interior.SystemModules.Count)
                    {
                        AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
                        return;
                    }
                }
            }
            if (!doesItemFitInSlot(slotSwap, CurrentDraggingSlot)) return;
            slot.SetInventoryItem(HoldingItem);
            UpdateEquipmentBasedOnItem(slot);
            CurrentDraggingSlot.SetInventoryItem(slotSwap);
            UpdateEquipmentBasedOnItem(CurrentDraggingSlot);
            CO.co.RequestPeriodicInventoryUpdateRpc();
            Debug.Log($"We moved FROM {CurrentDraggingSlot} item: {CurrentDraggingSlot.GetEquippedItem()} (Supposed to be {slotSwap})");
            Debug.Log($"We moved TO {slot} item: {slot.GetEquippedItem()} (Supposed to be {HoldingItem})");

            if (HoldingItem is ScriptableEquippableModule)
            {
                AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
                CO.co.PlayerMainDrifter.Interior.AddModuleRpc(HoldingItem.GetItemResourceIDFull());
            }
        }
        StopHoldingItem();
    }
    public void DepositItemInSell()
    {
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            StopHoldingItem();
            return;
        }
        if (HoldingItemTile)
        {
            int MaterialSellReward = CurrentDraggingSlot.GetEquippedItem().SellMaterials;
            int SuppliesSellReward = CurrentDraggingSlot.GetEquippedItem().SellSupplies;
            int TechSellReward = CurrentDraggingSlot.GetEquippedItem().SellTech;
            CO.co.RequestSellingItemRpc(MaterialSellReward, SuppliesSellReward, TechSellReward);
            CurrentDraggingSlot.SetInventoryItem(null);
            UpdateEquipmentBasedOnItem(CurrentDraggingSlot);
            CO.co.RequestPeriodicInventoryUpdateRpc();

            AUDCO.aud.PlaySFX(AUDCO.aud.Salvage);
        }
        StopHoldingItem();
    }

    private bool doesItemFitInSlot(ScriptableEquippable item, InventorySlot slot)
    {
        if (item == null) return true;
        switch (slot.DefaultEquipState)
        {
            case InventorySlot.EquipStates.INVENTORY_ARMOR:
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
                break;
            case InventorySlot.EquipStates.INVENTORY_WEAPON:
                if (!(item is ScriptableEquippableWeapon)) return false;
                break;
            case InventorySlot.EquipStates.INVENTORY_ARTIFACT:
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
                break;
            case InventorySlot.EquipStates.INVENTORY_MODULE:
                if (!(item is ScriptableEquippableModule)) return false;
                if (slot.GetEquippedItem() != null) return false;
                break;
        }
        return true;
    }

    private void UpdateEquipmentBasedOnItem(InventorySlot slot)
    {
        if (slot == InventoryArmor)
        {
            if (slot.GetEquippedItem() == null) LOCALCO.local.GetPlayer().EquipArmorRpc(null);
            else LOCALCO.local.GetPlayer().EquipArmorRpc(slot.GetEquippedItem().GetItemResourceIDShort());
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            if (slot == InventoryArtifacts[i])
            {
                if (slot.GetEquippedItem() == null) LOCALCO.local.GetPlayer().EquipArtifactRpc(i,null);
                else LOCALCO.local.GetPlayer().EquipArtifactRpc(i, slot.GetEquippedItem().GetItemResourceIDShort());
                return;
            }
            if (slot == InventoryWeapons[i])
            {
                if (slot.GetEquippedItem() == null) LOCALCO.local.GetPlayer().EquipWeaponRpc(i, null);
                else LOCALCO.local.GetPlayer().EquipWeaponRpc(i, slot.GetEquippedItem().GetItemResourceIDShort());
                return;
            }
        }
        for (int i = 0; i < InventoryCargo.Length; i++)
        {
            if (slot == InventoryCargo[i])
            {
                if (slot.GetEquippedItem() == null) CO.co.SetDrifterInventoryItemRpc(i, null);
                else CO.co.SetDrifterInventoryItemRpc(i, slot.GetEquippedItem().GetItemResourceIDFull());
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
        SellButton.gameObject.SetActive(false);
    }

    [Header("Drifter Modules")]
    public DrifterInventorySlot[] DrifterModuleSlots;
    public DrifterInventorySlot[] DrifterWeaponSlots;
    public GameObject SubscreenModuleEditor;
    public TextMeshProUGUI ModuleTitle;
    public TextMeshProUGUI ModuleDesc;
    public GameObject DrifterModuleBuy;
    public GameObject DrifterModuleSell;
    public TextMeshProUGUI DrifterModuleBuyTex;
    public TextMeshProUGUI DrifterModuleSellTex;
    public Image DrifterMapImage;
    public RectTransform DrifterMapWidthAnchor;
    public RectTransform DrifterMapHeightAnchor;
    public RectTransform DrifterMapWidthAnchorLeft;
    public RectTransform DrifterMapHeightAnchorBottom;
    public Image DrifterMapIcon;
    private DrifterInventorySlot SelectedDrifterSlot = null;

    private void DeselectDrifterSlot()
    {
        if (SelectedDrifterSlot != null) SelectedDrifterSlot.SelectedBox.gameObject.SetActive(false);
        SelectedDrifterSlot = null;
        SubscreenModuleEditor.gameObject.SetActive(false);
    }
    public void PressUpgradeDrifterSlot()
    {
        if (!SelectedDrifterSlot.ModuleLink) return;
        if (SelectedDrifterSlot.ModuleLink.ModuleLevel.Value == SelectedDrifterSlot.ModuleLink.MaxModuleLevel - 1) return;
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        int MoneyNeeded = SelectedDrifterSlot.ModuleLink.ModuleUpgradeMaterials[SelectedDrifterSlot.ModuleLink.ModuleLevel.Value];
        if (MoneyNeeded > CO.co.Resource_Materials.Value)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        int TechNeeded = SelectedDrifterSlot.ModuleLink.ModuleUpgradeTechs[SelectedDrifterSlot.ModuleLink.ModuleLevel.Value];
        if (TechNeeded > CO.co.Resource_Tech.Value)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        SelectedDrifterSlot.ModuleLink.SendUpgradeRpc();
        AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
    }
    public void PressDismantleDrifterSlot()
    {
        if (!SelectedDrifterSlot.ModuleLink) return;
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (SelectedDrifterSlot.ModuleLink.ModuleType == Module.ModuleTypes.NAVIGATION
            || SelectedDrifterSlot.ModuleLink.ModuleType == Module.ModuleTypes.ENGINES
            || SelectedDrifterSlot.ModuleLink.ModuleType == Module.ModuleTypes.MEDICAL)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Salvage);
        SelectedDrifterSlot.ModuleLink.SalvageRpc();
       // CO.co.AddInventoryItemRpc(SelectedDrifterSlot.GetEquippedItem().ItemResourceID);
       // SelectedDrifterSlot.SetInventoryItem(null);
       // UpdateEquipmentBasedOnItem(SelectedDrifterSlot);
        CO.co.RequestPeriodicInventoryUpdateRpc();
        DeselectDrifterSlot();

        RefreshSubscreenModuleEditor();
    }

    public void RefreshDrifterSubscreen()
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
                if (!mod.ShowModule) continue;
                slot.gameObject.SetActive(true);
                slot.SetInventoryItem(mod.ShowAsModule); 
                slot.UpdateInventorySlot(mod);
                i++;
            } else
            {
                i++;
                if (CO.co.PlayerMainDrifter.Interior.SystemModuleLocations.Count+4 > i)
                {
                    slot.gameObject.SetActive(true);
                    slot.SetInventoryItem(null);
                    slot.UpdateInventorySlot(null);
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }
    }
    public void RefreshDrifterWeaponSubscreen()
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
                if (!mod.ShowModule) continue; 
                slot.gameObject.SetActive(true);
                slot.SetInventoryItem(mod.ShowAsModule);
                slot.UpdateInventorySlot(mod);
                i++;
            }
            else
            {
                i++;
                if (CO.co.PlayerMainDrifter.Interior.WeaponModuleLocations.Count > ID)
                {
                    slot.gameObject.SetActive(true);
                    slot.SetInventoryItem(null);
                    slot.UpdateInventorySlot(null);
                } else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }
    }
    public void PressDrifterModule(DrifterInventorySlot slot)
    {
        if (slot.GetEquippedItem() == null)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        DeselectDrifterSlot();
        SelectedDrifterSlot = slot;
        SelectedDrifterSlot.SelectedBox.gameObject.SetActive(false);
        SubscreenModuleEditor.gameObject.SetActive(true);
        RefreshSubscreenModuleEditor();
    }
    private void RefreshSubscreenModuleEditor()
    {
        if (!SelectedDrifterSlot) return;

        DrifterMapImage.sprite = CO.co.PlayerMainDrifter.Spr.sprite;
        DrifterMapIcon.sprite = SelectedDrifterSlot.GetEquippedItem().ItemIcon;

        /*Vector3 ModulePosition = SelectedDrifterSlot.ModuleLink.transform.localPosition;
        float MaxWidth = Mathf.Abs(DrifterMapWidthAnchor.transform.x - DrifterMapImage.transform.anchoredPosition.x);
        float MaxHeight = Mathf.Abs(DrifterMapHeightAnchor.transform.y - DrifterMapImage.transform.anchoredPosition.y);

        Debug.Log($"ModulePosition is {ModulePosition.x}, MaxWidth is {MaxWidth}, Drifter Radius X is {CO.co.PlayerMainDrifter.RadiusX}, LocalPositionX is {(ModulePosition.x / (CO.co.PlayerMainDrifter.RadiusX * 0.8f)) * MaxWidth}");

        DrifterMapIcon.transform.localPosition = new Vector3((ModulePosition.x / (CO.co.PlayerMainDrifter.RadiusX*0.8f)) * MaxWidth, (ModulePosition.y / (CO.co.PlayerMainDrifter.RadiusY * 0.8f)) * MaxHeight, 0);
        */

        Vector3 ModulePosition = SelectedDrifterSlot.ModuleLink.transform.localPosition;
        float RelativeX = (-ModulePosition.y / CO.co.PlayerMainDrifter.RadiusY);
        float RelativeY = (ModulePosition.x / CO.co.PlayerMainDrifter.RadiusX);
        float XPos = Mathf.Lerp(DrifterMapWidthAnchorLeft.position.x, DrifterMapWidthAnchor.position.x, (RelativeX + 1) * 0.5f);
        float YPos = Mathf.Lerp(DrifterMapHeightAnchorBottom.position.y, DrifterMapHeightAnchor.position.y, (RelativeY + 1) * 0.5f);

        DrifterMapIcon.transform.position = new Vector3(XPos, YPos, 0);

        if (SelectedDrifterSlot.GetEquippedItem() == null)
        {
            ModuleTitle.text = "[DISMANTLED]";
            ModuleDesc.text = "";
            DrifterModuleBuy.SetActive(false);
            DrifterModuleSell.SetActive(false);
        } else
        {
            DrifterModuleSell.SetActive(true);
            ModuleTitle.text = SelectedDrifterSlot.GetEquippedItem().ItemName;

            string Data = "\n";
            Data += $"\nHEALTH: {SelectedDrifterSlot.ModuleLink.GetMaxHealth().ToString("0")}";
            Data += $"<color=green> (+100)</color>";
            switch (SelectedDrifterSlot.ModuleLink.ModuleType)
            {
                case Module.ModuleTypes.WEAPON:
                    ModuleWeapon wep = (ModuleWeapon)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDAMAGE: {wep.GetDamage().ToString("0")}";
                    if (wep.DamagePerLevel != 0) Data += $"<color=green> (+{wep.DamagePerLevel})</color>";
                    Data += $"\nVOLLEY: {wep.GetProjectileCount()}";
                    if (wep.ProjectilesPerLevel != 0) Data += $"<color=green> (+{wep.ProjectilesPerLevel})</color>";
                    Data += $"\nFIRERATE: {wep.GetFireCooldown().ToString("0.0")}";
                    if (wep.CooldownPerLevel != 0) Data += $"<color=green> ({wep.CooldownPerLevel})</color>";
                    Data += $"\nAMMO SIZE: {wep.MaxAmmo}";
                    break;
                case Module.ModuleTypes.ENGINES:
                    Data += $"\nDODGE: {(10+ SelectedDrifterSlot.ModuleLink.ModuleLevel.Value*5).ToString("0")}%";
                    Data += $"<color=green> (+5%)</color>";
                    DrifterModuleSell.SetActive(false);
                    break;
                case Module.ModuleTypes.NAVIGATION:
                    Data += $"\nDAMAGE REDUCTION: {(0+ SelectedDrifterSlot.ModuleLink.ModuleLevel.Value * 10).ToString("0")}%";
                    Data += $"<color=green> (+10%)</color>";
                    DrifterModuleSell.SetActive(false);
                    break;
                case Module.ModuleTypes.ARMOR:
                    ModuleArmor armor = (ModuleArmor)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nARMOR: {armor.GetMaxArmor().ToString("0")}";
                    if (armor.ArmorPerUpgrade != 0) Data += $"<color=green> (+{armor.ArmorPerUpgrade})</color>";
                    Data += $"\nREGEN: {armor.GetArmorRegen().ToString("0.0")}/s";
                    if (armor.ArmorRegenPerUpgrade != 0) Data += $"<color=green> (+{armor.ArmorRegenPerUpgrade}/s)</color>";
                    break;
                case Module.ModuleTypes.MEDICAL:
                    ModuleMedical med = (ModuleMedical)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nREGEN: {med.GetRegenAmount().ToString("0.0")}/s";
                    if (med.RegenAmountPerLevel != 0) Data += $"<color=green> (+{med.RegenAmountPerLevel}/s)</color>";
                    Data += $"\nAURA SIZE: {med.GetRegenAura().ToString("0.0")}";
                    if (med.RegenAuraPerLevel != 0) Data += $"<color=green> (+{med.RegenAuraPerLevel.ToString("0.0")})</color>";
                    DrifterModuleSell.SetActive(false);
                    break;
                case Module.ModuleTypes.STEAM_REACTOR:
                    ModuleSteamReactor steam = (ModuleSteamReactor)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDURATION: {steam.GetEffectDuration().ToString("0.0")}";
                    Data += $"<color=green> (+{steam.EffectDurationPerLevel})</color>";
                    Data += $"\nCOOLDOWN: {steam.GetEffectCooldownMax().ToString("0.0")}";
                    Data += $"<color=green> ({steam.EffectCooldownReductionPerLevel})</color>";

                    //Unique
                    Data += $"\nDODGE BOOST: +{(steam.GetDodgeBoost()*100f).ToString("0")}%";
                    Data += $"<color=green> (+5%)</color>";
                    Data += $"\nARMOR REGEN: +{steam.GetArmorBoost().ToString("0")}/s";
                    Data += $"<color=green> (+5/s)</color>";
                    break;
                case Module.ModuleTypes.INCENDIARY_STORAGE:
                    ModuleIncendiaryCrates incin = (ModuleIncendiaryCrates)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDURATION: {incin.GetEffectDuration().ToString("0.0")}";
                    Data += $"<color=green> (+{incin.EffectDurationPerLevel})</color>)";
                    Data += $"\nCOOLDOWN: {incin.GetEffectCooldownMax().ToString("0.0")}";
                    Data += $"<color=green> ({incin.EffectCooldownReductionPerLevel})</color>)";

                    //Unique
                    Data += $"\nDAMAGE BOOST: +{((incin.GetDamageBonusMod()-1f)*100f).ToString("0")}%";
                    Data += $"<color=green> (+10%)</color>";
                    Data += $"\nFIRE CHANCE: +{(incin.GetFlameBoost()*100f).ToString("0")}%";
                    Data += $"<color=green> (+10%)</color>";
                    break;
                case Module.ModuleTypes.SPEAKERS_BUFF:
                    ModuleSpeaker speakb = (ModuleSpeaker)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDURATION: {speakb.GetEffectDuration().ToString("0.0")}";
                    Data += $"<color=green> (+{speakb.EffectDurationPerLevel})</color>";
                    Data += $"\nCOOLDOWN: {speakb.GetEffectCooldownMax().ToString("0.0")}";
                    Data += $"<color=green> ({speakb.EffectCooldownReductionPerLevel})</color>";

                    //Unique
                    Data += $"\nMELEE DAMAGE: +{speakb.GetBuffEffectPower().ToString("0")}";
                    Data += $"<color=green> (+4)</color>";
                    Data += $"\nRANGED DAMAGE: +{speakb.GetBuffEffectPower().ToString("0")}";
                    Data += $"<color=green> (+4)</color>";
                    break;
                case Module.ModuleTypes.SPEAKERS_DEBUFF:
                    ModuleSpeaker speakd = (ModuleSpeaker)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDURATION: {speakd.GetEffectDuration().ToString("0.0")}";
                    Data += $"<color=green> (+{speakd.EffectDurationPerLevel})</color>";
                    Data += $"\nCOOLDOWN: {speakd.GetEffectCooldownMax().ToString("0.0")}";
                    Data += $"<color=green> ({speakd.EffectCooldownReductionPerLevel})</color>";

                    //Unique
                    Data += $"\nSLOWNESS: +{(speakd.GetBuffEffectPower() * 100f).ToString("0")}%";
                    Data += $"<color=green> (+50%)</color>";
                    break;
                case Module.ModuleTypes.SPEAKERS_HEAL:
                    ModuleSpeaker speakh = (ModuleSpeaker)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDURATION: {speakh.GetEffectDuration().ToString("0.0")}";
                    Data += $"<color=green> (+{speakh.EffectDurationPerLevel})</color>";
                    Data += $"\nCOOLDOWN: {speakh.GetEffectCooldownMax().ToString("0.0")}";
                    Data += $"<color=green> ({speakh.EffectCooldownReductionPerLevel})</color>";

                    //Unique
                    Data += $"\nHEALING: +{speakh.GetBuffEffectPower().ToString("0")}/s";
                    Data += $"<color=green> (+2/s)</color>";
                    break;
                case Module.ModuleTypes.VENGEANCE_PLATING:
                    ModuleVengeancePlating vengeance = (ModuleVengeancePlating)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDAMAGE: {vengeance.GetShrapnelDamage().ToString("0")}";
                    Data += $"<color=green> (+20)</color>";
                    Data += $"\nCHANCE: {(vengeance.GetShrapnelChance()*100f).ToString("0")}%";
                    Data += $"<color=green> (+10%)</color>";
                    break;
                case Module.ModuleTypes.DOOR_CONTROLLER:
                    ModuleDoorControls doorcontroller = (ModuleDoorControls)SelectedDrifterSlot.ModuleLink;
                    Data += $"\nDOOR HEALTH: +{doorcontroller.GetDoorBuff().ToString("0")}";
                    Data += $"<color=green> (+150)</color>";
                    break;

            }
            ModuleDesc.text = SelectedDrifterSlot.GetEquippedItem().ItemDesc+Data;
           
            DrifterModuleSell.SetActive(true);
            if (SelectedDrifterSlot.ModuleLink.ModuleUpgradeMaterials.Length <= SelectedDrifterSlot.ModuleLink.ModuleLevel.Value) {
                DrifterModuleBuy.SetActive(false);
            } else
            {
                DrifterModuleBuy.SetActive(true);
                int MaterialsNeeded = SelectedDrifterSlot.ModuleLink.ModuleUpgradeMaterials[SelectedDrifterSlot.ModuleLink.ModuleLevel.Value];
                int TechNeeded = SelectedDrifterSlot.ModuleLink.ModuleUpgradeTechs[SelectedDrifterSlot.ModuleLink.ModuleLevel.Value];
                if (TechNeeded > 0) DrifterModuleBuyTex.text = $"UPGRADE \n<color=yellow>-{MaterialsNeeded}M</color> <color=#0077FF>-{TechNeeded}T</color>";
                else DrifterModuleBuyTex.text = $"UPGRADE \n<color=yellow>-{MaterialsNeeded}M</color>";
            }
            int MaterialsSalvage = SelectedDrifterSlot.ModuleLink.GetMaterialSalvageWorth();
            int TechSalvage = SelectedDrifterSlot.ModuleLink.GetTechSalvageWorth();
            if (TechSalvage > 0) DrifterModuleSellTex.text = $"DISMANTLE \n<color=yellow>+{MaterialsSalvage}M</color> <color=#0077FF>+{TechSalvage}T</color>";
            else if (MaterialsSalvage > 0) DrifterModuleSellTex.text = $"DISMANTLE \n<color=yellow>+{MaterialsSalvage}M</color>";
            else DrifterModuleSellTex.text = $"DISMANTLE";
        } 
    }

    [Header("Rest Menu")]
    public GameObject SubscreenRestButton;
    public TextMeshProUGUI SubscreenRestButtonTex;
    public GameObject RestRepairButton;
    public TextMeshProUGUI RepairButtonTex;
    public TextMeshProUGUI RestMenuTitleTex;
    public List<ShopItemButton> ShopItems;
    public void OpenRest()
    {
        if (!CO.co.AreWeResting.Value && CO.co.GetShopItems().Count == 0)
        {
            return;
        }
        for (int i = 0; i < ShopItems.Count; i++)
        {
            ShopItem shopLink = null;
            if (CO.co.GetShopItems().Count > i)
            {
                shopLink = CO.co.GetShopItems()[i];
            }
            ShopItems[i].Init(shopLink);
        }
        OpenSubscreen(SubscreenRest);
    }
    public void PressRepairButton()
    {
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (CO.co.PlayerMainDrifter.GetHealthRelative() < 1)
        {
            if (CO.co.Resource_Materials.Value >= CO.co.GetDrifterRepairCost())
            {
                AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
                CO.co.RepairOurDrifterRpc();
                return;
            }
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
    }
    void RefreshRest()
    {
        if (!CO.co.AreWeResting.Value && CO.co.GetShopItems().Count == 0)
        {
            OpenSubscreen(SubscreenDrifter);
            return;
        }
        RestRepairButton.SetActive(CO.co.AreWeResting.Value);
        RepairButtonTex.text = $"REPAIR <color=green>(100)\r\n<color=yellow>-{CO.co.GetDrifterRepairCost()}M";
        DrifterHealthSlider.value = CO.co.PlayerMainDrifter.GetHealthRelative();
        DrifterHealthTex.text = $"DRIFTER INTEGRITY: {CO.co.PlayerMainDrifter.GetHealth().ToString("0")}/{CO.co.PlayerMainDrifter.GetMaxHealth().ToString("0")}";
        DrifterHealthTex.color = CO.co.PlayerMainDrifter.GetHealthRelative() > 0.75 ? Color.green : (CO.co.PlayerMainDrifter.GetHealthRelative() > 0.25 ? Color.yellow : Color.red);
        DrifterRepairTex.color = CO.co.PlayerMainDrifter.GetHealthRelative() < 1f ? Color.white : Color.gray;
        if (CO.co.GetShopItems().Count > 0)
        {
            if (CO.co.AreWeCrafting.Value)
            {
                RestMenuTitleTex.text = $"CRAFTING \n<color=green>-{((1f-CO.co.GetAlchemyCraftingModifier())*100f).ToString("0")}% PRICE</color>";
            }
            else
            {
                RestMenuTitleTex.text = "TRADING";
            }
        } else
        {
            RestMenuTitleTex.text = "RESTING";
        }   
    }

    [Header("CREW SCREEN")]
    public UI_CrewButton[] CrewListButtons;
    public GameObject CrewScreen;
    public Image CrewOuter;
    public Image CrewIcon;
    public Image CrewStripes;
    public TextMeshProUGUI CrewMaxTex;
    public TextMeshProUGUI CrewName;
    public TextMeshProUGUI CrewDescription;
    public TextMeshProUGUI CrewUpgradeTex;
    public TextMeshProUGUI CrewDismissTex;
    CREW SelectedCrew;
    private void UpdateCrewScreen()
    {
        List<CREW> Allies = CO.co.GetAlliedAICrew();
        CrewMaxTex.text = $"CREW MEMBERS ({Allies.Count}/{CO.co.PlayerMainDrifter.MaximumCrew})";
        CrewMaxTex.color = Allies.Count > CO.co.PlayerMainDrifter.MaximumCrew ? Color.red : Color.white;
        for (int i = 0; i < CrewListButtons.Length; i++)
        {
            if (Allies.Count > i)
            {
                CrewListButtons[i].gameObject.SetActive(true);
                CrewListButtons[i].SetCrew(Allies[i]);
            } else
            {
                CrewListButtons[i].gameObject.SetActive(false);
            }
        }
        if (CrewScreen.gameObject.activeSelf)
        {
            if (SelectedCrew == null) CrewScreen.gameObject.SetActive(false);
            else UpdateCrewButton();
        }
    }
    private void UpdateCrewButton()
    {
        CREW Crew = SelectedCrew;

        Color col = Crew.CharacterBackground.BackgroundColor;
        CrewOuter.color = new Color(col.r * 0.4f, col.g * 0.4f, col.b * 0.4f);
        CrewIcon.sprite = Crew.CharacterBackground.Sprite_Player;
        CrewStripes.sprite = Crew.CharacterBackground.Sprite_Stripes;
        CrewStripes.color = col;
        CrewName.text = $"{Crew.UnitName} {Crew.CharacterName.Value} - LEVEL {Crew.GetUnitUpgradeLevel()}";
        CrewName.color = Crew.CharacterBackground.BackgroundColor;
        CrewDescription.text = $"{Crew.UnitDescription}";

        CrewUpgradeTex.text = $"PROMOTE \n<color=green>-{Crew.GetLevelupCost()}S</color>";
        CrewDismissTex.text = $"DISMISS \n<color=green>+{Crew.GetDismissValue()}S</color>";
    }
    public void PressCrewButton(CREW Crew)
    {
        if (Crew == null) return;
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        SelectedCrew = Crew;

        UpdateCrewButton();
        CrewScreen.gameObject.SetActive(true);
    }
    public void PressCrewPromote()
    {
        if (SelectedCrew == null) return;
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (CO.co.Resource_Supplies.Value < SelectedCrew.GetLevelupCost())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
        SelectedCrew.BuyLevelupRpc();
    }
    public void PressCrewDismiss()
    {
        if (SelectedCrew == null) return;
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        CrewScreen.gameObject.SetActive(false);
        AUDCO.aud.PlaySFX(AUDCO.aud.Salvage);
        SelectedCrew.DismissCrewmemberRpc();
    }
}
