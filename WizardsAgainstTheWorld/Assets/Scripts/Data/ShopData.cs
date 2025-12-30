using System;

[Serializable]
public class ShopData
{
    const float PriceIncreaseFactor = 1.25f;

    public int itemCount;
    public InventoryData inventory;
    public float priceMultiplier;
    public string traderIdentifier;
    public float fuelPriceMultiplier = 1f;
    public float juicePriceMultiplier = 1f;

    public decimal GetBuyPrice(decimal baseCost)
    {
        decimal price = baseCost * (decimal)priceMultiplier;
        return Math.Ceiling(price);
    }

    public decimal GetSellPrice(decimal baseCost)
    {
        decimal price = baseCost * (decimal)priceMultiplier * 0.5m;
        return Math.Floor(price);
    }

    public decimal GetFuelPrice()
    {
        return Math.Ceiling(9 * (decimal)priceMultiplier * (decimal)fuelPriceMultiplier);
    }

    public decimal GetJuicePrice()
    {
        return Math.Ceiling(6 * (decimal)priceMultiplier * (decimal)juicePriceMultiplier);
    }

    public void IncreaseFuelPrice()
    {
        fuelPriceMultiplier *= PriceIncreaseFactor;
    }

    public void IncreaseJuicePrice()
    {
        juicePriceMultiplier *= PriceIncreaseFactor;
    }
}