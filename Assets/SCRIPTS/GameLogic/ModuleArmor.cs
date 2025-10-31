using System;
using Unity.Netcode;
using UnityEngine;

public class ModuleArmor : Module
{
    public float MaxArmor = 100;
    public float ArmorPerUpgrade = 100;
    public float ArmorRegenPerUpgrade = 2;
    public float ArmorRegen = 10;
    float WaitWithRegen = 0f;
    [NonSerialized] public NetworkVariable<float> CurArmor = new();
    public override void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        if (!IsServer) return;
        CurHealth.Value = MaxHealth;
        CurArmor.Value = MaxArmor;
    }

    public float GetMaxArmor()
    {
        return MaxArmor + ArmorPerUpgrade * ModuleLevel.Value;
    }
    protected override void Frame()
    {
        if (!IsServer) return;
        if (IsDisabled())
        {
            CurArmor.Value = 0;
        }
        if (WaitWithRegen > 0) {
            WaitWithRegen -= CO.co.GetWorldSpeedDelta();
            return;
        }
        CurArmor.Value = Mathf.Clamp(CurArmor.Value + (ArmorRegen + ArmorRegenPerUpgrade* ModuleLevel.Value) * CO.co.GetWorldSpeedDelta(), 0, GetMaxArmor());
    }
    public void TakeArmorDamage(float fl, Vector3 impact)
    {
        CurArmor.Value = Mathf.Clamp(CurArmor.Value - fl, 0, GetMaxArmor());
        if (fl > 50) WaitWithRegen = 1f + 0.02f * fl;
        CO_SPAWNER.co.SpawnArmorDMGRpc(fl, impact);
    }
    public override void Heal(float fl)
    {
        if (IsDisabledForever()) return;
        CurHealth.Value = Mathf.Min(GetMaxHealth(), CurHealth.Value + fl);
        if (CurHealth.Value > 99) isDisabled.Value = false;
        if (GetHealthRelative() > 0.9f)
        {
            CurArmor.Value = Mathf.Clamp(CurArmor.Value + fl, 0, GetMaxArmor());
        }
        if (fl > 1) CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public bool CanAbsorbArmor()
    {
        return CurArmor.Value > 99 && !IsDisabled();
    }
    public float GetArmor()
    {
        return CurArmor.Value;
    }
}
