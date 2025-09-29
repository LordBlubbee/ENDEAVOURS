using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public TextMeshProUGUI UseItemButton;
    public Image InnerIconImage;
    public Image ImageBorder;

    public void SetInventoryItem(ScriptableEquippable equippable)
    {
        InnerIconImage.sprite = equippable.ItemIcon;
    }

    public enum EquipStates
    {
        NONE,
        SUCCESS,
        FAIL
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
        }
    }
}
