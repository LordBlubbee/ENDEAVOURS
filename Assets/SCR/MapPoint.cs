using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapPoint : NetworkBehaviour
{
    [NonSerialized] public List<MapPoint> ConnectedPoints = new();

    private void Start()
    {
        CO.co.RegisterMapPoint(this);
    }
}
