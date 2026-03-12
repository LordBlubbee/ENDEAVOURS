using System.Collections;
using UnityEngine;

public class MasteryDevotionEscape : ArtifactAbility
{
    public MasteryDevotionEscape(CREW crew) : base(crew)
    {
    }
    public override void OnDamaged()
    {
        if (CooldownTimer > 0) return;
        if (User.GetHealthRelative() > 0.3f) return;
        User.Heal(50f);
        if (User.GetHomeDrifter() != null)
        {
            User.TeleportCrewMember(User.GetHomeDrifter().MedicalModule.transform.position,User.GetHomeDrifter().Space);
        }

        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 90f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }
}
