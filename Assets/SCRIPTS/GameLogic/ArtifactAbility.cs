using UnityEngine;

public class ArtifactAbility
{
    protected CREW User;
    protected ScriptableEquippableArtifact Artifact;
    public ArtifactAbility(CREW crew)
    {
        User = crew;
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
    public virtual void OnDash()
    {

    }
    public virtual void OnDamaged(CREW us)
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
    public virtual void OnEnemyHitMelee(CREW crew)
    {

    }
    public virtual void OnEnemyHitRanged(CREW crew)
    {

    }
    public virtual void OnEnemyHitSpell(CREW crew)
    {

    }
    public virtual void OnEnemyKill(CREW crew)
    {

    }
}
