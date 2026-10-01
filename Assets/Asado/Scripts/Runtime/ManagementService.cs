using System;
using System.Collections.Generic;
using UnityEngine;
namespace Asadito.Runtime
{
    public sealed class EconomicResult
    {
        public float Asador, Economy, Operations, Overall;
        public int Spending, Waste, Income, Profit, Balance, Stars;
        public bool Recovery;
    }
    /// <summary>Transactions operate on the save aggregate; UI persists only successful state changes.</summary>
    public sealed class ManagementService
    {
        public readonly ManagementState State;
        public readonly ManagementConfig Config;
        public readonly Wallet Wallet;
        public ManagementService(ManagementState state, ManagementConfig config)
        { State = state; Config = config; Wallet = new Wallet(state); }
        public int Available(string foodId)
        {
            int count = 0;
            foreach (var unit in State.Inventory)
                if (unit.FoodId == foodId && unit.FreshnessAt(State.FreshnessCycle, Config.Product(foodId).FreshCycles) != Freshness.Spoiled) count++;
            return count;
        }
        public int Stock(string id)
        {
            int purchased = 0;
            foreach (var record in State.Purchases) if (record.FoodId == id) purchased += record.Quantity;
            return Math.Max(0, Config.Product(id).Stock - purchased);
        }
        public string Buy(string id, int quantity, int level, Promotion promotion = null)
        {
            if (State.ActiveRun != null) return "Terminá el asado actual";
            var product = Config.Product(id);
            if (quantity < 1) return "Cantidad inválida";
            if (level < product.UnlockLevel) return "Corte bloqueado";
            if (Stock(id) < quantity) return "Sin stock";
            if (State.Inventory.Count + quantity > Config.FridgeCapacity) return "Heladera llena";
            int cost = (promotion ?? new Promotion()).Quote(product.Price, quantity);
            if (!Wallet.Spend(cost)) return "No alcanzan las monedas";
            for (int i = 0; i < quantity; i++)
                State.Inventory.Add(new InventoryUnit { Id = State.NextUnitId++, FoodId = id,
                    Cost = cost / quantity + (i < cost % quantity ? 1 : 0), AcquiredCycle = State.FreshnessCycle });
            State.Purchases.Add(new PurchaseRecord { FoodId = id, Quantity = quantity });
            return null;
        }
        public bool Discard(int id)
        {
            var unit = State.Inventory.Find(x => x.Id == id);
            if (unit == null || State.ActiveRun != null) return false;
            State.TotalWaste += unit.Cost; State.PendingWaste += unit.Cost;
            State.Inventory.Remove(unit); return true;
        }
        public bool CanPrepare(string[] foods)
        {
            var counts = new Dictionary<string, int>();
            foreach (var food in foods) { if (!counts.ContainsKey(food)) counts[food] = 0; counts[food]++; }
            foreach (var pair in counts) if (Available(pair.Key) < pair.Value) return false;
            return foods.Length > 0 && State.ActiveRun == null;
        }
        public bool Prepare(int level, string[] selected)
        {
            if (!CanPrepare(selected)) return false;
            var run = new AsadoRun { Level = level };
            foreach (var food in selected)
            {
                var unit = State.Inventory.Find(x => x.FoodId == food && x.FreshnessAt(State.FreshnessCycle, Config.Product(food).FreshCycles) != Freshness.Spoiled);
                State.Inventory.Remove(unit); run.Units.Add(unit); run.FoodCost += unit.Cost; run.Recovery |= unit.Recovery;
            }
            State.ActiveRun = run; return true;
        }
        public bool CanRecover(string[] foods)
        {
            if (foods == null || foods.Length == 0 || State.ActiveRun != null || CanPrepare(foods)) return false;
            var needs = new Dictionary<string, int>();
            foreach (var food in foods) { if (!needs.ContainsKey(food)) needs[food] = 0; needs[food]++; }
            int cost = 0, missing = 0;
            foreach (var pair in needs)
            {
                int count = Math.Max(0, pair.Value - Available(pair.Key)); missing += count;
                cost += count * Config.Product(pair.Key).Price;
                if (Stock(pair.Key) < count) return true;
            }
            return cost > Wallet.Balance || State.Inventory.Count + missing > Config.FridgeCapacity;
        }
        public bool Recover(string[] foods)
        {
            if (!CanRecover(foods)) return false;
            // The rescue kit starts this order immediately; it does not grant sellable surplus or refill the wallet.
            var run = new AsadoRun { Recovery = true };
            foreach (var food in foods)
            {
                var unit = State.Inventory.Find(x => x.FoodId == food && x.FreshnessAt(State.FreshnessCycle, Config.Product(food).FreshCycles) != Freshness.Spoiled);
                if (unit != null) State.Inventory.Remove(unit);
                else unit = new InventoryUnit { Id = State.NextUnitId++, FoodId = food, AcquiredCycle = State.FreshnessCycle, Recovery = true };
                run.Units.Add(unit); run.FoodCost += unit.Cost;
            }
            State.ActiveRun = run; return true;
        }
        public EconomicResult Complete(float asador, int guestCount, StarThresholds thresholds)
        {
            if (State.ActiveRun == null) throw new InvalidOperationException("No active order; reward already claimed");
            var run = State.ActiveRun;
            int retainedCost = 0;
            foreach (var unit in State.Inventory) retainedCost += unit.Cost;
            float economy = Mathf.Clamp(100f - State.PendingWaste * Config.WasteEconomyPenalty / Math.Max(1, run.FoodCost + State.PendingWaste)
                - retainedCost * Config.SurplusPenalty / Math.Max(1, run.FoodCost), 0, 100);
            float operations = State.PendingWaste == 0 ? 100 : Mathf.Clamp(100f - State.PendingWaste * Config.WasteOperationsPenalty / Math.Max(1, run.FoodCost), 0, 100);
            float sum = Config.FoodWeight + Config.EconomyWeight + Config.OperationsWeight;
            float overall = (Mathf.Clamp(asador, 0, 100) * Config.FoodWeight + economy * Config.EconomyWeight + operations * Config.OperationsWeight) / sum;
            int reward = Mathf.RoundToInt((Config.BaseReward + Config.RewardPerGuest * guestCount) * Mathf.Lerp(Config.MinimumRewardRatio, 1f, overall / 100f) * (run.Recovery ? Config.RecoveryRewardMultiplier : 1));
            int stars = thresholds.Evaluate(Mathf.RoundToInt(overall * 2));
            if (run.Recovery) stars = Math.Min(stars, Config.RecoveryStarCap);
            Wallet.Credit(reward); State.TotalRewards += reward;
            var result = new EconomicResult { Asador = asador, Economy = economy, Operations = operations, Overall = overall,
                Spending = run.FoodCost, Waste = State.PendingWaste, Income = reward, Profit = reward - run.FoodCost - State.PendingWaste,
                Balance = Wallet.Balance, Stars = stars, Recovery = run.Recovery };
            State.ActiveRun = null; State.PendingWaste = 0; State.Cycle++; State.Purchases.Clear();
            // Freshness is data/model-ready but does not advance until Vertical 2 is enabled.
            if (Config.EnableFreshness) State.FreshnessCycle++;
            return result;
        }
        public void Abandon()
        {
            if (State.ActiveRun == null) return;
            State.TotalWaste += State.ActiveRun.FoodCost; State.PendingWaste += State.ActiveRun.FoodCost;
            State.ActiveRun = null;
        }
    }
}
