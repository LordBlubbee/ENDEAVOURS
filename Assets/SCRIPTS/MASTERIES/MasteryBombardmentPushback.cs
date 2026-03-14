using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasteryBombardmentPushback : ArtifactAbility
{
    public MasteryBombardmentPushback(CREW crew) : base(crew)
    {
    }
    public override void OnEnemyHitMelee(CREW crew, float damageDone)
    {
        Hit(crew);
    }
    public override void OnEnemyHitRanged(CREW crew, float damageDone)
    {
        Hit(crew);
    }
    public override void OnEnemyHitSpell(CREW crew, float damageDone)
    {
        Hit(crew);
    }
    List<CREW> PushingBack = new();
    public void Hit(CREW crew)
    {
        if (PushingBack.Contains(crew)) return;
        crew.Push(3f, 0.15f, (crew.transform.position - User.transform.position).normalized);
        User.StartCoroutine(Pushback(crew));
    }
    IEnumerator Pushback(CREW crew)
    {
        PushingBack.Add(crew);
        float Timer = 0f;
        while (Timer < 0.15f)
        {
            Timer += CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        if (crew != null) PushingBack.Remove(crew);
    }
}
