using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ModuleEffector : Module
{
    protected NetworkVariable<float> EffectCooldown = new();
    protected NetworkVariable<float> EffectActive = new();
    protected NetworkVariable<bool> EffectAutomatic = new(true);
    public float EffectDuration = 8f;
    public float EffectMaxCooldown = 40f;

    public float EffectDurationPerLevel = 2f;
    public float EffectCooldownReductionPerLevel = -5f;
    public float GetEffectDuration()
    {
        return EffectDuration + EffectDurationPerLevel * ModuleLevel.Value;
    }
    public float GetEffectCooldownMax()
    {
        return EffectMaxCooldown + EffectCooldownReductionPerLevel * ModuleLevel.Value;
    }
    public bool IsEffectOnCooldown()
    {
        return EffectCooldown.Value > 0f;
    }
    public bool IsEffectActive()
    {
        return EffectActive.Value > 0f;
    }

    public float GetEffectCooldown()
    {
        return EffectCooldown.Value;
    }

    public float GetEffectActive()
    {
        return EffectActive.Value;
    }

    public bool IsEffectAutomatic()
    {
        return EffectAutomatic.Value;
    }

    [Rpc(SendTo.Server)]
    public void ChangeAutomaticRpc(bool bol)
    {
        EffectAutomatic.Value = bol;
    }

    [Rpc(SendTo.Server)]
    public void ActivateEffectRpc()
    {
        ActivateEffect();
    }
    public void ActivateEffect()
    {
        if (IsEffectOnCooldown()) return;
        SetCooldown();
        Activation();
    }
    protected virtual void Activation()
    {
       
    }

    public void SetCooldown()
    {
        if (IsEffectActive()) return;
        if (IsEffectOnCooldown()) return;
        StartCoroutine(ActivateCooldown());
    }
    protected IEnumerator ActivateCooldown()
    {
        EffectActive.Value = GetEffectDuration();
        while (EffectActive.Value > 0f)
        {
            yield return null;
            EffectActive.Value -= CO.co.GetWorldSpeedDelta();
        }
        EffectCooldown.Value = GetEffectCooldownMax();
        while (EffectCooldown.Value > 0f)
        {
            yield return null;
            EffectCooldown.Value -= CO.co.GetWorldSpeedDelta();
        }
    }
}
