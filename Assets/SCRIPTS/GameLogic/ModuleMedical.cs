using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleMedical : Module
{
    public float RegenAuraRange;
    public float RegenAmount;
    public float RegenAuraPerLevel;
    public float RegenAmountPerLevel;
    public override void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;
        if (!IsServer) return;
        CurHealth.Value = MaxHealth;

        StartCoroutine(RegenAura());
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
                    float Dist = Vector3.Distance(crew.transform.position, transform.position);
                    if (Dist < RegenAuraRange + RegenAuraPerLevel * ModuleLevel.Value)
                    {
                        float Divide = 4f;
                        if (Dist < 10f) Divide = 2f;
                        crew.Heal((RegenAmount + RegenAmountPerLevel * ModuleLevel.Value) / Divide);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
