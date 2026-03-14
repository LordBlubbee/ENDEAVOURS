using System.Collections;
using UnityEngine;

public class MasterySabotageElectricity : ArtifactAbility
{
    float StoredEnergy;
    public MasterySabotageElectricity(CREW crew) : base(crew)
    {
    }
    public override void OnRepair(Module module, float amount)
    {
        StoredEnergy = Mathf.Min(StoredEnergy+amount, 100f + User.GetATT_ENGINEERING()*20f);
    }
    public override void OnEnemyHitMelee(CREW crew, float damageDone)
    {
        if (StoredEnergy > 20) Hit(crew);
    }
    public override void OnEnemyHitRanged(CREW crew, float damageDone)
    {
        if (StoredEnergy > 20 && (crew.transform.position - User.transform.position).magnitude < 8) Hit(crew);
    }
    public override void OnEnemyHitSpell(CREW crew, float damageDone)
    {
        if (StoredEnergy > 20 && (crew.transform.position - User.transform.position).magnitude < 8) Hit(crew);
    }

    public void Hit(CREW crew)
    {
        User.GainCredit_CrewDamage(crew.TakeDamage(StoredEnergy * 0.4f, crew.transform.position, iDamageable.DamageType.MELEE_CRIT));
        crew.Push(8f, 0.2f, (crew.transform.position - User.transform.position).normalized);
        StoredEnergy = 0;
    }
    public override void OnEnemyHitModule(Module crew, float damageDone)
    {
        if (StoredEnergy > 20 && (crew.transform.position - User.transform.position).magnitude < 8)
        {
            User.GainCredit_ModuleDamage(crew.TakeDamage(StoredEnergy * 0.4f, crew.transform.position, iDamageable.DamageType.MELEE_CRIT));
            StoredEnergy = 0;
        }
    }
}
