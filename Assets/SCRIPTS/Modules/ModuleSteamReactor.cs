using System.Collections;
using UnityEngine;

public class ModuleSteamReactor : ModuleEffector
{
    public float GetDodgeBoost()
    {
        return 0.4f + ModuleLevel.Value * 0.05f;
    }
    public float GetArmorBoost()
    {
        return 5f + ModuleLevel.Value* 5f;
    }

    protected override void Activation()
    {
        StartCoroutine(ArmorRepair());
    }
    protected IEnumerator ArmorRepair()
    {
        while (EffectActive.Value > 0f)
        {
            yield return new WaitForSeconds(0.1f);
            foreach (Module mod in Space.SystemModules)
            {
                if (mod is ModuleArmor)
                {
                    ((ModuleArmor)mod).HealArmor(GetArmorBoost()*0.1f);
                }
            }
        }
    }
}
