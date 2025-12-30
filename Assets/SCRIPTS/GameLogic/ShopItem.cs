using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ShopItem : NetworkBehaviour
{
    [NonSerialized] public NetworkVariable<int> MaterialCost = new();
    [NonSerialized] public NetworkVariable<int> SupplyCost = new();
    [NonSerialized] public NetworkVariable<int> AmmoCost = new();
    [NonSerialized] public NetworkVariable<int> TechCost = new();
    [NonSerialized] public NetworkVariable<FixedString64Bytes> ShopItemLink = new();
    [NonSerialized] public ScriptableShopitem Item;
    private void Start()
    {
        StartCoroutine(FindShopItem());
    }
    public void Init(ScriptableShopitem item)
    {
        //Called only on server
        Item = item;
        CO.co.AddShopItem(this);
        ShopItemLink.Value = item.GetResourceLink();
    }
    IEnumerator FindShopItem()
    {
        while (Item == null)
        {
            if (ShopItemLink.Value != default)
            {
                CO.co.AddShopItem(this);
                Item = Resources.Load<ScriptableShopitem>($"OBJ/SCRIPTABLES/SHOP/{ShopItemLink.Value.ToString()}");
            }
            yield return null;
        }
    }
    public override void OnNetworkDespawn()
    {
        CO.co.RemoveShopItem(this);
    }

    public bool CanBeBought()
    {
        if (MaterialCost.Value == -1) return false;
        if (MaterialCost.Value > CO.co.Resource_Materials.Value) return false;
        if (SupplyCost.Value > CO.co.Resource_Supplies.Value) return false;
        if (AmmoCost.Value > CO.co.Resource_Ammo.Value) return false;
        if (TechCost.Value > CO.co.Resource_Tech.Value) return false;
        return true;
    }

    [Rpc(SendTo.Server)]
    public void BuyShopItemRpc()
    {
        if (Item == null)
        {
            Debug.Log("Error: Item is null");
            return;
        }
        if (!CanBeBought()) return;
        int MatCost = MaterialCost.Value;
        MaterialCost.Value = -1; //Set to BOUGHT
        //Get the item here!
        if (Item.Equippable)
        {
            CO.co.AddInventoryItem(Item.Equippable);
        }
        if (Item.BuyCrew)
        {
            CREW NewCrew = CO_SPAWNER.co.SpawnUnitOnShip(Item.BuyCrew, CO.co.PlayerMainDrifter);
            CO_SPAWNER.co.SetQualityLevelOfCrew(NewCrew, 120 * CO.co.GetNewFriendlyCrewModifier());
        }
        CO.co.Resource_Materials.Value -= MatCost;
        CO.co.Resource_Supplies.Value -= SupplyCost.Value;
        CO.co.Resource_Ammo.Value -= AmmoCost.Value;
        CO.co.Resource_Tech.Value -= TechCost.Value;

        CO.co.Resource_Materials.Value += Item.DealMaterialsGain;
        CO.co.Resource_Supplies.Value += Item.DealSuppliesGain;
        CO.co.Resource_Ammo.Value += Item.DealAmmoGain;
        CO.co.Resource_Tech.Value += Item.DealTechGain;
    }
}
