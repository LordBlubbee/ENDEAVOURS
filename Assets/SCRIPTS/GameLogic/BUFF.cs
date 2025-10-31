using UnityEngine;

public class BUFF : MonoBehaviour
{
    private ScriptableBuff buff;
    private int Stacks;
    private float Duration;

    public ScriptableBuff GetScriptable()
    {
        return buff;
    }
    public BUFF(ScriptableBuff buf, CREW crew)
    {
        buff = buf;
        AddBuff(buf, crew);
    }

    public void AddBuff(ScriptableBuff buf, CREW crew)
    {
        Duration = buf.Duration;
        if (Stacks >= buf.MaxStacks) return;
        Stacks = Mathf.Clamp(Stacks + 1, 1, buf.MaxStacks);

        crew.ModifyHealthMax += buff.ModifyHealthMax;
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
    }
    public void RemoveBuff(CREW crew)
    {
        for (int i = 0; i < Stacks;i++)
        {
            crew.ModifyHealthMax -= buff.ModifyHealthMax;
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
