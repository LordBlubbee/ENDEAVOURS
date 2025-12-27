using TMPro;
using UnityEngine;

public class UI_LoadGameButton : MonoBehaviour
{
    public TextMeshProUGUI LoadText;
    private dataStructure SaveGame;

    public void Init(dataStructure sav)
    {
        SaveGame = sav;
    }
    public void PressLoad()
    {

    }
}
