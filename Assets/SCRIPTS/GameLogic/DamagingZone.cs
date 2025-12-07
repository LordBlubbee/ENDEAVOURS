using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DamagingZone : NetworkBehaviour
{
    public ParticleSystem ParticleSystem;
    public float Duration = 10f;
    public float DamagePerSecond = 20f;
    public float DamageRadius = 0f;
    public iDamageable.DamageType DamageType;
    public float ModuleDamageMod = 0f;
    float DurationLeft = 0f;
    public void Init(float DamageMod = 1f)
    {
        DamagePerSecond *= DamageMod;
        StartCoroutine(DamageTick());
    }
    IEnumerator DamageTick()
    {
        DurationLeft = Duration;
        float Timer = 0f;
        while (DurationLeft > 0f)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            if (Timer < 0f)
            {
                Timer += 0.5f;
                DurationLeft -= 0.5f;
                float Damage = DamagePerSecond * 0.5f;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position,DamageRadius))
                {
                    CREW Crew = col.GetComponent<CREW>();
                    if (Crew != null)
                    {
                        if (Crew.isDead()) continue;
                        Crew.TakeDamage(Damage, Crew.transform.position, DamageType);
                    }
                    if (ModuleDamageMod > 0)
                    {
                        Module Mod = col.GetComponent<Module>();
                        if (Mod != null)
                        {
                            if (Mod.IsDisabled()) continue;
                            Mod.TakeDamage(Damage * ModuleDamageMod, Mod.transform.position, DamageType);
                        }
                    }
                }
            }
            yield return null;
        }
        if (ParticleSystem)
        {
            ParticleSystem.Stop();
            while (ParticleSystem.particleCount > 0) yield return null;
            yield return new WaitForSeconds(2f);
        }
        NetworkObject.Despawn();
    }
}
