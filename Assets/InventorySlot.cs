using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public TextMeshProUGUI UseItemButton;
    public Image InnerIconImage;
    public Image ImageBorder;
    public EquipStates DefaultEquipState;
    ScriptableEquippable EquipItem = null;

    public ScriptableEquippable GetEquippedItem()
    {
        return EquipItem;
    }
    private void OnEnable()
    {
        SetEquipState(DefaultEquipState);
    }
    public void SetInventoryItem(ScriptableEquippable equippable)
    {
        EquipItem = equippable;
        if (equippable) InnerIconImage.sprite = equippable.ItemIcon;
        else InnerIconImage.sprite = null;
        SetEquipState(DefaultEquipState);
    }

    public enum EquipStates
    {
        NONE,
        SUCCESS,
        FAIL,
        INVENTORY_WEAPON,
        INVENTORY_ARTIFACT,
        INVENTORY_ARMOR,
        INVENTORY_MODULE
    }
    public void SetEquipState(EquipStates bol)
    {
        switch (bol)
        {
            case EquipStates.NONE:
                ImageBorder.color = Color.black;
                break;
            case EquipStates.SUCCESS:
                ImageBorder.color = Color.yellow;
                break;
            case EquipStates.FAIL:
                ImageBorder.color = Color.red;
                break;
            case EquipStates.INVENTORY_WEAPON:
                ImageBorder.color = new Color(1, 0.5f, 0);
                break;
            case EquipStates.INVENTORY_ARTIFACT:
                ImageBorder.color = new Color(0.2f, 0.8f, 1);
                break;
            case EquipStates.INVENTORY_ARMOR:
                ImageBorder.color = new Color(0.1f, 0.6f, 0.1f);
                break;
            case EquipStates.INVENTORY_MODULE:
                ImageBorder.color = new Color(0.7f, 0.7f, 0.2f);
                break;
        }
    }

    public void GrabItem()
    {
        UI.ui.InventoryUI.GrabItem(this, InnerIconImage, EquipItem);
    }

    public void DepositItem()
    {
        // Build pointer data for current mouse/finger position
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Perform UI raycast
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Search for the first InventorySlot in results
        foreach (var result in results)
        {
            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null)
            {
                Debug.Log($"Depositing item {gameObject.name} into slot {slot.name}");
                UI.ui.InventoryUI.DepositItem(slot);
                return;
            }
        }

        // If nothing valid found
        Debug.Log($"No valid slot found for {gameObject.name}, dropping failed.");
    }
}
