using Unity.Netcode;
using UnityEngine;

public class MapPoint : NetworkBehaviour
{
    private void Start()
    {
        CO.co.RegisterMapPoint(this);
    }
}
