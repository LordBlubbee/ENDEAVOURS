using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Screen_Map : MonoBehaviour
{
    public MapPointButton PrefabMapScreenPoint;
    public Image PrefabLineRenderer; // Add this in the inspector
    private List<MapPointButton> MapScreenPoints = new();
    private List<Image> MapLines = new(); // Track created lines
    public Transform Map;

    private void OnEnable()
    {
        // Clean up old points and lines
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
        foreach (MapPoint map in CO.co.GetMapPoints())
        {
            float Dist = (map.transform.position - CO.co.PlayerMapPoint.transform.position).magnitude;
            if (Mathf.Abs(map.transform.position.x - CO.co.PlayerMapPoint.transform.position.x) > 30f) continue;
            if (Mathf.Abs(map.transform.position.y - CO.co.PlayerMapPoint.transform.position.y) > 18f) continue;
            Vector3 spawnPos = Map.transform.position + (map.transform.position - CO.co.PlayerMapPoint.transform.position) * 0.5f;
            MapPointButton mpb = Instantiate(PrefabMapScreenPoint, spawnPos, Quaternion.identity, Map);
            mpb.Image.color = Color.white;
            if (map == CO.co.PlayerMapPoint)
            {
                mpb.Image.color = Color.green;
            }
            else if (CO.co.PlayerMapPoint.ConnectedPoints.Contains(map))
            {
                mpb.Image.color = Color.yellow;

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

                line.color = Color.yellow;
                // Ensure the line renders under other objects
                line.transform.SetSiblingIndex(0);

                MapLines.Add(line);
            }
            MapScreenPoints.Add(mpb);
        }
    }
}
