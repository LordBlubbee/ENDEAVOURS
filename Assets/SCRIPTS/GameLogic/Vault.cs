using UnityEngine;
using static iDamageable;

public class Vault : Module
{
    public bool CanBeDamagedWhenFull = false;
    public override bool IsDisabled()
    {
        return false;
    }
    public override void TakeDamage(float fl, Vector3 src, DamageType type)
    {
        if (!CanBeDamagedWhenFull && GetHealthRelative() >= 1f && type != DamageType.TRUE) return;
        base.TakeDamage(fl, src, type);
    }
}
