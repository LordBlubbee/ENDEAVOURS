using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class DoorPlaceholder : MonoBehaviour
{
    public DRIFTER Drifter;
    public DoorSystem Door;
    public float SpawnChance = 1f;
    public void InitDoor(SPACE space)
    {
        if (!Drifter.IsServer)
        {
            Destroy(gameObject);
            return;
        }
        if (Random.Range(0f, 1f) > SpawnChance)
        {
            Destroy(gameObject);
            return;
        }
        if (!space.GetCurrentGrid(transform.position))
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
