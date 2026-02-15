using System.Collections;
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
        StartCoroutine(EggHatch());
    }
    int Sensitivity = 30;
    IEnumerator EggHatch()
    {
        Sensitivity = Random.Range(8, 42);
        while (Col.enabled)
        {
            int Closest = 999;
            foreach (CREW Crew in CO.co.GetAlliedCrew())
            {
                Closest = Mathf.Min(Closest, (int)(Crew.transform.position - transform.position).magnitude);
            }
            if (Closest < 5)
            {
                Sensitivity--;
            }
            if (Closest < 10)
            {
                Sensitivity--;
            }
            if (Closest < 15)
            {
                Sensitivity--;
            }
            if (Closest < 20)
            {
                Sensitivity--;
                if (Sensitivity < 10)
                {
                    StartPulseRpc();
                }
            }
            if (Sensitivity < 1)
            {
                Hatch();
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPulseRpc()
    {
        StartCoroutine(PulseLight());
    }
    IEnumerator PulseLight()
    {
        float Light = 4f;
        while (Light > 1f && Lig.enabled)
        {
            Lig.intensity = Light;
            Light -= 9f * Time.deltaTime;
            yield return null;
        }
    }
    public ModuleTypes GetInteractableType() //Add iInteractable to reactivate, currently unused
    {
        return ModuleTypes.DRAGGABLE;
    }
    public float Heal(float fl)
    {
        if (fl < 0) return 0f;
        float Old = CurHealth.Value;
        CurHealth.Value = Mathf.Clamp(CurHealth.Value + fl, 0, GetMaxHealth());
        CO_SPAWNER.co.SpawnHealRpc(fl, transform.position);
        return CurHealth.Value - Old;
    }
    public float TakeDamage(float fl, Vector3 src, DamageType type)
    {
        if (fl < 0) return 0f;
        if (CurHealth.Value == 0) return 0f;
        CurHealth.Value = Mathf.Clamp(CurHealth.Value - fl, 0, GetMaxHealth());
        Sensitivity -= UnityEngine.Random.Range(1, 8);
        if (CurHealth.Value <= 0)
        {
            DestructionRpc();

            Lig.enabled = false;
            Col.enabled = false;
            if (Random.Range(0f,1f) < 0.15f && Sensitivity < 5)
            {
                SpawnCrab();
            }
        }
        CO_SPAWNER.co.SpawnDMGRpc(fl, transform.position);
        return fl;
    }

    private void Hatch()
    {
        DestructionRpc();

        Lig.enabled = false;
        Col.enabled = false;

        SpawnCrab();
    }
    private void SpawnCrab()
    {
        CREW crew = CO_SPAWNER.co.SpawnUnitInDungeon(SpawnPrefab, Dungeon, transform.position);
        CO_SPAWNER.co.SetQualityLevelOfCrew(crew, QualityLevel);
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
