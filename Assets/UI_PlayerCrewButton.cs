using System.Linq;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerCrewButton : MonoBehaviour
{
    public UI_SubscreenPlayers Subscreen;
    public TextMeshProUGUI CrewText;
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
        int[] stats = Crew.GetAttributes();
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
        str += "\n";
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

            str += $": {HighestStatNumber}";
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

            str += $": {SecondHighestStatNumber}";
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

            str += $": {ThirdHighestStatNumber}";
        }
        CrewText.text = str;
    }
}
