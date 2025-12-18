using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CrewButton : MonoBehaviour
{
    public Image CrewOuter;
    public Image CrewIcon;
    public Image CrewStripes;
    public TextMeshProUGUI CrewName;
    public TooltipObject Tool;
    CREW Crew;
    public void SetCrew(CREW cre)
    {
        Crew = cre;
        Color col = Crew.CharacterBackground.BackgroundColor;
        CrewOuter.color = new Color(col.r * 0.4f, col.g * 0.4f, col.b*0.4f); //ScriptableEquippable.GetRarityColor(Crew.UnitRarity);
        CrewIcon.sprite = Crew.CharacterBackground.Sprite_Player;
        CrewStripes.sprite = Crew.CharacterBackground.Sprite_Stripes;
        CrewStripes.color = col;
        CrewName.text = $"{Crew.UnitName} \n{Crew.CharacterName.Value} ({Crew.GetUnitUpgradeLevel()})";
        CrewName.color = Crew.CharacterBackground.BackgroundColor;
        Tool.Tooltip = $"{Crew.UnitName} {Crew.CharacterName.Value} \n\n{Crew.UnitDescription}";
    }
    public void WhenPressed()
    {
        UI.ui.InventoryUI.PressCrewButton(Crew);
    }
}
