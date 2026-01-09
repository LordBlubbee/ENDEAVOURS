using UnityEngine;
using static CO_SPAWNER;

public class BUFF
{
    private ScriptableBuff buff;
    private int Stacks;
    private float Duration;
    public CO_SPAWNER.BuffParticles BuffParticles;
    public string DebuffTex;
    public int GetStacks()
    {
        return Stacks;
    }

    public float TemporaryHitpoints = 0;
    public ScriptableBuff GetScriptable()
    {
        return buff;
    }
    public BUFF(ScriptableBuff buf, CREW crew)
    {
        buff = buf;
        BuffParticles = buf.BuffParticles;
        AddBuff(buf, crew);
    }
    public void AddBuff(ScriptableBuff buf, CREW crew)
    {
        Duration = buf.Duration;
        if (Stacks >= buf.MaxStacks) return;
        Stacks = Mathf.Clamp(Stacks + 1, 1, buf.MaxStacks);

        crew.HealthChangePerSecond += buff.HealthChangePerSecond;

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

        crew.ModifyDamageTaken += buff.ModifyDamageTaken;
        TemporaryHitpoints = buff.TemporaryHitpoints;
    }
    public void RemoveBuff(CREW crew)
    {
        for (int i = 0; i < Stacks;i++)
        {
            crew.HealthChangePerSecond -= buff.HealthChangePerSecond;

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

            crew.ModifyDamageTaken -= buff.ModifyDamageTaken;
        }
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
