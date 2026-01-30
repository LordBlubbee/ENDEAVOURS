using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static iDamageable;
using static Module;

public class LooncrabEgg : NetworkBehaviour, iDamageable
{
    public Light2D Lig;
    public Collider2D Col;
    private NetworkVariable<float> MaxHealth = new(100);
    public CREW SpawnPrefab;
    private NetworkVariable<float> CurHealth = new();
    public SpriteRenderer Spr;
    public Sprite BrokenSprite;

    int QualityLevel;
    public SPACE Space { get; set; }
    DUNGEON Dungeon;
    public void InitializeEgg(SPACE space, DUNGEON dung, int Quality)
    {
        Space = space;
        Dungeon = dung;
        QualityLevel = Quality;
        transform.SetParent(space.transform);
    }
    public ModuleTypes GetInteractableType() //Add iInteractable to reactivate, currently unused
    {
        return ModuleTypes.DRAGGABLE;
    }
    public void Heal(float fl)
    {
        if (fl < 0) return;
        CurHealth.Value = Mathf.Clamp(CurHealth.Value + fl, 0, GetMaxHealth());
        CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
    }
    public void TakeDamage(float fl, Vector3 src, DamageType type)
    {
        if (fl < 0) return;
        if (CurHealth.Value == 0) return;
        CurHealth.Value = Mathf.Clamp(CurHealth.Value - fl, 0, GetMaxHealth());
        if (CurHealth.Value <= 0)
        {
            DestructionRpc();

            Lig.enabled = false;
            Col.enabled = false;
            CREW crew = CO_SPAWNER.co.SpawnUnitInDungeon(SpawnPrefab, Dungeon, transform.position);
            CO_SPAWNER.co.SetQualityLevelOfCrew(crew, QualityLevel);
        }
        CO_SPAWNER.co.SpawnDMGRpc(fl, transform.position);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void DestructionRpc()
    {
        Spr.sprite = BrokenSprite;
    }
    public int GetFaction()
    {
        return 0;
    }
    public bool CanBeTargeted(SPACE space)
    {
        if (space != Space) return false;
        return true;
    }
    public float GetHealth()
    {
        return CurHealth.Value;
    }
    public float GetMaxHealth()
    {
        return MaxHealth.Value;
    }
    public void InitEgg(float health)
    {
        MaxHealth.Value = health;
        CurHealth.Value = health;
    }
    public float GetHealthRelative()
    {
        return GetHealth() / GetMaxHealth();
    }
    public void RemoveEgg()
    {
        NetworkObject.Despawn();
    }
}
