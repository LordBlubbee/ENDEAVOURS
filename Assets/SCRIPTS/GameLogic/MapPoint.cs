
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MapPoint : NetworkBehaviour
{
    [NonSerialized] public List<MapPoint> ConnectedPoints = new();
    [NonSerialized] public NetworkVariable<int> PointID = new();
    [NonSerialized] private NetworkVariable<FixedString32Bytes> PointName = new();
    public string GetName()
    {
        return PointName.Value + $"\n{AssociatedPoint.InitialMapData}";
    }
    private void Start()
    {
        CO.co.RegisterMapPoint(this);
        if (!IsServer)
        {
            RequestInitRpc();
        }
    }
    [NonSerialized] public ScriptablePoint AssociatedPoint;
    public void Init(ScriptablePoint Dialog, int ID)
    {
        AssociatedPoint = Dialog;
        if (IsServer)
        {
            PointName.Value = $"POINT-{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}-{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}";
            PointID.Value = ID;
            SetInitRpc(AssociatedPoint.ResourceLink);
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestInitRpc()
    {
        SetInitRpc(AssociatedPoint.ResourceLink);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetInitRpc(string str)
    {
        if (IsServer) return;
        Init(Resources.Load<ScriptablePoint>(str), -1);
    }
}
