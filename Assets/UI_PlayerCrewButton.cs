using System.Linq;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerCrewButton : MonoBehaviour
{
    public UI_SubscreenPlayers Subscreen;
    public TextMeshProUGUI CrewText;
    public TextMeshProUGUI[] StatText;
    public Image CrewOuter;
    public Image CrewIcon;
    public Image CrewStripes;
    public Sprite DefaultSprite;
    private CREW Crew;
    public void SetCrew(CREW cre)
    {
        if (cre == null)
        {
            Crew = null;
            CrewText.text = "[JOINING]";
            CrewOuter.color = Color.clear;
            CrewIcon.sprite = DefaultSprite;
            CrewStripes.color = Color.clear;
            return;
        }
        Crew = cre;
        Color col = Crew.CharacterBackground.BackgroundColor;
        CrewOuter.color = new Color(col.r * 0.4f, col.g * 0.4f, col.b * 0.4f); //ScriptableEquippable.GetRarityColor(Crew.UnitRarity);
        CrewIcon.sprite = Crew.CharacterBackground.Sprite_Player;
        CrewStripes.sprite = Crew.CharacterBackground.Sprite_Stripes;
        CrewStripes.color = col;
        string hex = ColorUtility.ToHtmlStringRGB(Crew.GetCharacterColor());
        string str = "";
        str += $"<color=#{hex}>{Crew.CharacterName.Value}</color>";
        hex = ColorUtility.ToHtmlStringRGB(Crew.CharacterBackground.BackgroundColor);
        str += $"\n<color=#{hex}>{Crew.CharacterBackground.BackgroundName}</color>";
        int[] stats = new int[7];
        stats[0] = cre.GetATT_PHYSIQUE();
        stats[1] = cre.GetATT_ARMS();
        stats[2] = cre.GetATT_DEXTERITY();
        stats[3] = cre.GetATT_COMMUNOPATHY();
        stats[4] = cre.GetATT_COMMAND();
        stats[5] = cre.GetATT_ENGINEERING();
        stats[6] = cre.GetATT_ALCHEMY();
        stats[7] = cre.GetATT_MEDICAL();
        int HighestStatID = -1;
        int SecondHighestStatID = -1;
        int ThirdHighestStatID = -1;

        int HighestStatNumber = int.MinValue;
        int SecondHighestStatNumber = int.MinValue;
        int ThirdHighestStatNumber = int.MinValue;

        for (int i = 0; i < stats.Length; i++)
        {
            int value = stats[i];

            if (value > HighestStatNumber)
            {
                // shift down
                ThirdHighestStatNumber = SecondHighestStatNumber;
                ThirdHighestStatID = SecondHighestStatID;

                SecondHighestStatNumber = HighestStatNumber;
                SecondHighestStatID = HighestStatID;

                HighestStatNumber = value;
                HighestStatID = i;
            }
            else if (value > SecondHighestStatNumber)
            {
                ThirdHighestStatNumber = SecondHighestStatNumber;
                ThirdHighestStatID = SecondHighestStatID;

                SecondHighestStatNumber = value;
                SecondHighestStatID = i;
            }
            else if (value > ThirdHighestStatNumber)
            {
                ThirdHighestStatNumber = value;
                ThirdHighestStatID = i;
            }
        }
        str += " - ";
        if (HighestStatNumber > 3)
        {
            switch (HighestStatID)
            {
                case 0: str += "PHYS"; break;
                case 1: str += "ARM"; break;
                case 2: str += "DEX"; break;
                case 3: str += "COM"; break;
                case 4: str += "CMD"; break;
                case 5: str += "ENG"; break;
                case 6: str += "ALC"; break;
                case 7: str += "MED"; break;
            }

            str += $" {HighestStatNumber}";
        }
        if (SecondHighestStatNumber > 3)
        {
            str += " - ";
            switch (SecondHighestStatID)
            {
                case 0: str += "PHYS"; break;
                case 1: str += "ARM"; break;
                case 2: str += "DEX"; break;
                case 3: str += "COM"; break;
                case 4: str += "CMD"; break;
                case 5: str += "ENG"; break;
                case 6: str += "ALC"; break;
                case 7: str += "MED"; break;
            }

            str += $" {SecondHighestStatNumber}";
        }
        if (ThirdHighestStatNumber > 3)
        {
            str += " - ";
            switch (ThirdHighestStatID)
            {
                case 0: str += "PHYS"; break;
                case 1: str += "ARM"; break;
                case 2: str += "DEX"; break;
                case 3: str += "COM"; break;
                case 4: str += "CMD"; break;
                case 5: str += "ENG"; break;
                case 6: str += "ALC"; break;
                case 7: str += "MED"; break;
            }

            str += $" {ThirdHighestStatNumber}";
        }
        CrewText.text = str;

        LOCALCO local = CO.co.GetLOCALCO(Crew.GetPlayerController());
        if (local)
        {
            StatText[0].text = $"Crew damage: {local.Stats_CrewDamage.Value.ToString("0")}";
            StatText[1].text = $"Module damage: {local.Stats_ModuleDamage.Value.ToString("0")}";
            StatText[2].text = $"Healing: {local.Stats_Healed.Value.ToString("0")}";
            StatText[3].text = $"Repairs: {local.Stats_Repaired.Value.ToString("0")}";
            StatText[4].text = $"Damage taken/prevented: {local.Stats_DamageTaken.Value.ToString("0")}";
        }
    }
}
