using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class UI_SubscreenPlayers : MonoBehaviour
{
    public TextMeshProUGUI MaxTex;
    public List<UI_PlayerCrewButton> PlayerButtons;
    private void OnEnable()
    {
        StartCoroutine(RefreshNum());
    }
    IEnumerator RefreshNum()
    {
        while (true)
        {
            Refresh();
            yield return new WaitForSeconds(1);
        }
    }

    private void Refresh()
    {
        List<LOCALCO> OtherPlayers = new(CO.co.GetLOCALCO());
        OtherPlayers.Remove(LOCALCO.local);

        MaxTex.text = $"ELITE CREW ({OtherPlayers.Count + 1}/8)";

        int i = 0;
        foreach (UI_PlayerCrewButton button in PlayerButtons)
        {
            if (i < OtherPlayers.Count)
            {
                if (OtherPlayers[i] != null)
                {
                    button.gameObject.SetActive(true);
                    CREW Play = OtherPlayers[i].GetPlayer();
                    button.SetCrew(Play);
                    i++;
                    continue;
                }
            }
            button.gameObject.SetActive(false);
            i++;
        }
    }
}
