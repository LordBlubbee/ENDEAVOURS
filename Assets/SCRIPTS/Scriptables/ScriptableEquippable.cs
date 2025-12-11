using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
public class ScriptableEquippable : ScriptableObject
{
    public string ItemName;
    [TextArea(3, 10)]
    public string ItemDesc;
    public Sprite ItemIcon;
    public string ItemResourceIDFull;
    public string ItemResourceIDShort;

    [Header("Sell Worth")]
    public int SellMaterials = 0;
    public int SellSupplies = 0;
    public int SellTech = 0;

    public Rarities ItemRarity;
    public enum Rarities
    {
        CIVILIAN,
        MILITARY,
        EXPERIMENTAL,
        RARE,
        ARTIFACT
    }

    public static Color GetRarityColor(Rarities rarity)
    {
        switch (rarity)
        {
            case Rarities.CIVILIAN:
                return Color.white;
            case Rarities.MILITARY:
                return new Color(0,0.6f,0);
            case Rarities.EXPERIMENTAL:
                return new Color(0.8f, 1f, 0);
            case Rarities.RARE:
                return Color.cyan;
            case Rarities.ARTIFACT:
                return Color.magenta;
        }
        return Color.white;
    }

    public int[] MinimumAttributes;
    public string GetItemResourceIDFull()
    {
        return ItemResourceIDFull;
    }
    public string GetItemResourceIDShort()
    {
        return ItemResourceIDShort;
    }
}
