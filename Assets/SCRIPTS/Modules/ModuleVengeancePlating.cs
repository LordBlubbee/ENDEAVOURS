using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Experimental;
using UnityEngine;

public class ModuleVengeancePlating : Module
{
    public PROJ ShrapnelPrefab;
    public GameObject ShrapnelVFX;
    public float GetShrapnelChance()
    {
        return 0.5f + ModuleLevel.Value * 0.1f;
    }
    public float GetShrapnelDamage()
    {
        return 30f + ModuleLevel.Value * 20f;
    }
    public void FireShrapnel(Vector3 origin)
    {
        if (UnityEngine.Random.Range(0f, 1f) > GetShrapnelChance()) return;
        PlayShrapnelVFXRpc(origin);
        Vector3 Target = GetClosestEnemyPositionInNebula(origin);
        if (Target == Vector3.zero) return;
        PROJ proj = (PROJ)GameObject.Instantiate(ShrapnelPrefab, origin, Quaternion.identity);

        // Rotate toward target (2D rotation -> Z axis)
        Vector3 dir = Target - origin;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float dmg = GetShrapnelDamage();
        proj.NetworkObject.Spawn();
        proj.Init(dmg, GetHomeDrifter().GetFaction(), null, Target);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayShrapnelVFXRpc(Vector3 vec)
    {
        Instantiate(ShrapnelVFX, vec, Quaternion.identity);
    }
    public Vector3 GetClosestEnemyPositionInNebula(Vector3 vec)
    {
        List<CREW> enemies = CO.co.GetAllCrews();
        Vector3 closest = Vector3.zero;
        float minDist = float.MaxValue;
        Vector3 myPos = vec;

        List<DRIFTER> drifters = CO.co.GetAllDrifters();
        foreach (var enemy in drifters)
        {
            if ((enemy.transform.position - vec).magnitude > 200) continue;
            if (enemy.GetFaction() == 0 || enemy.GetFaction() == Faction) continue;
            if (enemy.isDead()) continue;
            float dist = (enemy.getPos() - myPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform.TransformPoint(new Vector3(UnityEngine.Random.Range(-enemy.RadiusX, enemy.RadiusX), UnityEngine.Random.Range(-enemy.RadiusY, enemy.RadiusY)));
            }
        }
        return closest;
    }
}
