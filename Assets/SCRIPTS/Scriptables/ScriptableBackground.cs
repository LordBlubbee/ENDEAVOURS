
using System;
using System.Collections.Generic;
using UnityEngine;
using static CO;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableBackground", order = 1)]
public class ScriptableBackground : ScriptableObject
{
    public string BackgroundName;
    public string ResourcePath;
    public Sprite MainIcon;
    public Sprite Sprite_Player;
    public Sprite Sprite_Stripes;
    [TextArea(3, 10)]
    public string ShortDesc;
    [TextArea(5, 10)]
    public string LongDesc;
    public Color BackgroundColor;
    [Header("PHYS, ARM, DEX, COM, CMD, ENG, ALC, MED")]
    public int[] Background_ATT_BONUS = new int[8];
    public ScriptableEquippableWeapon Background_StartingWeapon;
    public ScriptableEquippableWeapon Background_StartingWeapon2;
    public ScriptableEquippableWeapon Background_StartingArtifact;
    public ScriptableEquippableWeapon Background_StartingArmor;
    public List<FactionReputation> Background_ReputationEffect;
    public enum NameCategories
    {
        NONE,
        LOGIPEDAN,
        CATALI,
        EPHEMERAL,
        NOMADEN,
        BAKUTO
    }
    public NameCategories NameCategory;
    public string GetRandomName()
    {
        List<string> list = new();
        switch (NameCategory)
        {
            case NameCategories.LOGIPEDAN:
                list.Add("Ally");
                break;
            case NameCategories.CATALI:
                list.Add("Ally");
                break;
            case NameCategories.EPHEMERAL:
                list.Add("Ally");
                break;
            case NameCategories.NOMADEN:
                list.Add("Ally");
                break;
            case NameCategories.BAKUTO:
                list.Add("Ally");
                break;
            default:
                list.Add("Ally");
                break;
        }
        return list.Count == 0 ? "" : list[UnityEngine.Random.Range(0,list.Count)];
    }
    public string GetRandomNameEnemy()
    {
        List<string> list = new();
        switch (NameCategory)
        {
            case NameCategories.LOGIPEDAN:
                list.Add("Logipedan");
                break;
            case NameCategories.CATALI:
                list.Add("Catali");
                break;
            case NameCategories.EPHEMERAL:
                list.Add("Ephemeral");
                break;
            case NameCategories.NOMADEN:
                list.Add("Nomaden");
                break;
            case NameCategories.BAKUTO:
                list.Add("Bakuto");
                break;
        }
        return list.Count == 0 ? "" : list[UnityEngine.Random.Range(0, list.Count)];
    }
}
[Serializable]
public struct FactionReputation
{
    public Faction Fac;
    public int Amount;
}