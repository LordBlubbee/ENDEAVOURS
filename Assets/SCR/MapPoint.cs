using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapPoint : NetworkBehaviour
{
    [NonSerialized] public List<MapPoint> ConnectedPoints = new();
    private string PointName;
    public string GetName()
    {
        return PointName + $"\n{AssociatedPoint.InitialMapData}";
    }
    private void Start()
    {
        CO.co.RegisterMapPoint(this);
    }
    [NonSerialized] public ScriptablePoint AssociatedPoint;
    public void Init(ScriptablePoint Dialog)
    {
        AssociatedPoint = Dialog;
        PointName = $"POINT-{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}-{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}";
    }
}
