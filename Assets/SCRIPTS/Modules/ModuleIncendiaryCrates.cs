using Unity.Netcode;
using UnityEngine;

public class ModuleIncendiaryCrates : ModuleEffector
{
    public AudioClip ActivateSFX;
    public AudioClip DeactivateSFX;
    protected override void Activation()
    {
        ActivationRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ActivationRpc()
    {
        AUDCO.aud.PlaySFXLoud(ActivateSFX, transform.position);
    }
    protected override void Deactivation()
    {
        DeactivationRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DeactivationRpc()
    {
        AUDCO.aud.PlaySFXLoud(DeactivateSFX, transform.position);
    }
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
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position,24f))
        {
            CREW crew = col.GetComponent<CREW>();
            if (!crew) continue;
            float Dis = (crew.transform.position - transform.position).magnitude;
            crew.TakeDamage((60f + (1f - (Dis / 16f)) * 60f) * (1f+ModuleLevel.Value*0.5f),crew.transform.position, iDamageable.DamageType.ENVIRONMENT_FIRE);
        }
    }
}
