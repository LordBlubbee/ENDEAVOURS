using UnityEngine;
using UnityEngine.UI;

public class DrifterInventorySlot : InventorySlot
{
    public Module ModuleLink;
    public Image[] UpgradeBoxes;
    public Image SelectedBox;
    public void UpdateInventorySlot(Module moduleLink)
    {
        ModuleLink = moduleLink;
        if (GetEquippedItem() == null)
        {
            foreach (Image img in UpgradeBoxes)
            {
                img.gameObject.SetActive(false);
            }
            return;
        }
        int i = 0;
        foreach (Image img in UpgradeBoxes)
        {
            img.gameObject.SetActive(i < ModuleLink.MaxModuleLevel);
            if (i < ModuleLink.ModuleLevel.Value)
            {
                img.color = Color.yellow;
            } else
            {
                img.color = Color.gray;
            }
            i++;
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
