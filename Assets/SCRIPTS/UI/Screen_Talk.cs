using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Screen_Talk : MonoBehaviour
{
    public TextMeshProUGUI MainTex;
    public GameObject[] ChoiceButton;
    public GameObject NextButton;
    public GameObject PreviousButton;
    public TextMeshProUGUI[] ChoiceButtonVotes;
    public TextMeshProUGUI[] ChoiceTex;
    public Image SpeakerImage;
    private int CurrentPage = 0;
    private void OnEnable()
    {
        UpdateData();
    }
    private void Update()
    {
        if (CO_STORY.co.ShouldUpdate)
        {
            CO_STORY.co.ShouldUpdate = false; 
            CurrentPage = 0;
            UpdateData();
        }
        for (int i = 0; i < ChoiceTex.Length; i++)
        {
            if (ChoiceButton[i].gameObject.activeSelf)
            {
                int Votes = CO_STORY.co.VoteResultAmount(i);
                ChoiceButtonVotes[i].text = Votes.ToString();
                ChoiceButtonVotes[i].color = (LOCALCO.local.CurrentDialogVote.Value == i) ? Color.cyan : Color.white;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
        }
    }
    private void UpdateData()
    {
        if (!CO_STORY.co.CommsActive.Value)
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
            return;
        }
        string curText = CO_STORY.co.GetMainStoryText(CurrentPage);
        if (curText == "")
        {
            CurrentPage = 0;
            curText = CO_STORY.co.GetMainStoryText(CurrentPage);
        }
        StartCoroutine(SetText(MainTex, curText));
        if (!CO_STORY.co.IsLastMainStoryText(CurrentPage))
        {
            for (int i = 0; i < ChoiceTex.Length; i++)
            {
                ChoiceTex[i].text = "";
                ChoiceButtonVotes[i].text = "";
                ChoiceButton[i].gameObject.SetActive(false);
            }
            NextButton.SetActive(true);
            PreviousButton.SetActive(CurrentPage > 0);
        }
        else
        {
            NextButton.SetActive(false);
            PreviousButton.SetActive(true);
            for (int i = 0; i < ChoiceTex.Length; i++)
            {
                string str = CO_STORY.co.GetCurrentChoice(i);
                StartCoroutine(SetText(ChoiceTex[i], str , 1f));
                ChoiceButtonVotes[i].text = CO_STORY.co.VoteResultAmount(i).ToString();
                ChoiceButton[i].gameObject.SetActive(str != "");
            }
        }
    }

    IEnumerator SetText(TextMeshProUGUI tex, string text, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        string keepTex = "";
        tex.text = keepTex;
        foreach (char c in text)
        {
            if (tex.text != keepTex) yield break;
            keepTex += c;
            tex.text = keepTex;
            yield return new WaitForSeconds(0.03f);
        }
    }
    public void NextPage()
    {
        if (CO_STORY.co.IsLastMainStoryText(CurrentPage)) return;
        CurrentPage++;
        UpdateData();
    }
    public void PreviousPage() {
        if (CurrentPage < 1) return;
        CurrentPage--; UpdateData(); 
    }

    [Header("REWARD SCREEN")]
    public GameObject RewardScreen;
    public TextMeshProUGUI MaterialGain;
    public List<InventorySlot> InventorySlots;
    public void OpenRewardScreen(int Materials, int Supplies, int Ammo, int Tech, FactionReputation[] Facs, FixedString64Bytes[] RewardItemsGained)
    {
        RewardScreen.SetActive(true);
        MaterialGain.text = "";
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
            } else
            {
                InventorySlots[i].gameObject.SetActive(true);
                InventorySlots[i].SetInventoryItem(Resources.Load<ScriptableEquippable>(RewardItemsGained[i].ToString()));
            }
        }
    }
    public void CloseRewardScreen()
    {
        RewardScreen.SetActive(false);
    }
    public void CloseScreen()
    {
        RewardScreen.SetActive(false);
        UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
    }
}
