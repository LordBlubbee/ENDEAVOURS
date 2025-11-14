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
        CrewOuter.color = ScriptableEquippable.GetRarityColor(Crew.UnitRarity);
        CrewIcon.sprite = Crew.CharacterBackground.Sprite_Player;
        CrewStripes.sprite = Crew.CharacterBackground.Sprite_Stripes;
        CrewStripes.color = Crew.CharacterBackground.BackgroundColor;
        CrewName.text = $"{Crew.UnitName} \n{Crew.CharacterName.Value}";
        CrewName.color = Crew.CharacterBackground.BackgroundColor;
        Tool.Tooltip = $"{Crew.UnitName} {Crew.CharacterName.Value} \n\n{Crew.UnitDescription}";
    }
    public void WhenPressed()
    {
        UI.ui.InventoryUI.PressCrewButton(Crew);
    }
}
