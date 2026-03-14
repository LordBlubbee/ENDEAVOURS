using UnityEngine;

public class ArtifactAbility
{
    protected CREW User;
    protected ScriptableEquippableArtifact Artifact;
    protected ScriptableMasteryItem Mastery;
    public ArtifactAbility(CREW crew)
    {
        User = crew;
    }

    public virtual void PeriodicEffect()
    {

    }
    public virtual void OnMelee()
    {

    }
    public virtual void OnRanged()
    {

    }
    public virtual void OnSpell()
    {

    }
    public virtual void OnMedkit(CREW crew)
    {

    }
    public virtual void OnRepair(Module module, float amount)
    {

    }
    public virtual void OnDash()
    {

    }
    public virtual void OnDamaged()
    {

    }
    public virtual float OnPreventDamageMelee(float dam)
    {
        return dam;
    }
    public virtual float OnPreventDamageRanged(float dam)
    {
        return dam;
    }
    public virtual float OnPreventDamageSpell(float dam)
    {
        return dam;
    }
    public virtual void OnEnemyHitMelee(CREW crew, float damageDone)
    {

    }
    public virtual void OnEnemyHitRanged(CREW crew, float damageDone)
    {

    }
    public virtual void OnEnemyHitSpell(CREW crew, float damageDone)
    {

    }
    public virtual void OnEnemyHitModule(Module mod, float damageDone)
    {

    }
    public virtual void OnEnemyKill(CREW crew)
    {

    }
}
