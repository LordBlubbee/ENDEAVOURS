using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    ShopItem ItemLink;
    public TooltipObject Tool;
    public TextMeshProUGUI Cost;
    public TextMeshProUGUI Gain;
    public Image Icon;
    public void Init(ShopItem item)
    {
        ItemLink = item; 
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        Cost.text = "";
        Gain.text = "";
        if (item.Item.Equippable)
        {
            Icon.sprite = item.Item.Equippable.ItemIcon;
            Tool.Tooltip = $"{item.Item.Equippable.ItemName} \n\n{item.Item.Equippable.ItemDesc}";
        } else if (item.Item.BuyCrew)
        {
            Icon.sprite = item.Item.BuyCrew.Spr.sprite;
            Tool.Tooltip = $"{item.Item.BuyCrew.UnitName} \n\n{item.Item.BuyCrew.UnitDescription}";
        }
        else
        {
            Tool.Tooltip = "A deal with which you can exchange or craft resources.";
            if (item.Item.DealMaterialsGain > 0)
            {
                Icon.sprite = CO_SPAWNER.co.ShopItemMaterialDeal;
                Gain.text += $"+{item.Item.DealMaterialsGain}M\n";
            }
            if (item.Item.DealSuppliesGain > 0)
            {
                Icon.sprite = CO_SPAWNER.co.ShopItemSupplyDeal;
                Gain.text += $"+{item.Item.DealSuppliesGain}S\n";
            }
            if (item.Item.DealAmmoGain > 0)
            {
                Icon.sprite = CO_SPAWNER.co.ShopItemAmmoDeal;
                Gain.text += $"+{item.Item.DealAmmoGain}A\n";
            }
            if (item.Item.DealTechGain > 0)
            {
                Icon.sprite = CO_SPAWNER.co.ShopItemTechnologyDeal;
                Gain.text += $"+{item.Item.DealTechGain}T\n";
            }
        }
        if (item.Item.AlchemySkillRequirement > 0) Cost.text += $"<color=#44BB44>Needs ALC ({item.Item.AlchemySkillRequirement})\n";
        if (item.MaterialCost.Value > 0) Cost.text += $"<color=yellow>-{item.MaterialCost.Value}M ";
        if (item.SupplyCost.Value > 0) Cost.text += $"<color=green>-{item.SupplyCost.Value}S ";
        if (item.AmmoCost.Value > 0) Cost.text += $"<color=red>-{item.AmmoCost.Value}A ";
        if (item.TechCost.Value > 0) Cost.text += $"<color=#00AAFF>-{item.TechCost.Value}T ";
    }
    private void Update()
    {
        if (ItemLink)
        {
            if (ItemLink.MaterialCost.Value == -1)
            {
                Icon.color = Color.gray;
                Cost.text = "<color=green>[BOUGHT]";
            }
            else if (ItemLink.CanBeBought()) Icon.color = Color.white;
            else Icon.color = Color.gray;
        } else
        {
            gameObject.SetActive(false);
        }
    }
    public void WhenPressed()
    {
        if (!ItemLink) return;
        if (!ItemLink.CanBeBought())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (!CO.co.PermissionToModifyDrifter())
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Purchase);
        ItemLink.BuyShopItemRpc();
    }
}
