using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectShip : MonoBehaviour
{
    SpawnableShip Ship;
    public TextMeshProUGUI Title;
    public Image Icon;
    bool IsUnlocked = false;
    public void Init(SpawnableShip ship)
    {
        Ship = ship;
        Title.text = ship.Name;
        Title.color = ship.Color;
        Icon.sprite = ship.Icon;

        if (GO.g.UnlockedDrifters.Contains(ship.ID))
        {
            IsUnlocked = true;
            Icon.color = Color.white;
        } else
        {
            Icon.color = Color.gray;
            IsUnlocked = false;
        }
    }
    public void Pressed()
    {
        if (!IsUnlocked)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        UI.ui.ShipSelectionUI.PressShipButton(Ship);
    }
}
