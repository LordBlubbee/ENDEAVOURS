using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MasterySabotageRestoration : ArtifactAbility
{
    public MasterySabotageRestoration(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitModule(Module mod, float damageDone)
    {
        if (CooldownTimer > 0f) return;
        if (!mod.IsDisabled()) return;
        User.Heal(50f + 10f * User.GetATT_ENGINEERING() * User.GetHealingFactor());
        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 20f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return null;
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
        }
    }
}
