using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class Screen_Reward : MonoBehaviour
{

    [Header("REWARD SCREEN")]
    public bool RewardActive = false;
    public TextMeshProUGUI MaterialGain;
    public List<InventorySlot> InventorySlots;
    public void OpenRewardScreen(int Materials, int Supplies, int Ammo, int Tech, int XP, FactionReputation[] Facs, FixedString64Bytes[] RewardItemsGained)
    {
        RewardActive = true;
        MaterialGain.text = "";
        if (XP != 0) MaterialGain.text += $"<color=#FFFF00>XP: +{XP}</color>\n";
        if (Materials > 0) MaterialGain.text += $"<color=green>MATERIALS: +{Materials}</color>\n";
        else if (Materials < 0) MaterialGain.text += $"<color=red>MATERIALS: {Materials}</color>\n";
        if (Supplies > 0) MaterialGain.text += $"<color=green>SUPPLIES: +{Supplies}</color>\n";
        else if (Supplies < 0) MaterialGain.text += $"<color=red>SUPPLIES: {Supplies}</color>\n";
        if (Ammo > 0) MaterialGain.text += $"<color=green>AMMUNITION: +{Ammo}</color>\n";
        else if (Ammo < 0) MaterialGain.text += $"<color=red>AMMUNITION: {Ammo}</color>\n";
        if (Tech > 0) MaterialGain.text += $"<color=green>TECHNOLOGY: +{Tech}</color>\n";
        else if (Tech < 0) MaterialGain.text += $"<color=red>TECHNOLOGY: {Tech}</color>\n";

        foreach (FactionReputation fac in Facs)
        {
            if (fac.Amount > 0) MaterialGain.text += $"<color=green>{fac.Fac} +{fac.Amount}</color>\n";
            else if (fac.Amount < 0) MaterialGain.text += $"<color=red>{fac.Fac} {fac.Amount}</color>\n";
        }

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            if (RewardItemsGained.Length <= i)
            {
                InventorySlots[i].gameObject.SetActive(false);
            }
            else
            {
                InventorySlots[i].gameObject.SetActive(true);
                InventorySlots[i].SetInventoryItem(Resources.Load<ScriptableEquippable>(RewardItemsGained[i].ToString()));
            }
        }
    }
    public void CloseRewardScreen()
    {
        RewardActive = false;
        if (CO_STORY.co.IsCommsActive()) UI.ui.SelectScreen(UI.ui.TalkUI.gameObject);
        else UI.ui.SelectScreen(UI.ui.MapUI.gameObject);
    }
    public void CloseRewardScreenInventory()
    {
        RewardActive = false;
        UI.ui.SelectScreen(UI.ui.InventoryUI.gameObject);
    }
}
