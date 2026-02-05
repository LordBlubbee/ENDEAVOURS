using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoadGameButton : MonoBehaviour
{
    public TextMeshProUGUI LoadText;
    private dataStructure SaveGame;
    public Image DeleteButton;
    public dataStructure GetSaveGame()
    {
        return SaveGame;
    }
    public void Init(dataStructure sav)
    {
        SaveGame = sav;
        if (sav == null)
        {
            DeleteButton.gameObject.SetActive(false);
            LoadText.text = "EMPTY";
            return;
        }
        DeleteButton.color = new Color(0.5f, 0, 0);
        DeleteButton.gameObject.SetActive(true);
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

    private bool PressDelete = false;
    private float DeleteTime = 0f;
    public void PressDeleteButton()
    {
        if (!PressDelete)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Press);
            PressDelete = true;
            DeleteButton.color = new Color(1, 0, 0);
            DeleteTime = 1f;
            return;
        }
        UI.ui.HostUI.PressDeleteGame(this);
    }
    private void Update()
    {
        if (PressDelete)
        {
            DeleteTime -= Time.deltaTime;
            if (DeleteTime <= 0f)
            {
                PressDelete = false;
                DeleteButton.color = new Color(0.5f, 0, 0);
            }
        }
    }
}
