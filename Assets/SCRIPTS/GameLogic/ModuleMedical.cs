using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleMedical : Module
{
    public float RegenAuraRange;
    public float RegenAmount;

    private void OnEnable()
    {
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
                foreach (CREW crew in Space.CrewInSpace)
                {
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
