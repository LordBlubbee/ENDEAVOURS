using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Screen_CharacterCreator : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TextMeshProUGUI UsernameTex;
    public Slider UsernameR;
    public Slider UsernameG;
    public Slider UsernameB;

    public GameObject[] Subscreens;
    public TextMeshProUGUI CompleteGivePoints;
    public TextMeshProUGUI CompleteBackground;
    public TextMeshProUGUI CompleteName;
    public Image[] BackgroundCategories;
    public Image[] AllBackgrounds;

    private int SkillPoints = 15;
    public TextMeshProUGUI SkillPointTex;
    private int[] SkillPower = new int[8];
    public string[] SkillName;
    public TextMeshProUGUI[] SkillTex;
    public Image[] SkillLevelButton;
    public TextMeshProUGUI[] SkillLevelTex;
    private ScriptableBackground SelectedBackground = null;
    public void LevelSkill(int ID)
    {
        int LevelNeed;
        switch (SkillPower[ID])
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
                LevelNeed = 3;
                break;
            case 5:
                LevelNeed = 4;
                break;
            case 6:
                LevelNeed = 5;
                break;
            case 7:
                LevelNeed = 6;
                break;
            case 8:
                LevelNeed = 7;
                break;
            case 9:
                LevelNeed = 8;
                break;
            default:
                LevelNeed = 999;
                break;
        }
        if (SkillPoints < LevelNeed) return;
        SkillPoints -= LevelNeed;
        SkillPower[ID]++;
        SkillRefresh();
    }
    private void SkillRefresh()
    {
        SkillPointTex.text = $"SKILL POINTS: ({SkillPoints})";
        for (int i = 0; i < SkillPower.Length; i++)
        {
            if (SelectedBackground)
            {
                if (SelectedBackground.Background_ATT_BONUS[i] > 0)
                {
                    SkillTex[i].text = $"{SkillName[i]} <color=green>({SkillPower[i]+ SelectedBackground.Background_ATT_BONUS[i]})";
                } else if (SelectedBackground.Background_ATT_BONUS[i] < 0)
                {
                    SkillTex[i].text = $"{SkillName[i]} <color=red> ({SkillPower[i]+ SelectedBackground.Background_ATT_BONUS[i]})";
                } else
                {
                    SkillTex[i].text = $"{SkillName[i]} ({SkillPower[i]})";
                }
            } else
            {
                SkillTex[i].text = $"{SkillName[i]} ({SkillPower[i]})";
            }
                int LevelNeed;
            switch (SkillPower[i])
            {
                case 0:
                    LevelNeed = 1;
                    SkillTex[i].color = new Color(1, 0, 0);
                    break;
                case 1:
                    LevelNeed = 1;
                    SkillTex[i].color = new Color(1,0.5f,0);
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
                    LevelNeed = 3;
                    SkillTex[i].color = new Color(0.6f, 0.9f, 0);
                    break;
                case 5:
                    LevelNeed = 4;
                    SkillTex[i].color = new Color(0.4f, 0.9f, 0);
                    break;
                case 6:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0.2f, 0.9f, 0);
                    break;
                case 7:
                    LevelNeed = 6;
                    SkillTex[i].color = new Color(0, 1, 0);
                    break;
                case 8:
                    LevelNeed = 7;
                    SkillTex[i].color = new Color(0, 1, 0.5f);
                    break;
                case 9:
                    LevelNeed = 8;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
                default:
                    LevelNeed = 999;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
            }
            if (LevelNeed > SkillPoints) SkillLevelButton[i].gameObject.SetActive(false);
            else
            {
                SkillLevelButton[i].gameObject.SetActive(true);
                SkillLevelTex[i].text = $"LEVEL ({LevelNeed})";
                if (LevelNeed > 1) SkillLevelTex[i].color = Color.yellow;
                else SkillLevelTex[i].color = Color.green;
            }
        }
    }
    private void Start()
    {
        UsernameTex.text = GO.g.localUsername;
        UsernameTex.color = GO.g.localColor;
        UsernameR.value = GO.g.localColor.r;
        UsernameG.value = GO.g.localColor.g;
        UsernameB.value = GO.g.localColor.b;
        for (int i = 0; i < SkillPower.Length; i++)
        {
            SkillPower[i] = 1;
        }
        SkillRefresh();
        UpdateCompletionStatus();
    }

    private void Update()
    {
        UpdateCompletionStatus();
    }

    private void UpdateCompletionStatus()
    {
        CompleteName.color = !UsernameTex.text.IsNullOrEmpty() ? Color.green : Color.red;
        CompleteBackground.color = SelectedBackground ? Color.green : Color.red;
        CompleteGivePoints.color = SkillPoints < 1 ? Color.green : Color.red;
    }

    private bool ReadyToMoveOn()
    {
        if (UsernameTex.text.IsNullOrEmpty())
        {
            return false;
        }
        if (!SelectedBackground)
        {
            return false;
        }
        if (SkillPoints > 0)
        {
            return false;
        }
        return true;
    }
    public void EditUsername()
    {
        GO.g.localUsername = UsernameInput.text;
        UsernameTex.text = GO.g.localUsername;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideR()
    {
        GO.g.localColor = new Color(UsernameR.value, GO.g.localColor.g, GO.g.localColor.b);
        UsernameTex.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideG()
    {
        GO.g.localColor = new Color(GO.g.localColor.r, UsernameG.value, GO.g.localColor.b);
        UsernameTex.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideB()
    {
        GO.g.localColor = new Color(GO.g.localColor.r, GO.g.localColor.g, UsernameB.value);
        UsernameTex.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void PressCreateCharacter()
    {
        if (!SelectedBackground) return;
        Debug.Log($"{SelectedBackground.ResourcePath}");
        LOCALCO.local.CreatePlayerRpc(GO.g.localUsername, GO.g.localColor, SkillPower, SelectedBackground.ResourcePath);
        UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
    }
    public void OpenSubscreen(GameObject ob)
    {
        foreach (GameObject sub in Subscreens)
        {
            sub.SetActive(false);
        }
        ob.SetActive(true);
    }
    public void ChooseBackgroundFaction(Image ob)
    {
        foreach (Image sub in BackgroundCategories)
        {
            sub.color = Color.gray;
        }
        ob.color = Color.cyan;
    }
    public void ChooseBackgroundExact(Image ob)
    {
        foreach (Image sub in AllBackgrounds)
        {
            sub.color = Color.gray;
        }
        ob.color = Color.cyan;
    }
    public void ChooseBackground(ScriptableBackground back)
    {
        SelectedBackground = back;
        SkillRefresh();
    }
}
