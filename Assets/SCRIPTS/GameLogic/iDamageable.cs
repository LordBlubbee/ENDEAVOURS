using UnityEngine;

public interface iDamageable
{
    public SPACE Space { get; set; }

    public bool CanBeTargeted(SPACE space);
    public int GetFaction();
    public float GetHealth();

    public float GetMaxHealth();
    public float GetHealthRelative();
    public void Heal(float fl);
    public void TakeDamage(float fl, Vector3 src, DamageType type);
    public enum DamageType
    {
        TRUE,
        MELEE,
        RANGED,
        SPELL,
        BOMBARDMENT,
        ENVIRONMENT_FIRE,
        ENVIRONMENT_MIST,
        MELEE_CRIT,
        RANGED_CRIT,
        SPELL_CRIT
    }
    public Transform transform { get; }

}
