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
        if (User.GetHealthRelative() > 0.1f) return;
       

        User.StartCoroutine(Cooldown());
    }
    private float CooldownTimer = 0f;
    IEnumerator Cooldown()
    {
        CooldownTimer = 1f;
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        User.Heal(50f);
        if (User.GetHomeDrifter() != null)
        {
            User.TeleportCrewMember(User.GetHomeDrifter().Space.GetRandomGrid().transform.position, User.GetHomeDrifter().Space);
        }
        CooldownTimer = 90f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            CooldownTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
    }
}
