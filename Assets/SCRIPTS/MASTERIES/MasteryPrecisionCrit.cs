using System.Collections;
using UnityEngine;

public class MasteryPrecisionCrit : ArtifactAbility
{
    public MasteryPrecisionCrit(CREW crew, ScriptableMasteryItem item) : base(crew)
    {
        Mastery = item;
    }
    public override void OnSpell()
    {
        ProcAbility();
    }
    public override void OnRanged()
    {
        ProcAbility();
    }
    private void ProcAbility()
    {
        if (Random.Range(0f, 1f) > 0.15f + 0.01f * User.GetATT_ALCHEMY()) return;

        PROJ proj = (PROJ)GameObject.Instantiate(Mastery.AbilityPrefab1.GetComponent<PROJ>(), User.transform.position + User.getLookVector() * 0.5f, User.transform.rotation);
        float dmg = 14f;
        dmg += User.ModifyRangedDamage;
        dmg *= 1f + 0.1f * User.GetATT_ARMS();
        proj.NetworkObject.Spawn();
        proj.Init(dmg, User.GetFaction(), User.Space, User.GetLookTowards());
        proj.CrewOwner = User;
    }
}
