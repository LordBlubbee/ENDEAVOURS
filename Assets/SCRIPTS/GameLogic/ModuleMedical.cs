using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleMedical : Module
{
    public float RegenAuraRange;
    public float RegenAmount;

    public override void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        CurHealth.Value = MaxHealth;
        if (IsServer)
        {
            StartCoroutine(RegenAura());
        }
    }
    IEnumerator RegenAura()
    {
        while (true)
        {
            if (!IsDisabled() && Space)
            {
                foreach (CREW crew in Space.GetCrew())
                {
                    if (crew.GetFaction() != GetFaction()) continue;
                    if (crew.isDead()) continue;
                    if (Vector3.Distance(crew.transform.position, transform.position) < RegenAuraRange)
                    {
                        crew.Heal(RegenAmount/2f);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
