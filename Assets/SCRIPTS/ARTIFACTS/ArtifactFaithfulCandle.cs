using System.Collections;
using UnityEngine;

public class ArtifactFaithfulCandle : ArtifactAbility
{
    private float CooldownTimer = 0f;

    public ArtifactFaithfulCandle(CREW crew, ScriptableEquippableArtifact art) : base(crew)
    {
        Artifact = art;
    }

    IEnumerator Cooldown()
    {
        CooldownTimer = 12f / (0.9f + User.GetATT_ALCHEMY() * 0.1f);
        while (CooldownTimer > 0f)
        {
            yield return new WaitForSeconds(1);
            CooldownTimer -= 1;
        }
    }
    public override void OnSpell()
    {
        ProcAbility();
    }
    public override void OnMelee()
    {
        ProcAbility();
    }
    public override void OnRanged()
    {
        ProcAbility();
    }

    private void ProcAbility()
    {
        Debug.Log("Proccing...");
        if (CooldownTimer > 0) return;

        PROJ proj = (PROJ)GameObject.Instantiate(Artifact.AbilityPrefab1.GetComponent<PROJ>(), User.transform.position + User.getLookVector() * 0.5f, User.transform.rotation);
        float dmg = 30f;
        dmg += User.ModifySpellDamage;
        dmg *= 0.6f + 0.2f * User.GetATT_COMMUNOPATHY();
        proj.NetworkObject.Spawn();
        proj.Init(dmg, User.GetFaction(), User.Space, User.GetLookTowards());
        proj.CrewOwner = User;

        User.StartCoroutine(Cooldown());
    }
}
