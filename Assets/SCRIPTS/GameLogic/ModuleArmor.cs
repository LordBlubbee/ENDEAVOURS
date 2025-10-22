using System;
using Unity.Netcode;
using UnityEngine;

public class ModuleArmor : Module
{
    public float MaxArmor = 100;
    public float ArmorRegen = 10;
    float WaitWithRegen = 0f;
    [NonSerialized] public NetworkVariable<float> CurArmor = new();
    public override void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        CurHealth.Value = MaxHealth;
        CurArmor.Value = MaxArmor;
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
        CurArmor.Value = Mathf.Clamp(CurArmor.Value + ArmorRegen * CO.co.GetWorldSpeedDelta(), 0, MaxArmor);
    }
    public void TakeArmorDamage(float fl, Vector3 impact)
    {
        CurArmor.Value = Mathf.Clamp(CurArmor.Value - fl, 0, MaxArmor);
        if (fl > 50) WaitWithRegen = 1f + 0.02f * fl;
        CO_SPAWNER.co.SpawnArmorDMGRpc(fl, impact);
    }

    public bool CanAbsorbArmor()
    {
        return CurArmor.Value > 99;
    }
    public float GetArmor()
    {
        return CurArmor.Value;
    }
}
