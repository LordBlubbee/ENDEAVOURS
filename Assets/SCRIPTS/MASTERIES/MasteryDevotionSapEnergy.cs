using System.Collections;
using UnityEngine;

public class MasteryDevotionSapEnergy : ArtifactAbility
{
    public MasteryDevotionSapEnergy(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitSpell(CREW crew, float damageDone)
    {
        if (Random.Range(0f,1f) < 0.25f)
        {
            float Heal = damageDone * User.GetHealingFactor() * 0.3f;
            User.GainCredit_Healing(User.Heal(Heal));
            foreach (CREW allies in CO.co.GetAlliedCrew(User.GetFaction()))
            {
                if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
                if (allies.isDead()) continue;
                allies.AddStamina(Heal);
            }
        }
    }
}
