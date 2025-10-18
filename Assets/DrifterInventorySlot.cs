using UnityEngine;
using UnityEngine.UI;

public class DrifterInventorySlot : InventorySlot
{
    public Image[] UpgradeBoxes;
    public Image SelectedBox;
    public void UpdateInventorySlot()
    {
        if (GetEquippedItem() == null)
        {
            foreach (Image img in UpgradeBoxes)
            {
                img.gameObject.SetActive(false);
            }
            return;
        }
        foreach (Image img in UpgradeBoxes)
        {
            img.color = Color.gray;
            img.gameObject.SetActive(true);
        }
    }
    public void WhenPressed()
    {
        if (GetEquippedItem() == null)
        {
            return;
        }
        UI.ui.InventoryUI.PressDrifterModule(this);
    }
}
