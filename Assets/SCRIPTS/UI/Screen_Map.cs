using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_Map : MonoBehaviour
{
    public MapPointButton PrefabMapScreenPoint;
    public Image PrefabLineRenderer; // Add this in the inspector
    public Transform Anchor1;
    public Transform Anchor2;
    private List<MapPointButton> MapScreenPoints = new();
    private List<Image> MapLines = new(); // Track created lines
    public Transform Map; 
    public GameObject[] ChoiceButton;
    public TextMeshProUGUI[] ChoiceButtonVotes;
    public TextMeshProUGUI[] ChoiceTex;
    public TextMeshProUGUI WarningTex;

    private void OnEnable()
    {
        // Clean up old points and lines
        UpdateMap();
    }
    private float GetStandardDistanceUnit()
    {
        return Anchor2.position.y - Anchor1.position.y;
    }

    private void Update()
    {
        for (int i = 0; i < ChoiceTex.Length; i++)
        {
            if (ChoiceButton[i].gameObject.activeSelf)
            {
                int Votes = CO.co.VoteResultAmount(i);
                ChoiceButtonVotes[i].text = Votes.ToString();
                ChoiceButtonVotes[i].color = (LOCALCO.local.CurrentMapVote.Value == i) ? Color.cyan : Color.white;
            }
        }
        if (CO.co.GetAlliedAICrew().Count > CO.co.PlayerMainDrifter.MaximumCrew)
        {
            WarningTex.gameObject.SetActive(true);
            WarningTex.text = "CANNOT CONTINUE: CREW FULL!";
        } else
        {
            WarningTex.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.U))
        {
            UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
        }
    }
    public void UpdateMap()
    {
        if (!gameObject.activeSelf) return;
        foreach (MapPointButton points in MapScreenPoints)
        {
            Destroy(points.gameObject);
        }
        MapScreenPoints = new();
        foreach (var line in MapLines)
        {
            Destroy(line.gameObject);
        }
        MapLines = new();

        // Spawn map points
        float scale = transform.localScale.x;
        transform.localScale = new Vector3(1, 1, 1);
        MapPoint PlayerPoint = CO.co.GetPlayerMapPoint();
        foreach (MapPoint map in CO.co.GetMapPoints())
        {
            //float Dist = (map.transform.position - CO.co.PlayerMapPoint.transform.position).magnitude;
            if (Mathf.Abs(map.transform.position.x - PlayerPoint.transform.position.x) > 30f) continue;
            Vector3 spawnPos = Anchor1.position + (map.transform.position - CO.co.GetPlayerMapPoint().transform.position) * GetStandardDistanceUnit();
            MapPointButton mpb = Instantiate(PrefabMapScreenPoint, spawnPos, Quaternion.identity, Map);

            mpb.SetDefaultColor(Color.gray);
            if (map == PlayerPoint)
            {
                mpb.Init(map, -1, false);
                mpb.SetDefaultColor(Color.cyan);
            }
            else if (PlayerPoint.ConnectedPoints.Contains(map))
            {
                int ID = 0;
                for (int i = 0; i < PlayerPoint.ConnectedPoints.Count; i++)
                {
                    if (PlayerPoint.ConnectedPoints[i] == map)
                    {
                        ID = i;
                        break;
                    }
                }
                mpb.Init(map, ID, true);
                mpb.SetDefaultColor(Color.white);

                // Draw UI line from center to yellow point
                Vector3 startPos = Map.transform.position;
                Vector3 endPos = spawnPos;

                // Convert world positions to local positions relative to Map
                Vector2 localStart = ((RectTransform)Map).InverseTransformPoint(startPos);
                Vector2 localEnd = ((RectTransform)Map).InverseTransformPoint(endPos);

                Vector2 direction = localEnd - localStart;
                float length = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Create the line
                Image line = Instantiate(PrefabLineRenderer, Map);
                RectTransform lineRect = line.rectTransform;
                lineRect.sizeDelta = new Vector2(length, 5f); // 5f is line thickness
                lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                lineRect.pivot = new Vector2(0f, 0.5f); // left edge as pivot
                lineRect.anchoredPosition = localStart;
                lineRect.rotation = Quaternion.Euler(0, 0, angle);

                line.color = Color.white;
                // Ensure the line renders under other objects
                line.transform.SetSiblingIndex(0);

                MapLines.Add(line);
            }
            else
            {
                mpb.Init(map, -1, false);
            }
            MapScreenPoints.Add(mpb);
        }
        for (int i = 0; i < ChoiceTex.Length; i++)
        {
            if (i < PlayerPoint.ConnectedPoints.Count) {
                ChoiceButton[i].gameObject.SetActive(true);
                ChoiceTex[i].text = PlayerPoint.ConnectedPoints[i].GetNameAndData();
            }
            else ChoiceButton[i].gameObject.SetActive(false);
        }
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
