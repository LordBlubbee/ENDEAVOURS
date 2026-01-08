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
        if (IsEffectActive()) return;
        if (IsEffectOnCooldown()) return;
        SetActive();
        Activation();
    }
    protected virtual void Activation()
    {
       
    }
    protected virtual void Deactivation()
    {

    }
    public void SetActive()
    {
        if (IsEffectActive()) return;
        if (IsEffectOnCooldown()) return;
        StartCoroutine(ActivateCooldown());
    }
    public void SetCooldown()
    {
        Debug.Log("Trying to set cooldown...");
        if (IsEffectActive()) return;
        if (IsEffectOnCooldown()) return;
        Debug.Log("Success!...");
        StartCoroutine(CooldownOnly());
    }
    protected IEnumerator CooldownOnly()
    {
        EffectCooldown.Value = GetEffectCooldownMax();
        while (EffectCooldown.Value > 0f)
        {
            yield return null;
            EffectCooldown.Value -= CO.co.GetWorldSpeedDelta();
        }
    }
    protected IEnumerator ActivateCooldown()
    {
        EffectActive.Value = GetEffectDuration();
        while (EffectActive.Value > 0f && !IsDisabled())
        {
            yield return null;
            EffectActive.Value -= CO.co.GetWorldSpeedDelta();
        }
        Deactivation();
        EffectActive.Value = 0;
        StartCoroutine(CooldownOnly());
    }
}
