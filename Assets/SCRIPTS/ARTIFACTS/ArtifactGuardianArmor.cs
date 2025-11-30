using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

public class ArtifactGuardianArmor : ArtifactAbility
{
    public ArtifactGuardianArmor(CREW crew) : base(crew)
    {
    }

    public override float OnPreventDamageMelee(float dam)
    {
        if (CooldownTimer > 0) return dam;
        User.StartCoroutine(Cooldown());
        return 0f;
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 30f / (0.9f + User.GetATT_COMMAND() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return new WaitForSeconds(1);
            CooldownTimer -= 1;
        }
    }
}
