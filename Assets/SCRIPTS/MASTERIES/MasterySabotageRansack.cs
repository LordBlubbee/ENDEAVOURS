using System.Collections;
using UnityEngine;

public class MasterySabotageRansack : ArtifactAbility
{
    public MasterySabotageRansack(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitModule(Module mod, float damageDone)
    {

        if (CooldownTimer > 0f) return;

        User.GainCredit_ModuleDamage(mod.TakeDamage(100f, mod.transform.position, iDamageable.DamageType.SPELL_CRIT));

        User.StartCoroutine(Cooldown());
    }   
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 20f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return null;
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
        }
    }
}

