using UnityEngine;

public class ArtifactTokenOfVengeance : ArtifactAbility
{
    public ArtifactTokenOfVengeance(CREW crew) : base(crew)
    {
    }

    public override void OnDamaged()
    {
        foreach (CREW allies in CO.co.GetEnemyCrew(User.GetFaction()))
        {
            if ((User.transform.position - allies.transform.position).magnitude > 16f) continue;
            ScriptableBuff buff = new();
            buff.name = "TokenOfVengeance";
            buff.MaxStacks = 5;
            buff.Duration = 3;
            buff.ModifyDamageTaken += 0.2f+User.GetATT_ALCHEMY()*0.04f;
            allies.AddBuff(buff);
        }

    }
}
