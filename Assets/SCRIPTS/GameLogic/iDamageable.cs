using UnityEngine;

public interface iDamageable
{
    public SPACE Space { get; set; }

    public bool CanBeTargeted();
    public int GetFaction();
    public float GetHealth();

    public float GetMaxHealth();
    public float GetHealthRelative();
    public void Heal(float fl);
    public void TakeDamage(float fl, Vector3 src);
    public Transform transform { get; }

}
