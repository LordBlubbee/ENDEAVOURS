using UnityEngine;
using static iDamageable;

public class Vault : Module
{
    public bool CanBeDisabled = false;
    public bool CanBeDamagedWhenFull = false;
    public override bool IsDisabled()
    {
        return base.IsDisabled() && CanBeDisabled;
    }
    public override float TakeDamage(float fl, Vector3 src, DamageType type)
    {
        if (!CanBeDamagedWhenFull && GetHealthRelative() >= 1f && type != DamageType.TRUE) return 0f;
        return base.TakeDamage(fl, src, type);
    }
}
