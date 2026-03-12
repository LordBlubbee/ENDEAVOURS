using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MasteryTree : MonoBehaviour
{
    public int GlobalID;
    public Image ScreenOutline;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Tagline;
    public Image Icon1;
    public Image Icon2;
    public UI_Mastery[] Masteries;
    ScriptableMasteryTree MasteryTree;
    public void InitMastery(ScriptableMasteryTree Tree)
    {
        MasteryTree = Tree;
        Title.text = Tree.ItemName;
        Title.color = Tree.ThemeColor;
        ScreenOutline.color = Tree.ThemeColor;
        Tagline.text = Tree.ItemTagline;
        Icon1.sprite = Tree.ItemIcon;
        Icon2.sprite = Tree.ItemIcon;
        for (int i = 0; i < Masteries.Length; i++)
        {
            Masteries[i].SetItem(Tree.MasteryItems[i],i + GlobalID * 10, Tree.ThemeColor);
        }
        SetUnlockStatus(LOCALCO.local.GetPlayer().UnlockedMasteries);
    }
    public void SetUnlockStatus(List<int> GlobalIDs)
    {
        for (int i = 0; i < Masteries.Length; i++)
        {
            if (GlobalIDs.Contains(i + GlobalID * 10))
            {
                Masteries[i].SetState(UI_Mastery.MasteryStates.UNLOCKED);
            }
            else if (HasMasteryBeenUnlocked(i,GlobalIDs))
            {
                if (LOCALCO.local.GetPlayer().MasteryPoints.Value > 0)
                    Masteries[i].SetState(UI_Mastery.MasteryStates.RESEARCHABLE);
                else
                    Masteries[i].SetState(UI_Mastery.MasteryStates.CONNECTED);
            }
            else
            {
                Masteries[i].SetState(UI_Mastery.MasteryStates.LOCKED);
            }
        }
    }

    private bool HasMasteryBeenUnlocked(int ID, List<int> GlobalIDs)
    {
        if (Masteries[ID].RequireUnlock == -1)
        {
            return true;
        }
        return GlobalIDs.Contains(Masteries[ID].RequireUnlock + GlobalID * 10);
    }
}
