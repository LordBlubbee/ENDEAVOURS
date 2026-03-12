using System.Collections;
using UnityEngine;

public class MasteryDestructionSlash : ArtifactAbility
{
    public MasteryDestructionSlash(CREW crew, ScriptableMasteryItem item) : base(crew)
    {
        Mastery = item;
    }
    public override void OnMelee()
    {
        ProcAbility();
    }
    private void ProcAbility()
    {
        if (Random.Range(0f, 1f) > 0.15f + 0.01f * User.GetATT_ALCHEMY()) return;

        PROJ proj = (PROJ)GameObject.Instantiate(Mastery.AbilityPrefab1.GetComponent<PROJ>(), User.transform.position + User.getLookVector() * 0.5f, User.transform.rotation);
        float dmg = 10f;
        dmg += User.ModifyMeleeDamage;
        dmg *= 1f + 0.1f * User.GetATT_PHYSIQUE();
        proj.NetworkObject.Spawn();
        proj.Init(dmg, User.GetFaction(), User.Space, User.GetLookTowards());
        proj.CrewOwner = User;
    }
}
