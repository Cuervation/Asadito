using System;
using System.Collections.Generic;
namespace Asadito.Runtime
{
    public enum Freshness { Fresh, OK, ConsumeSoon, Spoiled }
    [Serializable] public sealed class InventoryUnit
    {
        public int Id;
        public string FoodId;
        public int Cost, AcquiredCycle;
        public bool Recovery;
        public Freshness FreshnessAt(int cycle, int life)
        {
            int age = Math.Max(0, cycle - AcquiredCycle);
            if (age >= life) return Freshness.Spoiled;
            if (age == life - 1) return Freshness.ConsumeSoon;
            return age == 0 ? Freshness.Fresh : Freshness.OK;
        }
    }
    [Serializable] public sealed class PurchaseRecord { public string FoodId; public int Quantity; }
    [Serializable] public sealed class AsadoRun
    {
        public int Level, FoodCost;
        public bool Recovery;
        public List<InventoryUnit> Units = new List<InventoryUnit>();
    }
    [Serializable] public sealed class ManagementState
    {
        public int Balance, Cycle, FreshnessCycle, NextUnitId = 1, TotalRewards, TotalWaste, PendingWaste;
        public int FridgeTier = 1, FreezerTier, GrillTier = 1;
        public List<InventoryUnit> Inventory = new List<InventoryUnit>();
        public List<PurchaseRecord> Purchases = new List<PurchaseRecord>();
        public bool IntroRewardGranted;
        public AsadoRun ActiveRun;
        public static ManagementState New(ManagementConfig config) => new ManagementState { Balance = config.InitialBalance };
    }
    public sealed class Wallet
    {
        private readonly ManagementState state;
        public Wallet(ManagementState state) { this.state = state; }
        public int Balance => state.Balance;
        public bool Spend(int value)
        {
            if (value < 0 || value > Balance) return false;
            state.Balance -= value; return true;
        }
        public void Credit(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            state.Balance = checked(state.Balance + value);
        }
    }
    public enum PromotionKind { None, Percentage, BuyNPayM, Pack, Daily, Clearance, Premium }
    // Future offers use the same quote contract; no promotions are activated in Vertical 1.
    public sealed class Promotion
    {
        public PromotionKind Kind;
        public int Percent, Buy = 2, Pay = 1, PackQuantity = 1, PackPrice;
        public int Quote(int unitPrice, int quantity)
        {
            if (unitPrice < 0 || quantity < 1) throw new ArgumentOutOfRangeException();
            if (Kind == PromotionKind.BuyNPayM)
            {
                if (Buy < 1 || Pay < 0 || Pay > Buy) throw new InvalidOperationException("Invalid promotion");
                return checked((quantity / Buy * Pay + quantity % Buy) * unitPrice);
            }
            if (Kind == PromotionKind.Pack)
            {
                if (PackQuantity < 1 || PackPrice < 0) throw new InvalidOperationException("Invalid pack");
                return checked(quantity / PackQuantity * PackPrice + quantity % PackQuantity * unitPrice);
            }
            int discount = Kind == PromotionKind.None || Kind == PromotionKind.Premium ? 0 : Math.Max(0, Math.Min(100, Percent));
            return checked((int)Math.Ceiling((long)unitPrice * quantity * (100 - discount) / 100d));
        }
    }
}
