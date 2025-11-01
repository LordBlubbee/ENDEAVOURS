
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
    public string GetNameAndData()
    {
        return PointName.Value.ToString() + $"\n{AssociatedPoint.InitialMapData}";
    }
    public string GetData()
    {
        return $"\n{AssociatedPoint.InitialMapData}";
    }
    public string GetNameOnly()
    {
        return PointName.Value.ToString();
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

    private bool Initialized = false;

    public bool HasInitialized()
    {
        return Initialized;
    }
    public void Init(ScriptablePoint Dialog)
    {
        if (HasInitialized()) return;
        Initialized = true;
        AssociatedPoint = Dialog;
        if (IsServer)
        {
            if (Dialog.UniqueName == "") PointName.Value = $"POINT-{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}-{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}";
            else PointName.Value = Dialog.UniqueName;
            SetInitRpc(AssociatedPoint.GetResourceLink());
        }
    }

    public void Register(int ID)
    {
        //Server only
        PointID.Value = ID;
    }

    [Rpc(SendTo.Server)]
    public void RequestInitRpc()
    {
        SetInitRpc(AssociatedPoint.GetResourceLink());
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetInitRpc(string str)
    {
        if (IsServer) return;
        Init(Resources.Load<ScriptablePoint>($"OBJ/SCRIPTABLES/EVENTS/{str}"));
    }
}
