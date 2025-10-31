using UnityEngine;

public class BUFF : MonoBehaviour
{
    private ScriptableBuff buff;
    private int Stacks;
    private float Duration;
    public BUFF(ScriptableBuff buf, CREW crew)
    {
        buff = buf;
        Stacks = Mathf.Clamp(Stacks + 1, 1, buf.MaxStacks);
        Duration = buf.Duration;

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
        crew.ModifyHealthMax -= buff.ModifyHealthMax;
        crew.ModifyHealthRegen -= buff.ModifyHealthRegen;
        crew.ModifyStaminaMax -= buff.ModifyStaminaMax;
        crew.ModifyStaminaRegen -= buff.ModifyStaminaRegen;
        crew.ModifyMovementSpeed -= buff.ModifyMovementSpeed;
        crew.ModifyMovementSlow -= buff.ModifyMovementSlow;
        crew.ModifyAnimationSpeed -= buff.ModifyAnimationSpeed;
        crew.ModifyAnimationSlow -= buff.ModifyAnimationSlow;

        crew.ModifyMeleeDamage -= buff.ModifyMeleeDamage;
        crew.ModifyRangedDamage -=  buff.ModifyRangedDamage;
        crew.ModifySpellDamage -= buff.ModifySpellDamage;
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
