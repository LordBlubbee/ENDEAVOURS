using TMPro;
using UnityEngine;

public class Screen_Host : MonoBehaviour
{
    public TextMeshProUGUI DifficultyTex;
    public TooltipObject DifficultyTooltip;
    public TextMeshProUGUI HostControlTex;
    public TooltipObject HostControlTooltip;
    private void OnEnable()
    {
        RefreshPlayerGameDifficulty(); RefreshHostControl();
        LOBBY.lobby.HostNameInput.text = GO.g.preferredLobbyName;
    }
    public void PressPlayerGameDifficulty()
    {
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
