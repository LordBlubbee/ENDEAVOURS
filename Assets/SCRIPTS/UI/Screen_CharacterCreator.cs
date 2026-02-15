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
    public GameObject PlayerListButton;

    public Image AppearanceSprite;
    public Image AppearanceStripes;

    private ScriptableBackground SelectedBackground = null;
    private int SkillPoints = 12;
    public TextMeshProUGUI SkillPointTex;
    private int[] SkillPower = new int[8];
    public string[] SkillName;
    public TextMeshProUGUI[] SkillTex;
    public Image[] SkillLevelButton;
    public TextMeshProUGUI[] SkillLevelTex;

    [Header("Load Character")]
    public GameObject LoadCharacterScreen;
    public TextMeshProUGUI LoadCharacterTitle;
    public Image LoadCharacterIcon;
    public Image LoadCharacterStripes;

    public void AddLoadedCharacter(string Name, Color Col, ScriptableBackground Back)
    {
        //Load from LOCALCO
        LoadCharacterScreen.SetActive(true);
        LoadCharacterTitle.text = Name;
        LoadCharacterTitle.color = Col;
        LoadCharacterIcon.sprite = Back.Sprite_Player;
        LoadCharacterStripes.sprite = Back.Sprite_Stripes;
        LoadCharacterStripes.color = Col;
    }
    public void PressLoadCharacter()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        LoadCharacterScreen.SetActive(false);
        UI.ui.TutorialManager.OpenTutorial();
        LOCALCO.local.LoadPlayerRpc();
        UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
    }

    public void CancelLoadCharacter()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        LoadCharacterScreen.SetActive(false);
    }
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
                LevelNeed = 2;
                break;
            case 5:
                LevelNeed = 3;
                break;
            case 6:
                LevelNeed = 3;
                break;
            case 7:
                LevelNeed = 4;
                break;
            default:
                LevelNeed = 5;
                break;
        }
        if (SkillPoints < LevelNeed)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
        SkillPoints -= LevelNeed;
        SkillPower[ID]++;
        SkillRefresh();
    }
    public void ResetSkills()
    {
        SkillPoints = 12;
        for (int i = 0; i < SkillPower.Length; i++)
        {
            SkillPower[i] = 1;
        }
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
                    LevelNeed = 2;
                    SkillTex[i].color = new Color(0.6f, 0.9f, 0);
                    break;
                case 5:
                    LevelNeed = 3;
                    SkillTex[i].color = new Color(0.4f, 0.9f, 0);
                    break;
                case 6:
                    LevelNeed = 3;
                    SkillTex[i].color = new Color(0.2f, 0.9f, 0);
                    break;
                case 7:
                    LevelNeed = 4;
                    SkillTex[i].color = new Color(0, 1, 0);
                    break;
                case 8:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0, 1, 0.5f);
                    break;
                case 9:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
                default:
                    LevelNeed = 5;
                    SkillTex[i].color = new Color(0, 1, 1);
                    break;
            }
            if (LevelNeed > SkillPoints)
            {
                SkillLevelButton[i].gameObject.SetActive(true);
                SkillLevelTex[i].text = $"LEVEL ({LevelNeed})";
                SkillLevelTex[i].color = Color.gray;
            }
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
        PlayerListButton.gameObject.SetActive(CO.co.GetLOCALCO().Count > 1);
    }

    private void UpdateCompletionStatus()
    {
        CompleteBackground.color = SelectedBackground ? Color.green : Color.red;
        CompleteName.color = SelectedBackground ? (!UsernameTex.text.IsNullOrEmpty() ? Color.green : Color.red) : Color.gray;
        CompleteGivePoints.color = SkillPoints < 1 ? Color.green : Color.red;
    }

    public void OpenAppearance()
    {
        if (SelectedBackground)
        {
            OpenSubscreen(Subscreens[1]);
        }
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
        AppearanceStripes.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideG()
    {
        GO.g.localColor = new Color(GO.g.localColor.r, UsernameG.value, GO.g.localColor.b);
        UsernameTex.color = GO.g.localColor;
        AppearanceStripes.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideB()
    {
        GO.g.localColor = new Color(GO.g.localColor.r, GO.g.localColor.g, UsernameB.value);
        UsernameTex.color = GO.g.localColor;
        AppearanceStripes.color = GO.g.localColor;
        GO.g.saveSettings();
    }

    public bool Test_Skip;
    public void PressCreateCharacter()
    {
        if (Test_Skip)
        {
            UI.ui.TutorialManager.OpenTutorial();
            LOCALCO.local.CreatePlayerRpc("Test Man", new Color(1,1,1), new int[] {5,5,5,5,5,5,5,5}, "Invictus_Marine");
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
            return;
        }
        if (!ReadyToMoveOn())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (!SelectedBackground)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        UI.ui.TutorialManager.OpenTutorial();
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

    public GameObject SelectedBackgroundScreen;
    public Image SelectedBackgroundScreenImage;
    public Image SelectedBackgroundScreenImageOuter;
    public TextMeshProUGUI SelectedBackgroundScreenTitle;
    public TextMeshProUGUI SelectedBackgroundScreenDesc;
    UI_OpenBackground CurrentlySelectedBackground;
    public void OpenExactBackground(UI_OpenBackground back)
    {
        CurrentlySelectedBackground = back;
        SelectedBackgroundScreenImage.sprite = back.Background.MainIcon;
        SelectedBackgroundScreenImageOuter.color = back.Background.BackgroundColor;
        SelectedBackgroundScreenTitle.text = back.Background.BackgroundName;
        SelectedBackgroundScreenDesc.text = back.Background.LongDesc;

        AppearanceSprite.sprite = back.Background.Sprite_Player;
        AppearanceStripes.sprite = back.Background.Sprite_Stripes;
        OpenSubscreen(SelectedBackgroundScreen);
    }
    public void ChooseBackgroundExact(Image ob)
    {
        //Handles texture
        foreach (Image sub in AllBackgrounds)
        {
            sub.color = Color.gray;
        }
        ob.color = Color.cyan;
    }
    public void PressChooseBackground()
    {
        //Handles background chosen
        SelectedBackground = CurrentlySelectedBackground.Background;
        ChooseBackgroundExact(CurrentlySelectedBackground.Border);
        OpenSubscreen(Subscreens[0]); //Background selected
        SkillRefresh();
    }
}
