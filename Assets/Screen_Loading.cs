using System;
using UnityEngine;

public class Screen_Loading : MonoBehaviour
{
    private bool Loaded = false;
    private void Update()
    {
        if (CO.co == null) return;
        if (LOCALCO.local == null) return;
        if (!CO.co.GetLOCALCO().Contains(LOCALCO.local)) return;
        if (Loaded) return;
        Loaded = true;
        if (CO.co.IsServer)
        {
            dataStructure sav = GO.g.loadSlot(GO.g.currentSaveSlot);
            if (sav != null)
            {
                sav.loadGame();
            }
        }
        if (CO.co.HasShipBeenLaunched.Value)
        {
            UI.ui.SelectScreen(UI.ui.CharacterCreationUI.gameObject);
        } else
        {
            UI.ui.SelectScreen(UI.ui.ShipSelectionUI.gameObject);
        }
    }
}
