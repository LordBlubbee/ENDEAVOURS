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
    public TextMeshProUGUI NextButtonTex;
    public TextMeshProUGUI PreviousButtonTex;
    public TextMeshProUGUI[] ChoiceButtonVotes;
    public TextMeshProUGUI[] ChoiceTex;
    public Image SpeakerImage;
    public Image SpeakerFader;
    private int CurrentPage = 0;
    private ScriptableDialogSpeaker CurrentSpeaker;
    private void OnEnable()
    {
        UpdateData();
    }

    IEnumerator FadeNewSpeakerImage()
    {
        SpeakerFader.color = Color.white;
        while (SpeakerFader.color.a > 0)
        {
            SpeakerFader.color = new Color(1, 1, 1, SpeakerFader.color.a - CO.co.GetWorldSpeedDelta() * 4f);
            yield return null;
        }
        SpeakerFader.color = Color.clear;
    }
    private void Update()
    {
        if (!CO_STORY.co.CommsActive.Value)
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
            return;
        }
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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.U))
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
        }
    }
    private void UpdateData()
    {
        string curText = CO_STORY.co.GetMainStoryText(CurrentPage);
        if (curText == "")
        {
            CurrentPage = 0;
            curText = CO_STORY.co.GetMainStoryText(CurrentPage);
        }
        CurrentSpeaker = CO_STORY.co.GetSpeaker(CurrentPage);
        float Delay = 0;
        if (SpeakerImage.sprite != CurrentSpeaker.Portrait || CurrentPage == 0)
        {
            StartCoroutine(FadeNewSpeakerImage());
            StartCoroutine(SetText(MainTex,"",Color.white, false));
            Delay = 0.6f;
        }
        SpeakerImage.sprite = CurrentSpeaker.Portrait;
        StartCoroutine(SetText(MainTex, curText, CurrentSpeaker.NameColor, true, Delay));
        if (!CO_STORY.co.IsLastMainStoryText(CurrentPage))
        {
            for (int i = 0; i < ChoiceTex.Length; i++)
            {
                ChoiceTex[i].text = "";
                ChoiceButtonVotes[i].text = "";
                ChoiceButton[i].gameObject.SetActive(false);
            }
            NextButtonTex.color = Color.white;
            PreviousButtonTex.color = (CurrentPage > 0) ? Color.white : Color.gray;
        }
        else
        {
            NextButtonTex.color = Color.gray;
            PreviousButtonTex.color = Color.white;
            for (int i = 0; i < ChoiceTex.Length; i++)
            {
                string str = CO_STORY.co.GetCurrentChoice(i);
                StartCoroutine(SetText(ChoiceTex[i], str, Color.cyan, false, 0.5f));
                ChoiceButtonVotes[i].text = CO_STORY.co.VoteResultAmount(i).ToString();
                ChoiceButton[i].gameObject.SetActive(str != "");
            }
        }
    }

    int speakLetter = 0;
    IEnumerator SetText(TextMeshProUGUI tex, string text, Color col, bool Speak, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        string keepTex = "";
        tex.text = keepTex;
        foreach (char c in text)
        {
            if (tex.text != keepTex) yield break;
            keepTex += c;
            tex.text = keepTex;
            if (CurrentSpeaker && Speak)
            {
                if (CurrentSpeaker.Voice.Length > 0 && char.IsLetterOrDigit(c))
                {
                    speakLetter--;
                    if (speakLetter < 0)
                    {
                        speakLetter = Random.Range(2, 4);
                        AudioClip clip = CurrentSpeaker.Voice[Random.Range(0, CurrentSpeaker.Voice.Length)];
                        AUDCO.aud.PlaySFX(clip);
                    }
                }
            }
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
    public void OpenRewardScreen(int Materials, int Supplies, int Ammo, int Tech, int XP, FactionReputation[] Facs, FixedString64Bytes[] RewardItemsGained)
    {
        RewardScreen.SetActive(true);
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
