using System;
using System.Collections.Generic;
using UnityEngine;
namespace Asadito.Runtime
{
    public sealed class CartQuote
    {
        public int Total, Quantity;
        public string Error;
        public bool CanBuy => Error == null;
    }
    public sealed class EconomicResult
    {
        public float Asador, Economy, Operations, Overall;
        public int Spending, Waste, Income, Profit, Balance, Stars;
        public bool Recovery, Passed;
        public float Cooking, Satiety;
        public string Advice;
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
        // Quote is read-only. Checkout revalidates the complete basket before any debit or unit creation.
        public CartQuote QuoteCart(IReadOnlyDictionary<string, int> cart, int level, Promotion promotion = null)
        {
            var quote = new CartQuote();
            if (cart == null || cart.Count == 0) { quote.Error = "Elegí un corte para empezar"; return quote; }
            foreach (var pair in cart)
            {
                var product = Array.Find(Config.Products, p => p.FoodId == pair.Key);
                if (product == null || pair.Value < 1) { quote.Error = "Cantidad o corte inválido"; return quote; }
                try
                {
                    quote.Quantity = checked(quote.Quantity + pair.Value);
                    quote.Total = checked(quote.Total + (promotion ?? new Promotion()).Quote(product.Price, pair.Value));
                }
                catch (OverflowException) { quote.Error = "Cantidad demasiado grande"; return quote; }
            }
            if (State.ActiveRun != null) quote.Error = "Terminá el asado actual";
            else
            {
                foreach (var pair in cart)
                {
                    if (level < Config.Product(pair.Key).UnlockLevel) { quote.Error = "Corte bloqueado"; break; }
                    if (Stock(pair.Key) < pair.Value) { quote.Error = "Sin stock de " + FoodCatalog.Get(pair.Key).DisplayName; break; }
                }
                if (quote.Error == null && (long)State.Inventory.Count + quote.Quantity > Config.FridgeCapacity) quote.Error = "Heladera llena: quitá piezas del carrito";
                if (quote.Error == null && quote.Total > Wallet.Balance) quote.Error = "No alcanzan las monedas: quitá piezas del carrito";
            }
            return quote;
        }
        public string BuyCart(IReadOnlyDictionary<string, int> cart, int level, Promotion promotion = null)
        {
            var quote = QuoteCart(cart, level, promotion);
            if (!quote.CanBuy) return quote.Error;
            if (!Wallet.Spend(quote.Total)) return "No alcanzan las monedas";
            // Config order makes unit IDs and cost allocation independent from UI/dictionary order.
            foreach (var product in Config.Products)
            {
                if (!cart.TryGetValue(product.FoodId, out int quantity)) continue;
                int cost = (promotion ?? new Promotion()).Quote(product.Price, quantity);
                for (int i = 0; i < quantity; i++)
                    State.Inventory.Add(new InventoryUnit { Id = State.NextUnitId++, FoodId = product.FoodId,
                        Cost = cost / quantity + (i < cost % quantity ? 1 : 0), AcquiredCycle = State.FreshnessCycle });
                State.Purchases.Add(new PurchaseRecord { FoodId = product.FoodId, Quantity = quantity });
            }
            return null;
        }
        public string Buy(string id, int quantity, int level, Promotion promotion = null)
        {
            string error = BuyCart(new Dictionary<string, int> { [id] = quantity }, level, promotion);
            // Preserve the existing single-product API's short error contract.
            if (error != null && error.StartsWith("Sin stock de ")) return "Sin stock";
            if (error != null && error.StartsWith("Heladera llena")) return "Heladera llena";
            if (error != null && error.StartsWith("No alcanzan las monedas")) return "No alcanzan las monedas";
            return error;
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
            if (foods == null || foods.Length == 0 || State.ActiveRun != null) return false;
            var counts = new Dictionary<string, int>();
            foreach (var food in foods) { if (!counts.ContainsKey(food)) counts[food] = 0; counts[food]++; }
            foreach (var pair in counts) if (Available(pair.Key) < pair.Value) return false;
            return foods.Length > 0 && State.ActiveRun == null;
        }
        // Physical inventory selection validates stable IDs as one transaction; no reservation or new save field.
        public bool CanPrepareUnits(int level,IReadOnlyList<int> ids)
        {
            if(level<1||level>Config.PlayableLevels||ids==null||ids.Count==0||State.ActiveRun!=null)return false;
            var unique=new HashSet<int>();
            foreach(int id in ids)
            {
                if(!unique.Add(id))return false;
                var unit=State.Inventory.Find(x=>x.Id==id);
                if(unit==null)return false;
                var p=Array.Find(Config.Products,x=>x.FoodId==unit.FoodId);
                if(p==null||p.UnlockLevel>level||unit.FreshnessAt(State.FreshnessCycle,p.FreshCycles)==Freshness.Spoiled)return false;
            }
            return true;
        }
        public bool PrepareUnits(int level,IReadOnlyList<int> ids)
        {
            if(!CanPrepareUnits(level,ids))return false;
            var run=new AsadoRun{Level=level};
            foreach(int id in ids)
            {
                var unit=State.Inventory.Find(x=>x.Id==id);
                run.Units.Add(unit);run.FoodCost+=unit.Cost;run.Recovery|=unit.Recovery;
            }
            foreach(var unit in run.Units)State.Inventory.Remove(unit);
            State.ActiveRun=run;return true;
        }
        public bool Prepare(int level,string[] selected)
        {
            if(!CanPrepare(selected))return false;
            var ids=new List<int>();
            foreach(var food in selected)
            {
                var unit=State.Inventory.Find(x=>x.FoodId==food&&!ids.Contains(x.Id)&&x.FreshnessAt(State.FreshnessCycle,Config.Product(food).FreshCycles)!=Freshness.Spoiled);
                if(unit==null)return false;ids.Add(unit.Id);
            }
            return PrepareUnits(level,ids);
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
        public EconomicResult Complete(float asador, int guestCount, StarThresholds thresholds, float cooking = 100, float satiety = 100, int cookingWaste = 0)
        {
            if (State.ActiveRun == null) throw new InvalidOperationException("No active order; reward already claimed");
            var run = State.ActiveRun;
            cookingWaste = Mathf.Clamp(cookingWaste, 0, run.FoodCost);
            int waste = State.PendingWaste + cookingWaste;
            int retainedCost = 0;
            foreach (var unit in State.Inventory) retainedCost += unit.Cost;
            float economy = Mathf.Clamp(100f - waste * Config.WasteEconomyPenalty / Math.Max(1, run.FoodCost + State.PendingWaste)
                - retainedCost * Config.SurplusPenalty / Math.Max(1, run.FoodCost), 0, 100);
            float operations = waste == 0 ? 100 : Mathf.Clamp(100f - waste * Config.WasteOperationsPenalty / Math.Max(1, run.FoodCost), 0, 100);
            float sum = Config.FoodWeight + Config.EconomyWeight + Config.OperationsWeight;
            float overall = (Mathf.Clamp(asador, 0, 100) * Config.FoodWeight + economy * Config.EconomyWeight + operations * Config.OperationsWeight) / sum;
            int reward = Mathf.RoundToInt((Config.BaseReward + Config.RewardPerGuest * guestCount) * Mathf.Lerp(Config.MinimumRewardRatio, 1f, Mathf.Min(overall, Mathf.Clamp(asador, 0, 100), Mathf.Clamp(cooking, 0, 100)) / 100f) * (run.Recovery ? Config.RecoveryRewardMultiplier : 1));
            // Excellent bookkeeping cannot compensate for raw/burnt food or hungry guests.
            bool passed = asador >= Config.MinimumAsadorForStar && cooking >= Config.MinimumCookingForStar && satiety >= Config.MinimumSatietyForStar;
            int stars = passed ? thresholds.Evaluate(Mathf.RoundToInt(overall * 2)) : 0;
            if (asador < Config.TwoStarAsador) stars = Math.Min(stars, 1);
            else if (asador < Config.ThreeStarAsador) stars = Math.Min(stars, 2);
            // Recovery is a playable second chance, not a reward for serving uncooked food.
            if (run.Recovery && !passed) reward = 0;
            if (run.Recovery) stars = Math.Min(stars, Config.RecoveryStarCap);
            Wallet.Credit(reward); State.TotalRewards += reward; State.TotalWaste += cookingWaste;
            var result = new EconomicResult { Asador = asador, Economy = economy, Operations = operations, Overall = overall,
                Spending = run.FoodCost, Waste = waste, Income = reward, Profit = reward - run.FoodCost - State.PendingWaste,
                Balance = Wallet.Balance, Stars = stars, Recovery = run.Recovery, Passed = stars > 0, Cooking = cooking, Satiety = satiety,
                Advice = cooking < Config.MinimumCookingForStar ? "Había carne cruda o quemada. Revisá cada pieza antes de servir." :
                    satiety < Config.MinimumSatietyForStar ? "Faltó comida: revisá cantidades y hambre de tus invitados." :
                    asador < Config.MinimumAsadorForStar ? "Mejorá la cocción y respetá los puntos pedidos." :
                    waste > 0 ? "Buen asado. Evitá desperdicios para mejorar tu ganancia." :
                    retainedCost > 0 ? "Bien hecho. Usá lo que quedó en la heladera en el próximo asado." :
                    asador < Config.ThreeStarAsador ? "Buen asado. Acercate al punto y los gustos de cada invitado." : "¡Excelente! Cocinaste bien y cuidaste tus monedas." };
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
