using UnityEngine;

public class ArtifactTokenOfVengeance : ArtifactAbility
{
    public ArtifactTokenOfVengeance(CREW crew) : base(crew)
    {
    }

    public override void OnDamaged()
    {
        foreach (CREW enemies in CO.co.GetEnemyCrew(User.GetFaction()))
        {
            if ((User.transform.position - enemies.transform.position).magnitude > 20f) continue;
            ScriptableBuff buff = new();
            buff.name = "TokenOfVengeance";
            buff.MaxStacks = 5;
            buff.BuffParticles = CO_SPAWNER.BuffParticles.VENGEANCE;
            buff.Duration = 7;
            buff.ModifyDamageTaken += 0.2f+User.GetATT_ALCHEMY()*0.04f;
            enemies.AddBuff(buff);
        }

    }
}
