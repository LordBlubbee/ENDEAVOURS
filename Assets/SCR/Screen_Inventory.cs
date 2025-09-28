using UnityEngine;

public class Screen_Inventory : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
        }
    }
}
