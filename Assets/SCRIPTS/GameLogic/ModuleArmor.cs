using System;
using Unity.Netcode;
using UnityEngine;

public class ModuleArmor : Module
{
    public float MaxArmor = 100;
    public float ArmorRegen = 10;
    public float ArmorAuraSize = 40;
    [NonSerialized] public NetworkVariable<float> CurArmor = new();
    public override void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        CurHealth.Value = MaxHealth;
        CurArmor.Value = MaxArmor;
    }

    private void Update()
    {
        if (!IsServer) return;
        CurArmor.Value = Mathf.Clamp(CurArmor.Value + ArmorRegen * CO.co.GetWorldSpeedDelta(), 0, MaxArmor);
    }
    public void TakeArmorDamage(float fl)
    {
        float before = CurArmor.Value;
        CurArmor.Value = Mathf.Clamp(CurArmor.Value - fl, 0, MaxArmor);
    }

    public float GetArmor()
    {
        return CurArmor.Value;
    }
}
