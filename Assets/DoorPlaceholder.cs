using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class DoorPlaceholder : MonoBehaviour
{
    public DRIFTER Drifter;
    public DoorSystem Door;
    public void InitDoor()
    {
        if (!Drifter.IsServer)
        {
            Destroy(gameObject);
            return;
        }
        DoorSystem mod = Instantiate(Door, transform.position, transform.rotation);
        mod.NetworkObject.Spawn();
        mod.transform.SetParent(Drifter.Space.transform);
        mod.SpaceID.Value = Drifter.Space.SpaceID.Value;
        mod.Faction = Drifter.GetFaction();
        mod.SetHomeDrifter(Drifter);
        mod.Init();
        mod.Space.GetModules().Add(mod);
        Destroy(gameObject);
    }
}
