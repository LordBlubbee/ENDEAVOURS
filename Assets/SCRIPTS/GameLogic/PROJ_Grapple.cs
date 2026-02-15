using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static CO;
public class PROJ_Grapple : PROJ
{
    public LineRenderer CreateLinePrefab;
    private LineRenderer Line;
    protected override void PotentialHitTarget(GameObject collision)
    {
        WalkableTile crew = collision.GetComponent<WalkableTile>();
        if (crew != null)
        {
            if (crew.Space == Space) return;
            if (!crew.canBeBoarded) return;
            CrewOwner.UseGrapple(crew);
            ImpactSFXRpc();

            isActive = false;
            transform.SetParent(collision.transform.parent);
            if (ImpactVFX) ImpactVFXRpc();
            ExpireSlowlyRpc();
            KillLineAfterDelayRpc();
            //Instead of bulletimpact()
            return;
        }
    }
    protected override void Start()
    {
        Transform tr = CO.co.GetTransformAtPoint(transform.position);
        if (tr != null && CreateLinePrefab)
        {
            Line = Instantiate(CreateLinePrefab);
            Line.transform.SetParent(tr);
            Line.SetPosition(0, Line.transform.parent.position);
            Line.SetPosition(1, Line.transform.parent.position);
        }
        base.Start();
    }
    protected override void FixedUpdate()
    {
        if (Line) Line.SetPosition(1, transform.position);
        base.FixedUpdate();
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void KillLineAfterDelayRpc()
    {
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        float Timer = 0f;
        while (Timer < 1f)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        KillLine();
    }

    public override void OnNetworkDespawn()
    {
        KillLine();
        base.OnNetworkDespawn();
    }
    protected void KillLine()
    {
        if (Line)
        {
            Destroy(Line.gameObject);
        }
    }
}
