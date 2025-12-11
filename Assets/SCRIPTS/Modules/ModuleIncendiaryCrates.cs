using UnityEngine;

public class ModuleIncendiaryCrates : ModuleEffector
{
    public float GetFlameBoost()
    {
        return 0.4f + ModuleLevel.Value * 0.1f;
    }
    public float GetDamageBonusMod()
    {
        return 1.2f + ModuleLevel.Value * 0.1f;
    }
    protected override void DeactivateModule()
    {
        //EXPLOSION
        if (!IsDisabled()) return;
        CO_SPAWNER.co.SpawnExplosionLargeRpc(transform.position);
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position,16f))
        {
            CREW crew = col.GetComponent<CREW>();
            if (!crew) continue;
            float Dis = (crew.transform.position - transform.position).magnitude;
            crew.TakeDamage(40f + (16f / Dis) * 5f,crew.transform.position, iDamageable.DamageType.ENVIRONMENT_FIRE);
        }
    }
}
