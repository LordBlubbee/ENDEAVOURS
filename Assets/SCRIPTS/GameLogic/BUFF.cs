using System.Xml.Linq;
using UnityEngine;
using static CO_SPAWNER;

public class BUFF
{
    private ScriptableBuff buff;
    public string Name;
    private int Stacks;
    private float Duration;
    public CO_SPAWNER.BuffParticles BuffParticles;
    public string DebuffTex;
    public float HealthChangePerSecond;

    public CREW BuffOwner;
    public int GetStacks()
    {
        return Stacks;
    }
    public int GetMaxStacks()
    {
        return buff.MaxStacks;
    }

    public float TemporaryHitpoints = 0;
    public ScriptableBuff GetScriptable()
    {
        return buff;
    }
    public BUFF(ScriptableBuff buf, CREW crew, CREW Owner)
    {
        Name = buf.name;
        buff = buf;
        BuffParticles = buf.BuffParticles;
        BuffOwner = Owner;
        AddBuff(buf, crew, Owner);
    }
    public void AddBuff(ScriptableBuff buf, CREW crew, CREW Owner)
    {
        Name = buf.name;
        Duration = buf.Duration;
        buff = buf;
        if (Stacks >= buf.MaxStacks)
        {
            LoseStack(crew);
        }
        Stacks = Mathf.Clamp(Stacks + 1, 1, buf.MaxStacks);

        HealthChangePerSecond += buff.HealthChangePerSecond;

        crew.ModifyHealthMax.Value += buff.ModifyHealthMax;
        crew.ModifyHealthRegen += buff.ModifyHealthRegen;
        crew.ModifyStaminaMax += buff.ModifyStaminaMax;
        crew.ModifyStaminaRegen += buff.ModifyStaminaRegen;
        crew.ModifyMovementSpeed += buff.ModifyMovementSpeed;
        crew.ModifyMovementSlow += buff.ModifyMovementSlow;
        crew.ModifyAnimationSpeed += buff.ModifyAnimationSpeed;
        crew.ModifyAnimationSlow += buff.ModifyAnimationSlow;

        crew.ModifyMeleeDamage += buff.ModifyMeleeDamage;
        crew.ModifyRangedDamage += buff.ModifyRangedDamage;
        crew.ModifySpellDamage += buff.ModifySpellDamage;

        crew.ModifyHealingDone += buff.ModifyHealingDone;
        crew.ModifyRepairDone += buff.ModifyRepairDone;

        crew.ModifyDamageTaken += buff.ModifyDamageTaken;
        crew.FireRes += buff.ModifyDamageResEnv;
        crew.MistRes += buff.ModifyDamageResEnv;
        crew.MeleeRes += buff.ModifyDamageResMelee;
        crew.RangedRes += buff.ModifyDamageResRanged;
        crew.SpellRes += buff.ModifyDamageResSpell;

        TemporaryHitpoints = buff.TemporaryHitpoints;
    }
    public void RemoveBuff(CREW crew)
    {
        while (Stacks > 0)
        {
            LoseStack(crew);
        }
    }

    public void LoseStack(CREW crew)
    {
        Stacks--;

        HealthChangePerSecond -= buff.HealthChangePerSecond;

        crew.ModifyHealthMax.Value -= buff.ModifyHealthMax;
        crew.ModifyHealthRegen -= buff.ModifyHealthRegen;
        crew.ModifyStaminaMax -= buff.ModifyStaminaMax;
        crew.ModifyStaminaRegen -= buff.ModifyStaminaRegen;
        crew.ModifyMovementSpeed -= buff.ModifyMovementSpeed;
        crew.ModifyMovementSlow -= buff.ModifyMovementSlow;
        crew.ModifyAnimationSpeed -= buff.ModifyAnimationSpeed;
        crew.ModifyAnimationSlow -= buff.ModifyAnimationSlow;

        crew.ModifyMeleeDamage -= buff.ModifyMeleeDamage;
        crew.ModifyRangedDamage -= buff.ModifyRangedDamage;
        crew.ModifySpellDamage -= buff.ModifySpellDamage;

        crew.ModifyHealingDone -= buff.ModifyHealingDone;
        crew.ModifyRepairDone -= buff.ModifyRepairDone;

        crew.ModifyDamageTaken -= buff.ModifyDamageTaken;
        crew.FireRes -= buff.ModifyDamageResEnv;
        crew.MistRes -= buff.ModifyDamageResEnv;
        crew.MeleeRes -= buff.ModifyDamageResMelee;
        crew.RangedRes -= buff.ModifyDamageResRanged;
        crew.SpellRes -= buff.ModifyDamageResSpell;
    }
    public bool IsExpired()
    {
        return Duration <= 0f;
    }

    public void TickBuff(float sped)
    {
        Duration -= sped;
    }
}
