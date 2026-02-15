using TMPro;
using UnityEngine;

public class Screen_Host : MonoBehaviour
{
    public TextMeshProUGUI DifficultyTex;
    public TooltipObject DifficultyTooltip;
    public TextMeshProUGUI HostControlTex;
    public TooltipObject HostControlTooltip;
    public GameObject LobbyNameTagObject;
    public GameObject HostControlObject;
    public bool AloneGame = false;

    public UI_LoadGameButton[] LoadGameButtons;
    private void OnEnable()
    {
        SelectedLoadedGame = LoadGameButtons[GO.g.currentSaveSlot];
        RefreshPlayerGameDifficulty(); RefreshHostControl(); RefreshLoadedGames();
        LOBBY.lobby.HostNameInput.text = GO.g.preferredLobbyName;

        LobbyNameTagObject.SetActive(!AloneGame);
        HostControlObject.SetActive(!AloneGame);
    }

    public void PressPlay()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        if (AloneGame)
        {
            LOBBY.lobby.PressPlayAlone();
            return;
        }
        LOBBY.lobby.StartHost();
    }
    public void RefreshLoadedGames()
    {
        bool HasShownEmptyGame = false;
        for (int i = 0; i < LoadGameButtons.Length; i++)
        {
            dataStructure sav = GO.g.loadSlot(i); 
            if (sav == null)
            {
                if (HasShownEmptyGame)
                {
                    LoadGameButtons[i].gameObject.SetActive(false);
                    continue;
                }
                HasShownEmptyGame = true;
            }
            LoadGameButtons[i].gameObject.SetActive(true);
            LoadGameButtons[i].Init(sav);
            LoadGameButtons[i].SetSelected(LoadGameButtons[i] == SelectedLoadedGame);
          
        }
    }
    public void PressDeleteGame()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Salvage);
        GO.g.deleteGame(GO.g.currentSaveSlot);
        RefreshLoadedGames();
    }

    private UI_LoadGameButton SelectedLoadedGame;
    public UI_LoadGameButton GetSelectedLoadedGame()
    {
        return SelectedLoadedGame;
    }
    public void PressLoadGame(UI_LoadGameButton but)
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        SelectedLoadedGame = but;
        RefreshLoadedGames();
        for (int i = 0; i < LoadGameButtons.Length; i++)
        {
            if (LoadGameButtons[i] == but)
            {
                GO.g.currentSaveSlot = i;
                GO.g.saveSettings();
                return;
            }
        }
    }
    public void PressDeleteGame(UI_LoadGameButton but)
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Salvage);
     
        for (int i = 0; i < LoadGameButtons.Length; i++)
        {
            if (LoadGameButtons[i] == but)
            {
                GO.g.deleteGame(i);
                GO.g.saveSettings();
                RefreshLoadedGames();
                return;
            }
        }
    }
    public void PressPlayerGameDifficulty()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        GO.g.preferredGameDifficulty++;
        if (GO.g.preferredGameDifficulty > 4) GO.g.preferredGameDifficulty = 0;
        GO.g.saveSettings();
        RefreshPlayerGameDifficulty();

    }
    public void RefreshPlayerGameDifficulty()
    {
        switch (GO.g.preferredGameDifficulty)
        {
            case 0:
                DifficultyTex.text = "ADVANTAGED";
                DifficultyTex.color = Color.green;
                DifficultyTooltip.Tooltip = "Difficulty is slightly reduced, though the Endeavour is still challenging. \n\nEnemy Drifter strength -15% \nEnemy crew- and creature strength -20% \nEnemy crew- and creature count -10%";
                break;
            case 1:
                DifficultyTex.text = "MEDIUM";
                DifficultyTex.color = Color.yellow;
                DifficultyTooltip.Tooltip = "The base Endeavour experience is a challenge to complete. Prepare yourselves.";
                break;
            case 2:
                DifficultyTex.text = "HARD";
                DifficultyTex.color = new Color(1,0.6f,0.1f);
                DifficultyTooltip.Tooltip = "The Endeavour was launched late, and your enemies have become stronger. Very challenging. \n\nEnemy Drifter strength +15% \nEnemy crew- and creature strength +20% \nEnemy crew- and creature count +10% \nLoot -10%";
                break;
            case 3:
                DifficultyTex.text = "INSANE";
                DifficultyTex.color = Color.red;
                DifficultyTooltip.Tooltip = "All odds seem to be stacked against you. Extremely challenging. \n\nEnemy Drifter strength +25% \nEnemy crew- and creature strength +40% \nEnemy crew- and creature count +15% \nLoot -10%";
                break;
            case 4:
                DifficultyTex.text = "IMPOSSIBLE";
                DifficultyTex.color = new Color(0.5f,0,0);
                DifficultyTooltip.Tooltip = "Everything seems lost. It is unknown whether the Endeavour can be completed this way. \n\nEnemy Drifter strength +40% \nEnemy crew- and creature strength +80% \nEnemy crew- and creature count +25% \nLoot -20%";
                break;
        }
    }
    public void PressHostControl()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        GO.g.preferredHostControl++;
        if (GO.g.preferredHostControl > 1) GO.g.preferredHostControl = 0;
        GO.g.saveSettings();
        RefreshHostControl();

    }
    public void RefreshHostControl()
    {
        switch (GO.g.preferredHostControl)
        {
            case 0:
                HostControlTex.text = "NONE";
                HostControlTex.color = Color.green;
                HostControlTooltip.Tooltip = "All players can purchase items or modify the Drifter. Destinations and dialogue options are voted for until all are in agreement.";
                break;
            case 1:
                HostControlTex.text = "BUYING";
                HostControlTex.color = Color.yellow;
                HostControlTooltip.Tooltip = "Only the host may purchase or craft items, upgrade modules, and promote crew.";
                break;
        }
    }
    public void OnEditHostInput()
    {
        GO.g.preferredLobbyName = LOBBY.lobby.HostNameInput.text;
        GO.g.saveSettings();
    }
}
