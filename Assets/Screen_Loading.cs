using UnityEngine;

public class Screen_Loading : MonoBehaviour
{
    private void Update()
    {
        if (CO.co == null) return;
        if (CO.co.HasShipBeenLaunched.Value)
        {
            UI.ui.SelectScreen(UI.ui.CharacterCreationUI);
        } else
        {
            UI.ui.SelectScreen(UI.ui.ShipSelectionUI.gameObject);
        }
    }
}
