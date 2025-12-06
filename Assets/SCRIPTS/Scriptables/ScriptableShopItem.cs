using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableShopItem", order = 1)]
public class ScriptableShopitem : ScriptableObject
{
    //This is a SPEAKER, an animated/speaking entity that does the animation for a piece of text tied to it.
    public ScriptableEquippable Equippable;

    public int AlchemySkillRequirement = 0;
    public int Weight = 10;

    public bool IsCrafted = false;
    public int DealMaterialsCost;
    public int DealSuppliesCost;
    public int DealAmmoCost;
    public int DealTechCost;

    [Header("DEAL")]
    public int DealMaterialsGain;
    public int DealSuppliesGain;
    public int DealAmmoGain;
    public int DealTechGain;

}
