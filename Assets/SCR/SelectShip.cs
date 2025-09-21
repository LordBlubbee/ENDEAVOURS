using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectShip : MonoBehaviour
{
    SpawnableShip Ship;
    public TextMeshProUGUI Title;
    public Image Icon;
    public void Init(SpawnableShip ship)
    {
        Ship = ship;
        Title.text = ship.Name;
        Icon.sprite = ship.Icon;
    }
    public void Pressed()
    {
        UI.ui.ShipSelectionUI.PressShipButton(Ship);
    }
}
