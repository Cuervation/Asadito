using System;
using UnityEngine;
namespace Asadito.Runtime
{
    [Serializable] public sealed class ProductEconomy
    {
        public string FoodId;
        public float PortionAmount;
        public int Price, Stock, UnlockLevel, FreshCycles;
    }
    [Serializable] public sealed class ManagementConfig
    {
        public bool EnableFreshness;
        public int PlayableLevels, ManagementFromLevel, CoinsFromLevel;
        public float MinimumRewardRatio, WasteEconomyPenalty, WasteOperationsPenalty, SurplusPenalty;
        public int InitialBalance, FridgeCapacity, BaseReward, RewardPerGuest, RecoveryStarCap;
        public float FoodWeight, EconomyWeight, OperationsWeight, RecoveryRewardMultiplier;
        public ProductEconomy[] Products;
        public ProductEconomy Product(string id)
        {
            foreach (var item in Products) if (item.FoodId == id) return item;
            throw new ArgumentException("No economy configuration for " + id);
        }
        private static ManagementConfig cached;
        public static ManagementConfig Load()
        {
            if (cached == null)
            {
                var data = Resources.Load<TextAsset>("Definitions/ManagementConfig");
                if (data == null) throw new InvalidOperationException("ManagementConfig missing");
                cached = JsonUtility.FromJson<ManagementConfig>(data.text);
                if (cached.InitialBalance < 0 || cached.FridgeCapacity < 2 || cached.Products == null ||
                    cached.FoodWeight + cached.EconomyWeight + cached.OperationsWeight <= 0)
                    throw new InvalidOperationException("Invalid management configuration");
                foreach (var product in cached.Products)
                {
                    FoodCatalog.Get(product.FoodId);
                    if (product.Price <= 0 || product.Stock < 0 || product.FreshCycles < 1)
                        throw new InvalidOperationException("Invalid product economy: " + product.FoodId);
                }
            }
            return cached;
        }
    }
}
