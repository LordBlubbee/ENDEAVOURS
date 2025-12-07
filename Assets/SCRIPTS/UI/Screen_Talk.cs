using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
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
    public GameObject OverrideVoteButton;
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
            SpeakerFader.color = new Color(1, 1, 1, SpeakerFader.color.a - Time.deltaTime * 4f);
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
        string StoryID = CO_STORY.co.GetMainStoryText(0);
        if (PreviousTex != StoryID)
        {
            PreviousTex = StoryID;
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
        OverrideVoteButton.gameObject.SetActive(LOCALCO.local.CurrentDialogVote.Value > -1 && CO.co.IsHost);
    }

    public void PressOverrideButton()
    {
        CO_STORY.co.OverrideTalkResult = true;
    }

    string PreviousTex = "";
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
            //StartCoroutine(SetText(MainTex,"",Color.white, false));
            Delay = 0.4f;
        }
        SpeakerImage.sprite = CurrentSpeaker.Portrait;
        if (gameObject.activeSelf) StartCoroutine(SetMainText(MainTex, curText, CurrentSpeaker.NameColor, true, Delay));
        else MainTex.text = curText;
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
                StartCoroutine(SetChoiceText(ChoiceTex[i], str, Color.white, false, 0.5f));
                ChoiceButtonVotes[i].text = CO_STORY.co.VoteResultAmount(i).ToString();
                ChoiceButton[i].gameObject.SetActive(str != "");
            }
        }
    }

    bool isSpeaking = false;
    bool completeSpeakingNow = false;
    int speakID = 0;
    int speakLetter = 0;
    IEnumerator SetMainText(TextMeshProUGUI tex, string text, Color col, bool Speak, float delay = 0)
    {
        isSpeaking = true;
        speakID++;
        int ID = speakID;

        tex.color = col;
        tex.text = text;
        tex.maxVisibleCharacters = 0;

        float Timer = 0f;
        yield return new WaitForSeconds(delay);

        int totalChars = text.Length;
        int visibleCount = 0;
        speakLetter = 0;

        while (visibleCount < totalChars)
        {
            while (Timer > 0f && !completeSpeakingNow)
            {
                Timer -= Time.deltaTime;
                yield return null;
            }

            if (ID != speakID) yield break; // text was replaced

            Timer += 0.015f;
            visibleCount++;
            if (completeSpeakingNow)
            {
                visibleCount = totalChars;
                completeSpeakingNow = false;
            }
            tex.maxVisibleCharacters = visibleCount;

            if (CurrentSpeaker && Speak)
            {
                char c = text[Mathf.Clamp(visibleCount - 1, 0, totalChars - 1)];
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
        }
        isSpeaking = false;
    }
    IEnumerator SetChoiceText(TextMeshProUGUI tex, string text, Color col, bool Speak, float delay = 0)
    {
        tex.color = col;
        tex.text = text;
        tex.maxVisibleCharacters = 0;

        float Timer = 0f;
        yield return new WaitForSeconds(delay);

        int totalChars = text.Length;
        int visibleCount = 0;
        speakLetter = 0;

        while (visibleCount < totalChars)
        {
            while (Timer > 0f)
            {
                Timer -= Time.deltaTime;
                yield return null;
            }
            Timer += 0.015f;
            visibleCount++;
            tex.maxVisibleCharacters = visibleCount;
        }
    }
    /*IEnumerator SetMainText(TextMeshProUGUI tex, string text, Color col, bool Speak, float delay = 0)
    {
        speakID++;
        int ID = speakID;
        string keepTex = "";
        tex.text = keepTex;
        float Timer = 0f;
        yield return new WaitForSeconds(delay);
        foreach (char c in text)
        {
            while (Timer > 0f)
            {
                Timer -= Time.deltaTime;
                yield return null;
            }
            if (ID != speakID) yield break;
            Timer += 0.02f;
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
        }
    }*/
    public void NextPage()
    {
        if (isSpeaking)
        {
            completeSpeakingNow = true;
            return;
        }
        if (CO_STORY.co.IsLastMainStoryText(CurrentPage)) return;
        CurrentPage++;
        UpdateData();
    }
    public void PreviousPage() {
        if (CurrentPage < 1) return;
        CurrentPage--; UpdateData(); 
    }
    public void CloseScreen()
    {
        UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
    }
}
