using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockAttacks : MonoBehaviour
{
    public TOOL tool;
    public Collider2D col;
    public float ReduceDamageMod = 1f;
    public float BlockChance = 1f;
    public float ReflectProjectileChance = 0f;
    public bool isActive;
    private float EnableTimer = 0f;
    public void SetActive(bool bol)
    {
        EnableTimer = 0.1f;
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
