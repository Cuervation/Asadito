using System;
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
            service.Buy("chorizo",2,5);service.Buy("tira",1,5);
            Assert.IsTrue(service.Prepare(5,new[]{"chorizo","tira"}));Assert.AreEqual(1,service.Available("chorizo"));
            Assert.IsNotNull(service.Buy("chorizo",1,5));
            int balance=state.Balance;var result=service.Complete(90,2,new StarThresholds());
            Assert.Greater(result.Income,0);Assert.AreEqual(balance+result.Income,state.Balance);
            Assert.AreEqual(result.Income-result.Spending-result.Waste,result.Profit);
            Assert.Throws<InvalidOperationException>(()=>service.Complete(90,2,new StarThresholds()));
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
            service.Buy("chorizo",2,5);service.Prepare(5,new[]{"chorizo"});service.Complete(80,1,new StarThresholds());
            Assert.AreEqual(1,state.Cycle);Assert.AreEqual(0,state.FreshnessCycle);Assert.AreEqual(config.Product("chorizo").Stock,service.Stock("chorizo"));
            config.EnableFreshness=true;service.Prepare(5,new[]{"chorizo"});service.Complete(80,1,new StarThresholds());Assert.AreEqual(1,state.FreshnessCycle);
        }
        [Test] public void RecoveryWorksWithZeroCoinsAndFullFridgeWithoutGrantingSurplus()
        {
            state.Balance=0;Assert.IsTrue(service.CanRecover(new[]{"tira","chorizo"}));
            Assert.IsTrue(service.Recover(new[]{"tira","chorizo"}));Assert.IsEmpty(state.Inventory);
            var result=service.Complete(100,2,new StarThresholds());Assert.AreEqual(config.RecoveryStarCap,result.Stars);
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
            service.Buy("chorizo",2,5);service.Discard(state.Inventory[0].Id);
            service.Prepare(5,new[]{"chorizo"});var result=service.Complete(100,1,new StarThresholds());
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
    }
}
