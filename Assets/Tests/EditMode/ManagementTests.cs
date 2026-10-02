using System;
using System.Collections.Generic;
using Asadito.Runtime;
using NUnit.Framework;
using UnityEngine;
namespace Asadito.Tests
{
    public sealed class ManagementTests
    {
        private ManagementConfig config;
        private ManagementState state;
        private ManagementService service;
        [SetUp] public void Setup()
        {
            config = JsonUtility.FromJson<ManagementConfig>(JsonUtility.ToJson(ManagementConfig.Load()));
            state = ManagementState.New(config); service = new ManagementService(state, config);
        }
        [Test] public void FullCatalogueHasUniquePositivePricesAndUnlocksForStockingAtLevelOne()
        {
            var foods=FoodCatalog.GetAll();var ids=new HashSet<string>();
            Assert.AreEqual(18,foods.Length);Assert.AreEqual(foods.Length,config.Products.Length);
            foreach(var food in foods)
            {
                var product=config.Product(food.Id);Assert.IsNotNull(product,food.Id);Assert.IsTrue(ids.Add(product.FoodId));
                Assert.Greater(product.Price,0);Assert.AreEqual(1,product.UnlockLevel);Assert.Greater(product.Stock,0);
                Assert.That(product.PortionAmount,Is.GreaterThan(0).And.LessThanOrEqualTo(1));
                Assert.IsTrue(service.QuoteCart(new Dictionary<string,int>{{food.Id,1}},1).CanBuy,food.Id+" may be stocked before appearing in an order");
            }
        }
        [Test] public void StockingNonOrderFoodsDoesNotReserveTheCurrentAsadoBudget()
        {
            Assert.That(MvpLevelCatalog.Get(1).FoodIds,Does.Not.Contain("lomo"));
            var cart=new Dictionary<string,int>{{"lomo",2}};Assert.IsTrue(service.QuoteCart(cart,1).CanBuy);
            Assert.IsNull(service.BuyCart(cart,1));Assert.AreEqual(10,state.Balance);Assert.AreEqual(2,service.Available("lomo"));
            var loaded=MvpSaveData.Migrate(JsonUtility.FromJson<MvpSaveData>(JsonUtility.ToJson(new MvpSaveData{Management=state}))).Management;
            Assert.AreEqual(10,loaded.Balance);Assert.AreEqual(2,loaded.Inventory.Count);
            Assert.AreEqual(state.Inventory[0].Id,loaded.Inventory[0].Id);Assert.AreEqual(state.Inventory[1].Id,loaded.Inventory[1].Id);
            string before=JsonUtility.ToJson(state);
            Assert.IsNotNull(service.BuyCart(new Dictionary<string,int>{{"chorizo",1},{"tira",1}},1));
            Assert.AreEqual(before,JsonUtility.ToJson(state),"Insufficient order budget is the player's consequence, not a reserved balance or partial debit");
        }
        [Test] public void SaveConstructionDoesNotLoadTheResourceBackedLevelCatalogue()
        {
            var cache=typeof(MvpLevelCatalog).GetField("cached",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
            var previous=cache.GetValue(null);cache.SetValue(null,null);
            try
            {
                var save=new MvpSaveData();Assert.IsNull(cache.GetValue(null),"Serializable constructors must not invoke Unity Resources");
                Assert.IsEmpty(save.StarsByLevel);Assert.IsEmpty(save.BestScoreByLevel);
                MvpSaveData.Migrate(save);Assert.AreEqual(MvpLevelCatalog.Count,save.StarsByLevel.Length);
                save.RecordLevelResult(1,130,2);Assert.AreEqual(2,save.StarsByLevel[0]);
            }
            finally{cache.SetValue(null,previous);}
        }
        [Test] public void AbundantCounterStockKeepsPricesBalanceCapacityAndPurchasesExact()
        {
            Assert.AreEqual(24,service.Stock("chorizo"));Assert.AreEqual(24,service.Stock("tira"));
            Assert.AreEqual(650,state.Balance);Assert.AreEqual(8,config.FridgeCapacity);
            Assert.AreEqual(100,config.Product("chorizo").Price);Assert.AreEqual(180,config.Product("tira").Price);
            Assert.IsNull(service.BuyCart(new Dictionary<string,int>{{"chorizo",1},{"tira",1}},1));
            Assert.AreEqual(370,state.Balance);Assert.AreEqual(23,service.Stock("chorizo"));Assert.AreEqual(23,service.Stock("tira"));
            Assert.AreEqual(2,state.Inventory.Count);
            string before=JsonUtility.ToJson(state);
            Assert.IsNotNull(service.BuyCart(new Dictionary<string,int>{{"chorizo",25}},1));Assert.AreEqual(before,JsonUtility.ToJson(state));
        }
        [Test] public void PhysicalPreparationConsumesExactIdsNotFirstMatchingSku()
        {
            state.Balance=2000;Assert.IsNull(service.BuyCart(new System.Collections.Generic.Dictionary<string,int>{{"chorizo",3},{"tira",2}},1));
            int chosen=state.Inventory[2].Id,tira=state.Inventory[4].Id,kept=state.Inventory[0].Id;
            Assert.IsTrue(service.PrepareUnits(1,new[]{chosen,tira}));
            Assert.AreEqual(chosen,state.ActiveRun.Units[0].Id);Assert.AreEqual(tira,state.ActiveRun.Units[1].Id);
            Assert.AreEqual(3,state.Inventory.Count);Assert.IsNotNull(state.Inventory.Find(u=>u.Id==kept));
            Assert.AreEqual(280,state.ActiveRun.FoodCost);Assert.IsFalse(service.PrepareUnits(1,new[]{chosen,tira}));
            var reloaded=JsonUtility.FromJson<ManagementState>(JsonUtility.ToJson(state));
            Assert.AreEqual(chosen,reloaded.ActiveRun.Units[0].Id);Assert.AreEqual(3,reloaded.Inventory.Count);
        }
        [TestCase("duplicate")] [TestCase("missing")] [TestCase("locked")] [TestCase("spoiled")] [TestCase("active")]
        public void PhysicalPreparationRejectsWholeSelectionAtomically(string reason)
        {
            Assert.IsNull(service.BuyCart(new System.Collections.Generic.Dictionary<string,int>{{"chorizo",1},{"tira",1}},1));
            int a=state.Inventory[0].Id,b=state.Inventory[1].Id;
            if(reason=="duplicate")b=a;
            if(reason=="missing")b=99999;
            if(reason=="locked")config.Product("tira").UnlockLevel=2;
            if(reason=="spoiled")state.FreshnessCycle=10;
            if(reason=="active")state.ActiveRun=new AsadoRun{Level=1};
            string before=JsonUtility.ToJson(state);
            Assert.IsFalse(service.CanPrepareUnits(1,new[]{a,b}));Assert.IsFalse(service.PrepareUnits(1,new[]{a,b}));
            Assert.AreEqual(before,JsonUtility.ToJson(state));
        }
        [Test] public void CartPreviewDoesNotMutateAndCheckoutTransfersWholeBasketPersistently()
        {
            var cart=new Dictionary<string,int>{{"tira",2},{"chorizo",2}};
            int next=state.NextUnitId;
            var quote=service.QuoteCart(cart,1);
            Assert.IsTrue(quote.CanBuy);Assert.AreEqual(560,quote.Total);Assert.AreEqual(4,quote.Quantity);
            Assert.IsEmpty(state.Inventory);Assert.IsEmpty(state.Purchases);Assert.AreEqual(config.InitialBalance,state.Balance);Assert.AreEqual(next,state.NextUnitId);
            Assert.IsNull(service.BuyCart(cart,1));
            Assert.AreEqual(config.InitialBalance-quote.Total,state.Balance);Assert.AreEqual(4,state.Inventory.Count);
            Assert.AreEqual(2,service.Available("tira"));Assert.AreEqual(config.Product("tira").Stock-2,service.Stock("tira"));
            int cost=0;foreach(var unit in state.Inventory) cost+=unit.Cost;Assert.AreEqual(560,cost);
            var reloaded=MvpSaveData.Migrate(JsonUtility.FromJson<MvpSaveData>(JsonUtility.ToJson(new MvpSaveData{Management=state}))).Management;
            Assert.AreEqual(state.Balance,reloaded.Balance);Assert.AreEqual(4,reloaded.Inventory.Count);Assert.AreEqual(state.NextUnitId,reloaded.NextUnitId);
            Assert.IsNull(reloaded.ActiveRun,"Empty serialized run must not block purchases/preparation");
            var loadedService=new ManagementService(reloaded,config);Assert.IsTrue(loadedService.CanPrepare(new[]{"tira","chorizo"}));
        }
        [TestCase("money")] [TestCase("stock")] [TestCase("capacity")] [TestCase("locked")] [TestCase("active")]
        public void CartFailureIsAtomicEvenWhenFirstProductCouldBeBought(string reason)
        {
            var cart=new Dictionary<string,int>{{"chorizo",1},{"tira",1}};
            if(reason=="money")state.Balance=200;
            if(reason=="stock")config.Product("tira").Stock=0;
            if(reason=="capacity")config.FridgeCapacity=1;
            if(reason=="locked")config.Product("tira").UnlockLevel=2;
            if(reason=="active")state.ActiveRun=new AsadoRun{Level=1};
            string before=JsonUtility.ToJson(state);
            var quote=service.QuoteCart(cart,1);Assert.IsFalse(quote.CanBuy);Assert.IsNotEmpty(quote.Error);
            Assert.IsNotNull(service.BuyCart(cart,1));Assert.AreEqual(before,JsonUtility.ToJson(state));
        }
        [Test] public void CartCheckoutRevalidatesChangedStockAndRejectsInvalidQuantities()
        {
            var cart=new Dictionary<string,int>{{"chorizo",1},{"tira",1}};
            Assert.IsTrue(service.QuoteCart(cart,1).CanBuy);
            config.FridgeCapacity=Math.Max(config.FridgeCapacity,config.Product("tira").Stock);
            Assert.IsNull(service.Buy("tira",config.Product("tira").Stock,1,new Promotion{Kind=PromotionKind.Percentage,Percent=100}));
            string before=JsonUtility.ToJson(state);Assert.IsNotNull(service.BuyCart(cart,1));Assert.AreEqual(before,JsonUtility.ToJson(state));
            foreach(var invalid in new[]{new Dictionary<string,int>(),new Dictionary<string,int>{{"chorizo",0}},new Dictionary<string,int>{{"unknown",1}},new Dictionary<string,int>{{"tira",int.MaxValue}}})
            {Assert.IsNotNull(service.BuyCart(invalid,1));Assert.AreEqual(before,JsonUtility.ToJson(state));}
        }
        [Test] public void WalletIsPersistentAndRejectsNegativeOrInsufficientSpending()
        {
            Assert.IsFalse(service.Wallet.Spend(-1)); Assert.IsFalse(service.Wallet.Spend(config.InitialBalance+1));
            Assert.AreEqual(config.InitialBalance, state.Balance);
            Assert.IsTrue(service.Wallet.Spend(50));service.Wallet.Credit(10);Assert.AreEqual(config.InitialBalance-40,state.Balance);
            Assert.Throws<ArgumentOutOfRangeException>(()=>service.Wallet.Credit(-1));
        }
        [Test] public void PurchaseRecordsExactCostAndInventoryWithoutDuplicatingFoodDefinitions()
        {
            Assert.IsNull(service.Buy("tira",2,5));Assert.AreEqual(2,service.Available("tira"));
            Assert.AreEqual(config.InitialBalance-config.Product("tira").Price*2,state.Balance);
            Assert.AreEqual(config.Product("tira").Stock-2,service.Stock("tira"));
            Assert.AreNotEqual(state.Inventory[0].Id,state.Inventory[1].Id);
        }
        [Test] public void InsufficientFundsIsAtomic()
        {
            state.Balance=0; Assert.IsNotNull(service.Buy("tira",1,5));
            Assert.IsEmpty(state.Inventory);Assert.IsEmpty(state.Purchases);Assert.AreEqual(0,state.Balance);
        }
        [Test] public void CapacityAndStockFailuresDoNotSpend()
        {
            config.FridgeCapacity=2;Assert.IsNull(service.Buy("chorizo",2,5));int balance=state.Balance;
            Assert.AreEqual("Heladera llena",service.Buy("tira",1,5));Assert.AreEqual(balance,state.Balance);
            config.FridgeCapacity=20;config.Product("tira").Stock=0;
            Assert.AreEqual("Sin stock",service.Buy("tira",1,5));Assert.AreEqual(balance,state.Balance);
        }
        [Test] public void PromotionQuotesHandleRemaindersAndExactInventoryCost()
        {
            Assert.AreEqual(200,new Promotion { Kind=PromotionKind.BuyNPayM,Buy=2,Pay=1 }.Quote(100,3));
            Assert.AreEqual(400,new Promotion { Kind=PromotionKind.BuyNPayM,Buy=3,Pay=2 }.Quote(100,5));
            Assert.AreEqual(150,new Promotion { Kind=PromotionKind.Percentage,Percent=25 }.Quote(100,2));
            Assert.AreEqual(250,new Promotion { Kind=PromotionKind.Pack,PackQuantity=2,PackPrice=150 }.Quote(100,3));
            Assert.IsNull(service.Buy("chorizo",3,5,new Promotion {Kind=PromotionKind.Percentage,Percent=17}));
            int total=0;foreach(var unit in state.Inventory)total+=unit.Cost;Assert.AreEqual(249,total);
        }
        [Test] public void PreparationConsumesSelectedUnitsOnlyAndRewardIsClaimedOnce()
        {
            service.Buy("chorizo",2,1);service.Buy("tira",1,1);
            Assert.IsTrue(service.Prepare(1,new[]{"chorizo","tira"}));Assert.AreEqual(1,service.Available("chorizo"));
            Assert.IsNotNull(service.Buy("chorizo",1,1));
            int balance=state.Balance;var result=service.Complete(90,2,new StarThresholds());
            Assert.Greater(result.Income,0);Assert.AreEqual(balance+result.Income,state.Balance);
            Assert.IsTrue(result.Order.IsComplete);Assert.IsTrue(result.Passed);
            Assert.AreEqual(result.Income-result.Spending-result.Waste,result.Profit);
            Assert.Throws<InvalidOperationException>(()=>service.Complete(90,2,new StarThresholds()));
        }
        [TestCase(1,"morcilla,morcilla",0,2,2)]
        [TestCase(1,"chorizo,chorizo",1,1,1)]
        [TestCase(1,"tira",1,1,0)]
        [TestCase(1,"tira,chorizo,morcilla",2,0,1)]
        [TestCase(2,"tira,tira,chorizo",2,1,1)]
        public void PerfectScoresCannotApproveWrongMissingOrExtraMenu(int level,string servedCsv,int matched,int missing,int unexpected)
        {
            string[] selected=servedCsv.Split(',');
            foreach(string food in selected)Assert.IsNull(service.Buy(food,1,level));
            int[] selectedIds=state.Inventory.ConvertAll(unit=>unit.Id).ToArray();
            Assert.IsTrue(service.PrepareUnits(level,selectedIds),"Preparing paid alternative cuts remains free");
            CollectionAssert.AreEqual(selectedIds,state.ActiveRun.Units.ConvertAll(unit=>unit.Id));
            int balance=state.Balance,cost=state.ActiveRun.FoodCost;
            var result=service.Complete(100,MvpLevelCatalog.Get(level).GuestCount,new StarThresholds(),100,100);
            Assert.AreEqual(MvpLevelCatalog.Get(level).FoodIds.Length,result.Order.RequiredCount);
            Assert.AreEqual(matched,result.Order.MatchedCount);
            int missingCount=0,unexpectedCount=0;
            foreach(int quantity in result.Order.MissingByFood.Values)missingCount+=quantity;
            foreach(int quantity in result.Order.UnexpectedByFood.Values)unexpectedCount+=quantity;
            Assert.AreEqual(missing,missingCount);Assert.AreEqual(unexpected,unexpectedCount);
            Assert.IsFalse(result.Order.IsComplete);Assert.IsFalse(result.Passed);Assert.AreEqual(0,result.Stars);
            Assert.That(result.Advice.ToLowerInvariant(),Does.Contain("pedido"));
            Assert.AreEqual(100,result.Asador);Assert.AreEqual(100,result.Cooking);Assert.AreEqual(100,result.Satiety);
            int expectedIncome=config.BaseReward+config.RewardPerGuest*MvpLevelCatalog.Get(level).GuestCount;
            Assert.AreEqual(expectedIncome,result.Income,"A failed menu does not rewrite the existing paid reward formula");
            Assert.AreEqual(balance+expectedIncome,state.Balance);Assert.AreEqual(cost,result.Spending);
            Assert.IsEmpty(state.Inventory);Assert.IsNull(state.ActiveRun);Assert.AreEqual(1,state.Cycle);
            string completed=JsonUtility.ToJson(state);
            Assert.Throws<InvalidOperationException>(()=>service.Complete(100,2,new StarThresholds()));
            Assert.AreEqual(completed,JsonUtility.ToJson(state),"Repeated completion must not credit or consume anything twice");
        }
        [TestCase(1,"chorizo,tira")]
        [TestCase(2,"tira,chorizo,chorizo")]
        public void CompleteCanonicalMenuPassesRegardlessOfPreparationOrder(int level,string selectedCsv)
        {
            string[] selected=selectedCsv.Split(',');
            foreach(string food in selected)Assert.IsNull(service.Buy(food,1,level));
            Assert.IsTrue(service.Prepare(level,selected));
            var result=service.Complete(100,MvpLevelCatalog.Get(level).GuestCount,new StarThresholds());
            Assert.IsTrue(result.Order.IsComplete);Assert.AreEqual(selected.Length,result.Order.RequiredCount);
            Assert.AreEqual(selected.Length,result.Order.MatchedCount);Assert.IsEmpty(result.Order.MissingByFood);
            Assert.IsEmpty(result.Order.UnexpectedByFood);Assert.IsTrue(result.Passed);Assert.AreEqual(3,result.Stars);
            int balance=state.Balance,rewards=state.TotalRewards;
            Assert.Throws<InvalidOperationException>(()=>service.Complete(100,MvpLevelCatalog.Get(level).GuestCount,new StarThresholds()));
            Assert.AreEqual(balance,state.Balance);Assert.AreEqual(rewards,state.TotalRewards);
        }
        [Test] public void CorrectFoodKeptInFridgeCannotFulfillPreparedMorcillaOrder()
        {
            Assert.IsNull(service.BuyCart(new Dictionary<string,int>{{"tira",1},{"chorizo",1},{"morcilla",2}},1));
            var kept=state.Inventory.FindAll(unit=>unit.FoodId!="morcilla").ConvertAll(unit=>unit.Id).ToArray();
            var selected=state.Inventory.FindAll(unit=>unit.FoodId=="morcilla").ConvertAll(unit=>unit.Id).ToArray();
            Assert.IsTrue(service.PrepareUnits(1,selected));Assert.AreEqual(180,state.ActiveRun.FoodCost);
            var result=service.Complete(100,2,new StarThresholds());
            Assert.IsFalse(result.Order.IsComplete);Assert.AreEqual(0,result.Order.MatchedCount);
            Assert.AreEqual(1,result.Order.MissingByFood["tira"]);Assert.AreEqual(1,result.Order.MissingByFood["chorizo"]);
            Assert.AreEqual(2,result.Order.UnexpectedByFood["morcilla"]);Assert.IsFalse(result.Passed);
            CollectionAssert.AreEqual(kept,state.Inventory.ConvertAll(unit=>unit.Id));
            var reloaded=MvpSaveData.Migrate(JsonUtility.FromJson<MvpSaveData>(JsonUtility.ToJson(new MvpSaveData{Management=state}))).Management;
            Assert.AreEqual(state.Balance,reloaded.Balance);CollectionAssert.AreEqual(kept,reloaded.Inventory.ConvertAll(unit=>unit.Id));
            Assert.IsNull(reloaded.ActiveRun);Assert.AreEqual(state.TotalRewards,reloaded.TotalRewards);
        }
        [Test] public void UnknownRunLevelCannotApproveOtherwiseCorrectMenu()
        {
            service.Buy("tira",1,1);service.Buy("chorizo",1,1);Assert.IsTrue(service.Prepare(1,new[]{"tira","chorizo"}));
            state.ActiveRun.Level=0;
            var result=service.Complete(100,2,new StarThresholds());
            Assert.IsFalse(result.Order.IsComplete);Assert.AreEqual(0,result.Stars);Assert.IsFalse(result.Passed);
            Assert.That(result.Advice.ToLowerInvariant(),Does.Contain("pedido"));
        }
        [Test] public void FreshnessUsesJourneysNotClockAndSpoiledCannotPrepare()
        {
            service.Buy("chorizo",1,5);var unit=state.Inventory[0];int life=config.Product("chorizo").FreshCycles;
            Assert.AreEqual(Freshness.Fresh,unit.FreshnessAt(0,life));
            Assert.AreEqual(Freshness.ConsumeSoon,unit.FreshnessAt(life-1,life));
            state.FreshnessCycle=life;Assert.AreEqual(Freshness.Spoiled,unit.FreshnessAt(state.FreshnessCycle,life));
            Assert.IsFalse(service.CanPrepare(new[]{"chorizo"}));Assert.IsFalse(service.Prepare(5,new[]{"chorizo"}));
            Assert.IsTrue(service.Discard(unit.Id));Assert.AreEqual(config.Product("chorizo").Price,state.TotalWaste);
        }
        [Test] public void JourneyAdvancesStockButFreshnessIsDeferredInVerticalOne()
        {
            service.Buy("chorizo",2,1);service.Buy("tira",2,1);
            service.Prepare(1,new[]{"chorizo","tira"});service.Complete(80,2,new StarThresholds());
            Assert.AreEqual(1,state.Cycle);Assert.AreEqual(0,state.FreshnessCycle);Assert.AreEqual(config.Product("chorizo").Stock,service.Stock("chorizo"));
            config.EnableFreshness=true;service.Prepare(1,new[]{"chorizo","tira"});service.Complete(80,2,new StarThresholds());Assert.AreEqual(1,state.FreshnessCycle);
        }
        [Test] public void RecoveryWorksWithZeroCoinsAndFullFridgeWithoutGrantingSurplus()
        {
            state.Balance=0;Assert.IsTrue(service.CanRecover(new[]{"tira","chorizo"}));
            Assert.IsTrue(service.Recover(new[]{"tira","chorizo"}));Assert.IsEmpty(state.Inventory);
            state.ActiveRun.Level=1;
            var result=service.Complete(100,2,new StarThresholds());Assert.IsTrue(result.Order.IsComplete);Assert.AreEqual(config.RecoveryStarCap,result.Stars);
            Assert.Greater(state.Balance,0);Assert.IsFalse(service.Recover(Array.Empty<string>()));
        }
        [Test] public void FullFridgeRescueConsumesOwnedFoodAndOnlySuppliesMissingFood()
        {
            state.Balance = 10000;
            Assert.IsNull(service.Buy("chorizo",8,5));
            Assert.IsTrue(service.CanRecover(new[]{"chorizo","tira"}));
            Assert.IsTrue(service.Recover(new[]{"chorizo","tira"}));
            Assert.AreEqual(7, state.Inventory.Count);
            Assert.IsFalse(state.ActiveRun.Units[0].Recovery);
            Assert.IsTrue(state.ActiveRun.Units[1].Recovery);
            Assert.AreEqual(config.Product("chorizo").Price, state.ActiveRun.FoodCost);
        }

        [Test] public void RecoveryCannotBeFarmedWhenAnOrderIsAffordable()
        {
            Assert.IsFalse(service.CanRecover(new[]{"chorizo"}));Assert.IsFalse(service.Recover(new[]{"chorizo"}));Assert.IsNull(state.ActiveRun);
        }
        [Test] public void WasteAndUnnecessaryPurchasesHaveGentleConfigurableScoreEffect()
        {
            service.Buy("chorizo",2,1);service.Buy("tira",1,1);service.Discard(state.Inventory[0].Id);
            service.Prepare(1,new[]{"chorizo","tira"});var result=service.Complete(100,2,new StarThresholds());
            Assert.IsTrue(result.Order.IsComplete);
            Assert.Less(result.Economy,100);Assert.Less(result.Operations,100);Assert.GreaterOrEqual(result.Overall,60);
            Assert.AreEqual(result.Asador*config.FoodWeight+result.Economy*config.EconomyWeight+result.Operations*config.OperationsWeight,result.Overall,.001);
        }
        [Test] public void AbandonRecordsLossAndDoesNotRewardOrDuplicateUnits()
        {
            service.Buy("tira",1,5);service.Prepare(5,new[]{"tira"});int balance=state.Balance;
            service.Abandon();service.Abandon();Assert.AreEqual(config.Product("tira").Price,state.TotalWaste);
            Assert.AreEqual(balance,state.Balance);Assert.IsEmpty(state.Inventory);Assert.IsNull(state.ActiveRun);
        }
        [Test] public void SaveMigrationPreservesOldProgressSettingsAndInitializesEconomyOnlyOnce()
        {
            var old=new MvpSaveData { Version=3,MaxUnlockedLevel=6,StarsByLevel=new[]{3,2},BestScoreByLevel=new[]{190,170},Settings=new MvpSettings{SfxVolume=.2f,HapticsEnabled=false} };
            var migrated=MvpSaveData.Migrate(old);Assert.AreEqual(MvpSaveData.CurrentVersion,migrated.Version);
            Assert.AreEqual(6,migrated.MaxUnlockedLevel);Assert.AreEqual(3,migrated.StarsByLevel[0]);Assert.AreEqual(170,migrated.BestScoreByLevel[1]);Assert.AreEqual(.2f,migrated.Settings.SfxVolume);
            migrated.Management.Balance=17;Assert.AreEqual(17,MvpSaveData.Migrate(migrated).Management.Balance);
            var roundtrip=MvpSaveData.Migrate(JsonUtility.FromJson<MvpSaveData>(JsonUtility.ToJson(migrated)));Assert.AreEqual(17,roundtrip.Management.Balance);
        }
        [Test] public void InvalidPreparationAndLockedProductsNeverConsumeInventory()
        {
            Assert.IsFalse(service.CanPrepare(null));Assert.IsFalse(service.CanPrepare(Array.Empty<string>()));
            service.Buy("chorizo",1,1);int balance=state.Balance;
            Assert.IsFalse(service.Prepare(7,new[]{"chorizo"}));
            config.Product("chorizo").UnlockLevel=2;
            Assert.IsFalse(service.Prepare(1,new[]{"chorizo"}));
            Assert.IsNull(state.ActiveRun);Assert.AreEqual(1,state.Inventory.Count);Assert.AreEqual(balance,state.Balance);
        }
        [Test] public void EveryInitialLevelUsesPaidManagementAndCanBuyItsRecipe()
        {
            Assert.AreEqual(1,config.ManagementFromLevel);Assert.AreEqual(1,config.CoinsFromLevel);
            Assert.IsFalse(config.EnableFreshness);Assert.AreEqual(6,config.PlayableLevels);
            for(int level=1;level<=6;level++)
            {
                var order=MvpLevelCatalog.Get(level);
                Assert.IsNotEmpty(order.LearningGoal);
                foreach(var id in order.FoodIds) Assert.IsNull(service.Buy(id,1,level));
                Assert.IsTrue(service.Prepare(level,order.FoodIds));
                var result=service.Complete(85,order.GuestCount,new StarThresholds());
                Assert.Greater(result.Income,0);Assert.GreaterOrEqual(result.Stars,1);
                Assert.GreaterOrEqual(result.Profit,0,"A good introductory asado should be profitable: "+level);
            }
        }
        [TestCase(20,100,100)] [TestCase(80,0,100)] [TestCase(80,100,20)]
        public void PerfectManagementCannotPassBadCookingOrHungryGuests(float asador,float cooking,float satiety)
        {
            service.Buy("tira",1,1);service.Buy("chorizo",1,1);service.Prepare(1,new[]{"tira","chorizo"});
            var result=service.Complete(asador,2,new StarThresholds(),cooking,satiety);
            Assert.IsTrue(result.Order.IsComplete,"This regression must isolate culinary gates, not a missing menu item");
            Assert.AreEqual(0,result.Stars);Assert.IsFalse(result.Passed);Assert.IsNotEmpty(result.Advice);
        }
        [TestCase(55,1)] [TestCase(70,2)] [TestCase(85,3)]
        public void HigherStarsRequireGoodAsadorNotOnlyEconomicScore(float asador,int expected)
        {
            service.Buy("chorizo",1,1);service.Buy("tira",1,1);service.Prepare(1,new[]{"chorizo","tira"});
            var result=service.Complete(asador,2,new StarThresholds());
            Assert.IsTrue(result.Order.IsComplete);Assert.AreEqual(expected,result.Stars);
        }
        [Test] public void FailedRecoveryCannotGenerateCoinsAndRemainsPlayable()
        {
            state.Balance=0;service.Recover(new[]{"chorizo","tira"});state.ActiveRun.Level=1;
            var result=service.Complete(30,2,new StarThresholds(),0,100);
            Assert.IsTrue(result.Order.IsComplete);
            Assert.AreEqual(0,result.Income);Assert.AreEqual(0,state.Balance);
            Assert.IsTrue(service.CanRecover(new[]{"chorizo","tira"}));
        }
        [Test] public void CookingWasteIsRecordedButNeverDeductedTwice()
        {
            service.Buy("tira",1,1);service.Buy("chorizo",1,1);
            service.Prepare(1,new[]{"tira","chorizo"});int cost=config.Product("tira").Price;
            var result=service.Complete(20,2,new StarThresholds(),0,100,cost);
            Assert.IsTrue(result.Order.IsComplete);
            Assert.AreEqual(cost,result.Waste);Assert.AreEqual(cost,state.TotalWaste);
            Assert.AreEqual(config.Product("tira").Price+config.Product("chorizo").Price,result.Spending);
            Assert.AreEqual(result.Income-result.Spending,result.Profit,"Cooking waste was already paid in food cost, never subtract it twice");
            Assert.Less(result.Economy,100);Assert.Less(result.Operations,100);
        }
        [Test] public void V4MigrationPreservesBonusWalletInventoryRunAndProgressWithoutNewMoney()
        {
            service.Buy("chorizo",2,1);service.Prepare(1,new[]{"chorizo"});state.IntroRewardGranted=true;
            state.Cycle=2;
            var old=new MvpSaveData{Version=4,MaxUnlockedLevel=8,Management=state,StarsByLevel=new[]{3}};
            int balance=state.Balance,id=state.ActiveRun.Units[0].Id;
            var migrated=MvpSaveData.Migrate(JsonUtility.FromJson<MvpSaveData>(JsonUtility.ToJson(old)));
            Assert.AreEqual(balance,migrated.Management.Balance);Assert.IsTrue(migrated.Management.IntroRewardGranted);
            Assert.IsTrue(migrated.Management.ManagementTutorialCompleted);Assert.AreEqual(8,migrated.MaxUnlockedLevel);
            Assert.AreEqual(3,migrated.StarsByLevel[0]);Assert.AreEqual(1,migrated.Management.Inventory.Count);
            Assert.AreEqual(id,migrated.Management.ActiveRun.Units[0].Id);
            Assert.AreEqual(balance,MvpSaveData.Migrate(migrated).Management.Balance);
        }
        [Test] public void LegacyCookingTutorialDoesNotSkipNewPurchaseGuideOrGrantBonus()
        {
            var old=new MvpSaveData{Version=4,Management=state};old.Settings.TutorialCompleted=true;
            int balance=state.Balance;
            var migrated=MvpSaveData.Migrate(old);
            Assert.IsFalse(migrated.Management.ManagementTutorialCompleted);Assert.AreEqual(balance,migrated.Management.Balance);
        }
        [Test] public void ReplayingDebutUsesRetainedInventoryAndActualMoneyWithoutResetOrBonus()
        {
            service.Buy("chorizo",2,1);service.Buy("tira",1,1);
            service.Prepare(1,new[]{"chorizo","tira"});var first=service.Complete(90,2,new StarThresholds());
            int balance=state.Balance;Assert.AreEqual(1,service.Available("chorizo"));
            Assert.IsNull(service.Buy("tira",1,1));Assert.AreEqual(balance-config.Product("tira").Price,state.Balance);
            service.Prepare(1,new[]{"chorizo","tira"});var second=service.Complete(90,2,new StarThresholds());
            Assert.LessOrEqual(second.Income,config.BaseReward+2*config.RewardPerGuest);
            Assert.AreEqual(balance-config.Product("tira").Price+second.Income,state.Balance);Assert.IsFalse(state.IntroRewardGranted);
            Assert.Throws<InvalidOperationException>(()=>service.Complete(90,2,new StarThresholds()));
        }
    }
}
