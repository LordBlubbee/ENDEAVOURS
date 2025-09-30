using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_Inventory : MonoBehaviour
{
    public GameObject[] Subscreens;
    public GameObject SubscreenEquipment;
    public GameObject SubscreenInventory;
    public GameObject SubscreenAttributes;
    public InventorySlot[] InventoryWeapons;
    public InventorySlot InventoryArmor;
    public InventorySlot[] InventoryArtifacts;
    public InventorySlot[] InventoryCargo;

    private void OnEnable()
    {
        SkillRefresh();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
        }
    }

    [Header("Skills")]
    public TextMeshProUGUI SkillPointTex;
    private int[] SkillPower = new int[8];
    public string[] SkillName;
    public TextMeshProUGUI[] SkillTex;
    public Image[] SkillLevelButton;
    public TextMeshProUGUI[] SkillLevelTex;
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
        if (LOCALCO.local.GetPlayer().SkillPoints.Value < LevelNeed) return;
        LOCALCO.local.GetPlayer().IncreaseATTRpc(ID);
        //LOCALCO.local.PlayerSkillPoints.Value -= LevelNeed;
        //SkillPower[ID]++;
    }
    public void SkillRefresh()
    {
        SkillPointTex.text = $"SKILL POINTS: ({LOCALCO.local.GetPlayer().SkillPoints.Value})";
        for (int i = 0; i < SkillPower.Length; i++)
        {
            if (LOCALCO.local.GetPlayer().CharacterBackground)
            {
                if (LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i] > 0)
                {
                    SkillTex[i].text = $"{SkillName[i]} <color=green>({SkillPower[i] + LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i]})";
                }
                else if (LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i] < 0)
                {
                    SkillTex[i].text = $"{SkillName[i]} <color=red> ({SkillPower[i] + LOCALCO.local.GetPlayer().CharacterBackground.Background_ATT_BONUS[i]})";
                }
                else
                {
                    SkillTex[i].text = $"{SkillName[i]} ({SkillPower[i]})";
                }
            }
            else
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
                    SkillTex[i].color = new Color(1, 0.5f, 0);
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
            if (LevelNeed > LOCALCO.local.GetPlayer().SkillPoints.Value) SkillLevelButton[i].gameObject.SetActive(false);
            else
            {
                SkillLevelButton[i].gameObject.SetActive(true);
                SkillLevelTex[i].text = $"LEVEL ({LevelNeed})";
                if (LevelNeed > 1) SkillLevelTex[i].color = Color.yellow;
                else SkillLevelTex[i].color = Color.green;
            }
        }
    }
    public void OpenSubscreen(GameObject ob)
    {
        foreach (GameObject sub in Subscreens)
        {
            sub.SetActive(false);
        }
        ob.SetActive(true);
        if (ob == SubscreenEquipment) SubscreenInventory.SetActive(true);
    }
}
