using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class DoorPlaceholder : MonoBehaviour
{
    public DRIFTER Drifter;
    public DoorSystem Door;
    public void InitDoor(SPACE space)
    {
        if (!Drifter.IsServer)
        {
            Destroy(gameObject);
            return;
        }
        DoorSystem mod = Instantiate(Door, transform.position, transform.rotation);
        mod.NetworkObject.Spawn();
        mod.transform.SetParent(space.transform);
        mod.SpaceID.Value = space.SpaceID.Value;
        mod.Faction = Drifter.GetFaction();
        mod.SetHomeDrifter(Drifter);
        mod.Init();
        space.GetModules().Add(mod);
        Destroy(gameObject);
    }
}
