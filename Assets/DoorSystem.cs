using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DoorSystem : Module
{
  //  public Collider2D Col;
    private NetworkVariable<bool> IsOpen = new();
    public Transform Door1;
    public Transform Door1Open;
    public Transform Door1Close;
    public Transform Door2;
    public Transform Door2Open;
    public Transform Door2Close;
    public override void Init()
    {
        if (IsServer)
        {
            StartCoroutine(OpeningMechanism());
        }
        base.Init();
    } 
    IEnumerator OpeningMechanism()
    {
        while (true)
        {
            if (IsDisabled()) IsOpen.Value = true;
            else
            {
                IsOpen.Value = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, 4f))
                {
                    CREW crew = col.GetComponent<CREW>();
                    if (crew != null)
                    {
                        if (crew.GetFaction() == GetFaction())
                        {
                            IsOpen.Value = true;
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
    }

    protected override void Frame()
    {
        bool Open = IsOpen.Value;
        //Col.enabled = !Open;
        Vector3 GoTo = Open ? Door1Open.position : Door1Close.position;
        Door1.transform.position = Vector3.Lerp(Door1.transform.position, GoTo, CO.co.GetWorldSpeedDelta() * 3f);
        GoTo = Open ? Door2Open.position : Door2Close.position;
        Door2.transform.position = Vector3.Lerp(Door2.transform.position, GoTo, CO.co.GetWorldSpeedDelta() * 3f);
        base.Frame();
    }
}
