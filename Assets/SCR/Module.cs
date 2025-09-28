using Unity.Netcode;
using UnityEngine;

public class Module : NetworkBehaviour
{
    // Damageable and Interactable Module

    [Header("MAIN")]
    public ModuleTypes ModuleType;
    public enum ModuleTypes
    {
        NAVIGATION,
        WEAPON,
        INVENTORY,
        MAPCOMMS,
        GENERATOR,
        ARMOR_MODULE,
        LOON_NAVIGATION
    }

    [Header("STATS")]
    public float MaxHealth = 100f;
    public float HitboxRadius = 16f;

    private NetworkVariable<float> CurHealth = new();

    private void Start()
    {
        if (!IsServer) return;

        Init();
    }

    private bool hasInitialized = false;

    public void Init()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        CurHealth.Value = MaxHealth;
    }
    public void InteractWithModule()
    {

    }
    public void Heal(float fl)
    {
        CurHealth.Value = Mathf.Min(MaxHealth, CurHealth.Value + fl);
    }
    public void TakeDamage(float fl)
    {
        CurHealth.Value -= fl;
        if (CurHealth.Value < 0.1f)
        {
            CurHealth.Value = 0f;
            //Death
        }
    }
}
