using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleSteamReactor : ModuleEffector
{
    public float GetDodgeBoost()
    {
        return 0.4f + ModuleLevel.Value * 0.05f;
    }
    public float GetArmorBoost()
    {
        return 4f + ModuleLevel.Value* 4f;
    }

    public GameObject SteamVFXPrefab;
    public AudioClip ActivateSFX;
    public AudioClip DeactivateSFX;
    protected override void Activation()
    {
        ActivationRpc();
        StartCoroutine(ArmorRepair());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ActivationRpc()
    {
        AUDCO.aud.PlaySFXLoud(ActivateSFX, transform.position);
        Instantiate(SteamVFXPrefab, transform.position, Quaternion.identity);
    }
    protected override void Deactivation()
    {
        DeactivationRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DeactivationRpc()
    {
        AUDCO.aud.PlaySFXLoud(DeactivateSFX, transform.position);
    }
    protected IEnumerator ArmorRepair()
    {
        int HealArmor = 4;
        while (EffectActive.Value > 0f)
        {
            yield return new WaitForSeconds(0.25f);
            HealArmor--;
            foreach (Module mod in Space.SystemModules)
            {
                if (mod is ModuleArmor)
                {
                    ((ModuleArmor)mod).HealArmor(GetArmorBoost()*0.25f);
                    if (HealArmor < 1) ((ModuleArmor)mod).Heal(GetArmorBoost()*1f);
                }
            }
            if (HealArmor < 1)
            {
                HealArmor += 4;
            }
        }
    }
}
