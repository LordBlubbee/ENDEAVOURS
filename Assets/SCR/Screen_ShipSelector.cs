using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_ShipSelector : MonoBehaviour
{
    public List<SpawnableShip> SpawnableShips;
    public SelectShip SpawnShipButton;
    public Transform ShipList;

    [Header("INFO")]
    public Image Background;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Faction;
    public TextMeshProUGUI Data;
    public TextMeshProUGUI Desc;
    SpawnableShip SelectedShip;

    private void Start()
    {
        PressShipButton(SpawnableShips[0]);
        foreach (SpawnableShip ship in SpawnableShips)
        {
            Instantiate(SpawnShipButton, ShipList).Init(ship);
        }
    }
    private void Update()
    {
        if (CO.co.HasShipBeenLaunched.Value)
        {
            UI.ui.SelectScreen(UI.ui.CharacterCreationUI);
        }
    }
    public void PressShipButton(SpawnableShip ship)
    {
        SelectedShip = ship;
        Background.sprite = ship.Background;
        Title.text = ship.Name;
        Faction.text = ship.Faction;
        Data.text = ship.Data;
        Desc.text = ship.Desc;
    }

    public void PressLaunch()
    {
        if (CO.co == null) return;
        if (CO.co.HasShipBeenLaunched.Value) return;
        if (SelectedShip == null) return;
        for (int i = 0; i < SpawnableShips.Count; i++)
        {
            if (SelectedShip == SpawnableShips[i])
                CO_SPAWNER.co.SpawnPlayerShipRpc(i);
        }
    }
}

[Serializable]
public class SpawnableShip
{
    public DRIFTER Prefab;
    public Sprite Icon;
    public Sprite Background;
    public string Name;
    public string Faction;
    [TextArea(8, 16)]
    public string Desc;
    [TextArea(8, 16)]
    public string Data;
}
