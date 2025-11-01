using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockAttacks : MonoBehaviour
{
    public TOOL tool;
    public Collider2D col;
    public float ReduceDamageModMelee = 1f;
    public float ReduceDamageModRanged = 1f;
    public float BlockChanceMelee = 1f;
    public float BlockChanceRanged = 1f;
    public float ReflectProjectileChance = 0f;
    public bool isActive;
    public bool isAlwaysActive = false;
    public AUDCO.BlockSoundEffects BlockSound;
    private float EnableTimer = 0f;
    public void SetActive(bool bol)
    {
        if (isAlwaysActive) return;
        EnableTimer = 0.4f;
        if (!isActive) StartCoroutine(Timer());
        col.enabled = bol;
        isActive = bol;
    }

    private IEnumerator Timer()
    {
        while (EnableTimer > 0f)
        {
            EnableTimer -= CO.co.GetWorldSpeedDelta();
            yield return null;
        }
        col.enabled = false;
        isActive = false;
    }
}
