using TMPro;
using UnityEngine;

public class UI_LoadGameButton : MonoBehaviour
{
    public TextMeshProUGUI LoadText;
    private dataStructure SaveGame;
    public dataStructure GetSaveGame()
    {
        return SaveGame;
    }
    public void Init(dataStructure sav)
    {
        SaveGame = sav;
        if (sav == null)
        {
            LoadText.text = "EMPTY";
            return;
        }
        LoadText.text = $"LOAD GAME {sav.saveTime.Day.ToString("00")}/{sav.saveTime.Month.ToString("00")}/{sav.saveTime.Year.ToString("0000")} \nAREA {sav.BiomeProgress} - {sav.BiomeName}";
    }
    public void SetSelected(bool bol)
    {
        LoadText.color = bol ? Color.green : Color.white;
    }
    public void PressLoad()
    {
        UI.ui.HostUI.PressLoadGame(this);
    }
}
