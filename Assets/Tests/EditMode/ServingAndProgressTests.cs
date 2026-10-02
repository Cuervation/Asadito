using NUnit.Framework;
using UnityEngine;

namespace Asadito.Tests
{
    public sealed class ServingAndProgressTests
    {
        [Test]
        public void ServingAllocator_IsDeterministic_AndBalancesSatietyPreferenceAndPoint()
        {
            var ana = new Asadito.Runtime.GuestProfile { Id = "ana", TargetFoodAmount = .1f };
            ana.FavoriteFoods.Add("tira");
            var tito = new Asadito.Runtime.GuestProfile { Id = "tito", TargetFoodAmount = .1f };
            tito.FavoriteFoods.Add("chorizo");
            var portions = new[]
            {
                new Asadito.Runtime.ServingPortion { Id = "01", FoodId = "chorizo", Amount = .1f },
                new Asadito.Runtime.ServingPortion { Id = "02", FoodId = "tira", Amount = .1f }
            };

            var first = Asadito.Runtime.ServingAllocator.Allocate(new[] { ana, tito }, portions);
            var second = Asadito.Runtime.ServingAllocator.Allocate(new[] { ana, tito }, portions);
            Assert.AreEqual(2, first.Count);
            Assert.AreEqual(first[0].GuestId, second[0].GuestId);
            Assert.AreEqual(first[1].GuestId, second[1].GuestId);
            Assert.AreEqual("tito", first[0].GuestId);
            Assert.AreEqual("ana", first[1].GuestId);
        }

        [Test]
        public void DebutMenuMatchesPreferencesInsteadOfGivingLargestMeatToHungriestGuest()
        {
            var guests=Asadito.Runtime.MvpLevelCatalog.CreateGuests(1);
            var portions=new[]{
                new Asadito.Runtime.ServingPortion{Id="01",FoodId="tira",Amount=.14f,Doneness=Asadito.Runtime.Doneness.Jugoso},
                new Asadito.Runtime.ServingPortion{Id="02",FoodId="chorizo",Amount=.10f,Doneness=Asadito.Runtime.Doneness.A_Punto}
            };
            var assigned=Asadito.Runtime.ServingAllocator.Allocate(guests,portions);
            Assert.AreEqual("ana",assigned[0].GuestId);Assert.AreEqual("tito",assigned[1].GuestId);
        }

        [Test]
        public void OrderFulfillment_CountsRepeatedFoodsWithoutTreatingMenuAsSet()
        {
            var requested=new[]{"chorizo","tira","chorizo"};
            var served=new[]{"tira","tira","chorizo"};
            var result=Asadito.Runtime.OrderFulfillment.Evaluate(requested,served);
            Assert.AreEqual(3,result.RequiredCount);Assert.AreEqual(2,result.MatchedCount);
            Assert.IsFalse(result.IsComplete);Assert.AreEqual(1,result.MissingByFood.Count);
            Assert.AreEqual(1,result.MissingByFood["chorizo"]);Assert.AreEqual(1,result.UnexpectedByFood.Count);
            Assert.AreEqual(1,result.UnexpectedByFood["tira"]);
            CollectionAssert.AreEqual(new[]{"chorizo","tira","chorizo"},requested);
            CollectionAssert.AreEqual(new[]{"tira","tira","chorizo"},served);
        }

        [Test]
        public void OrderFulfillment_ExactCountsIgnoreServingOrderAndDoNotMutateInputs()
        {
            var requested=new[]{"chorizo","tira","chorizo"};
            var served=new[]{"chorizo","chorizo","tira"};
            var result=Asadito.Runtime.OrderFulfillment.Evaluate(requested,served);
            Assert.IsTrue(result.IsComplete);Assert.AreEqual(3,result.RequiredCount);Assert.AreEqual(3,result.MatchedCount);
            Assert.IsEmpty(result.MissingByFood);Assert.IsEmpty(result.UnexpectedByFood);
            CollectionAssert.AreEqual(new[]{"chorizo","tira","chorizo"},requested);
            CollectionAssert.AreEqual(new[]{"chorizo","chorizo","tira"},served);
        }

        [Test]
        public void OrderFulfillment_SubstitutionAndUnknownFoodCannotCoverMissingRequestedFoods()
        {
            var result=Asadito.Runtime.OrderFulfillment.Evaluate(new[]{"tira","chorizo"},new[]{"morcilla","unknown"});
            Assert.IsFalse(result.IsComplete);Assert.AreEqual(2,result.RequiredCount);Assert.AreEqual(0,result.MatchedCount);
            Assert.AreEqual(1,result.MissingByFood["tira"]);Assert.AreEqual(1,result.MissingByFood["chorizo"]);
            Assert.AreEqual(1,result.UnexpectedByFood["morcilla"]);Assert.AreEqual(1,result.UnexpectedByFood["unknown"]);
        }

        [Test]
        public void OrderFulfillment_ExtraFoodFailsEvenWhenEveryRequestedFoodIsPresent()
        {
            var result=Asadito.Runtime.OrderFulfillment.Evaluate(new[]{"tira","chorizo"},new[]{"tira","chorizo","morcilla"});
            Assert.IsFalse(result.IsComplete);Assert.AreEqual(2,result.MatchedCount);Assert.IsEmpty(result.MissingByFood);
            Assert.AreEqual(1,result.UnexpectedByFood["morcilla"]);
        }

        [Test]
        public void OrderFulfillment_EmptyOrUnknownRequestNeverCompletesAndMissingServingsAreReported()
        {
            Assert.IsFalse(Asadito.Runtime.OrderFulfillment.Evaluate(null,new[]{"tira"}).IsComplete);
            Assert.IsFalse(Asadito.Runtime.OrderFulfillment.Evaluate(System.Array.Empty<string>(),System.Array.Empty<string>()).IsComplete);
            var missing=Asadito.Runtime.OrderFulfillment.Evaluate(new[]{"tira","chorizo"},null);
            Assert.IsFalse(missing.IsComplete);Assert.AreEqual(2,missing.RequiredCount);Assert.AreEqual(0,missing.MatchedCount);
            Assert.AreEqual(1,missing.MissingByFood["tira"]);Assert.AreEqual(1,missing.MissingByFood["chorizo"]);
            Assert.IsEmpty(missing.UnexpectedByFood);
        }

        [Test]
        public void RequestedFoodIdentityOverridesFavoritesAndPortionOrder()
        {
            var ana=new Asadito.Runtime.GuestProfile{Id="ana",RequestedFoodId="chorizo",TargetFoodAmount=.1f};
            ana.FavoriteFoods.Add("tira");
            var tito=new Asadito.Runtime.GuestProfile{Id="tito",RequestedFoodId="tira",TargetFoodAmount=.1f};
            tito.FavoriteFoods.Add("chorizo");
            var chorizo=new Asadito.Runtime.ServingPortion{Id="02",FoodId="chorizo",Amount=.1f};
            var tira=new Asadito.Runtime.ServingPortion{Id="01",FoodId="tira",Amount=.1f};
            var first=Asadito.Runtime.ServingAllocator.Allocate(new[]{ana,tito},new[]{chorizo,tira});
            var reordered=Asadito.Runtime.ServingAllocator.Allocate(new[]{tito,ana},new[]{tira,chorizo});
            foreach(var assignments in new[]{first,reordered})
            {
                Assert.AreEqual(2,assignments.Count);
                Assert.AreEqual("02",assignments.Find(assignment=>assignment.GuestId=="ana").PortionId);
                Assert.AreEqual("01",assignments.Find(assignment=>assignment.GuestId=="tito").PortionId);
            }
        }

        [Test]
        public void RequestedGuestsRemainUnservedRatherThanReceivingSubstitutions()
        {
            var guests=Asadito.Runtime.MvpLevelCatalog.CreateGuests(1);
            var wrong=new[]{
                new Asadito.Runtime.ServingPortion{Id="01",FoodId="morcilla",Amount=1f},
                new Asadito.Runtime.ServingPortion{Id="02",FoodId="morcilla",Amount=1f}
            };
            Assert.IsEmpty(Asadito.Runtime.ServingAllocator.Allocate(guests,wrong),"Large favorite portions cannot replace an explicit order");
            wrong[0].FoodId="tira";
            var partial=Asadito.Runtime.ServingAllocator.Allocate(guests,wrong);
            Assert.AreEqual(1,partial.Count);Assert.AreEqual("ana",partial[0].GuestId);Assert.AreEqual("01",partial[0].PortionId);
        }

        [Test]
        public void MvpGuestsDeriveRequestedFoodFromCanonicalMenuWithoutRewritingPreferences()
        {
            for(int level=1;level<=Asadito.Runtime.MvpLevelCatalog.Count;level++)
            {
                var order=Asadito.Runtime.MvpLevelCatalog.Get(level);
                var guests=Asadito.Runtime.MvpLevelCatalog.CreateGuests(level);
                for(int i=0;i<guests.Length;i++)Assert.AreEqual(order.FoodIds[i],guests[i].RequestedFoodId,"Level "+level+", guest "+i);
            }
            var debut=Asadito.Runtime.MvpLevelCatalog.CreateGuests(1);
            var second=Asadito.Runtime.MvpLevelCatalog.CreateGuests(2);
            Assert.AreEqual("tira",debut[0].RequestedFoodId);Assert.AreEqual("chorizo",second[0].RequestedFoodId);
            CollectionAssert.AreEqual(debut[0].FavoriteFoods,second[0].FavoriteFoods,"A request is not a preference-list mutation");
            Assert.That(second[0].FavoriteFoods,Does.Contain("tira"));Assert.That(second[0].FavoriteFoods,Does.Not.Contain("chorizo"));
            second[0].RequestedFoodId="mutated";second[0].FavoriteFoods.Clear();
            var fresh=Asadito.Runtime.MvpLevelCatalog.CreateGuests(2);
            Assert.AreEqual("chorizo",fresh[0].RequestedFoodId);Assert.That(fresh[0].FavoriteFoods,Does.Contain("tira"));
        }

        [Test]
        public void ScoreBreakdown_UsesCentralizedWeights()
        {
            var config = new Asadito.Runtime.ScoreConfig();
            var breakdown = new Asadito.Runtime.ScoreBreakdown
            {
                CookingQuality = 100f,
                Satiety = 0f,
                DonenessMatch = 0f,
                FoodPreference = 0f
            };
            Assert.AreEqual(40f, breakdown.Total(config), .01f);

            breakdown.CookingQuality = breakdown.Satiety = breakdown.DonenessMatch = breakdown.FoodPreference = 100f;
            Assert.AreEqual(100f, breakdown.Total(config), .01f, "One guest's score must not exceed 100.");
        }

        [Test]
        public void StarThresholds_AreConfigurable()
        {
            var thresholds = new Asadito.Runtime.StarThresholds { OneStar = 20, TwoStars = 70, ThreeStars = 120 };
            Assert.AreEqual(1, thresholds.Evaluate(30));
            Assert.AreEqual(2, thresholds.Evaluate(90));
            Assert.AreEqual(3, thresholds.Evaluate(150));
        }

        [Test]
        public void SaveData_TracksTwelveLevelProgressAndPreservesLegacyRows()
        {
            var save = new Asadito.Runtime.MvpSaveData();
            save.RecordLevelResult(1, 15, 0);
            Assert.AreEqual(1, save.MaxUnlockedLevel, "A level without a star must not advance progression.");
            save.RecordLevelResult(1, 130, 2);
            Assert.AreEqual(Asadito.Runtime.MvpSaveData.CurrentVersion, save.Version);
            Assert.AreEqual(2, save.StarsByLevel[0]);
            Assert.AreEqual(130, save.BestScoreByLevel[0]);
            Assert.AreEqual(2, save.MaxUnlockedLevel);
            Assert.AreEqual(Asadito.Runtime.MvpLevelCatalog.Count, save.StarsByLevel.Length);
            var legacy = new Asadito.Runtime.MvpSaveData
            {
                Version = 1,
                MaxUnlockedLevel = 5,
                StarsByLevel = new[] { 1, 2, 3, 1, 2 },
                BestScoreByLevel = new[] { 70, 90, 120, 60, 99 }
            };
            Asadito.Runtime.MvpSaveData migrated = Asadito.Runtime.MvpSaveData.Migrate(legacy);
            Assert.AreEqual(Asadito.Runtime.MvpSaveData.CurrentVersion, migrated.Version);
            Assert.AreEqual(12, migrated.StarsByLevel.Length);
            Assert.AreEqual(3, migrated.StarsByLevel[2]);
            Assert.AreEqual(120, migrated.BestScoreByLevel[2]);
            Assert.AreEqual(0, migrated.StarsByLevel[11]);
            Assert.AreEqual(20f, migrated.Settings.SimulationTimeScale,
                "Legacy default 30x should migrate to the measured mobile-friendly 20x default.");
            migrated.RecordLevelResult(5, 160, 2);
            Assert.AreEqual(6, migrated.MaxUnlockedLevel);
        }

        [Test]
        public void SaveData_LegacyUnlockCannotSkipUncompletedPredecessors()
        {
            var save = Asadito.Runtime.MvpSaveData.Migrate(new Asadito.Runtime.MvpSaveData
            {
                Version = 1, MaxUnlockedLevel = 6,
                StarsByLevel = new[] { 0, 2, 1 }, BestScoreByLevel = new[] { 77, 130, 100 }
            });
            string unchanged = UnityEngine.JsonUtility.ToJson(save);
            Assert.IsTrue(save.IsLevelUnlocked(1));
            Assert.IsFalse(save.IsLevelUnlocked(2), "Level 2 requires Level 1 completion, not an old unlock ceiling.");
            Assert.IsFalse(save.IsLevelUnlocked(3), "A later star cannot skip a missing prerequisite earlier in the chain.");
            Assert.IsFalse(save.IsLevelUnlocked(4));
            Assert.AreEqual(unchanged, UnityEngine.JsonUtility.ToJson(save), "Availability checks must never erase legacy progress or money.");
            save.RecordLevelResult(1, 77, 0);
            Assert.IsFalse(save.IsLevelUnlocked(2), "An unsuccessful completed attempt does not pass the level.");
            save.RecordLevelResult(1, 130, 1);
            Assert.IsTrue(save.IsLevelUnlocked(2));
            Assert.IsTrue(save.IsLevelUnlocked(4), "Legitimate saved completions remain available after filling the prerequisite.");
            Assert.IsFalse(save.IsLevelUnlocked(5));
            Assert.AreEqual(6, save.MaxUnlockedLevel);
            Assert.AreEqual(2, save.StarsByLevel[1]);
        }

        [Test]
        public void SaveData_CompletionUnlockPersistsAndFailedRetryCannotRelock()
        {
            var save = Asadito.Runtime.MvpSaveData.Migrate(new Asadito.Runtime.MvpSaveData());
            Assert.IsTrue(save.IsLevelUnlocked(1));
            Assert.IsFalse(save.IsLevelUnlocked(2));
            save.RecordLevelResult(1, 130, 1);
            Assert.IsTrue(save.IsLevelUnlocked(2));
            save.RecordLevelResult(1, 12, 0);
            var reloaded = Asadito.Runtime.MvpSaveData.Migrate(UnityEngine.JsonUtility.FromJson<Asadito.Runtime.MvpSaveData>(
                UnityEngine.JsonUtility.ToJson(save)));
            Assert.IsTrue(reloaded.IsLevelUnlocked(2));
            Assert.IsFalse(reloaded.IsLevelUnlocked(3));
            Assert.IsFalse(reloaded.IsLevelUnlocked(0));
            Assert.IsFalse(reloaded.IsLevelUnlocked(-1));
            Assert.IsFalse(reloaded.IsLevelUnlocked(Asadito.Runtime.MvpLevelCatalog.Count + 1));
        }

        [Test]
        public void SaveMigration_UpdatesOnlyTheOldDefaultSimulationSpeed()
        {
            var defaultSave = new Asadito.Runtime.MvpSaveData
            {
                Version = 2,
                Settings = new Asadito.Runtime.MvpSettings { SimulationTimeScale = 30f }
            };
            var tunedSave = new Asadito.Runtime.MvpSaveData
            {
                Version = 2,
                Settings = new Asadito.Runtime.MvpSettings { SimulationTimeScale = 25f }
            };

            Assert.AreEqual(20f, Asadito.Runtime.MvpSaveData.Migrate(defaultSave).Settings.SimulationTimeScale);
            Assert.AreEqual(25f, Asadito.Runtime.MvpSaveData.Migrate(tunedSave).Settings.SimulationTimeScale,
                "A non-default saved tuning choice must survive migration.");
        }

        [Test]
        public void MvpLevelCatalog_DefinesProgressivePlayableContentForAllTwelveLevels()
        {
            Assert.AreEqual(12, Asadito.Runtime.MvpLevelCatalog.Count);
            int[] expectedGuests = { 2, 3, 4, 4, 4, 5, 4, 5, 5, 6, 6, 6 };
            for (int level = 1; level <= Asadito.Runtime.MvpLevelCatalog.Count; level++)
            {
                Asadito.Runtime.MvpLevelDefinition content = Asadito.Runtime.MvpLevelCatalog.Get(level);
                Assert.AreEqual(expectedGuests[level - 1], content.GuestCount);
                Assert.AreEqual(content.GuestCount, content.FoodIds.Length);
                Assert.AreEqual(content.GuestCount, content.PortionAmounts.Length);
                Assert.AreEqual(content.GuestCount, Asadito.Runtime.MvpLevelCatalog.CreateGuests(level).Length);
            }
            Assert.Contains("tira", Asadito.Runtime.MvpLevelCatalog.Get(4).FoodIds);
            Assert.Contains("chorizo", Asadito.Runtime.MvpLevelCatalog.Get(5).FoodIds);
            Assert.Contains("tira", Asadito.Runtime.MvpLevelCatalog.Get(6).FoodIds);
            Assert.Contains("morcilla_vasca", Asadito.Runtime.MvpLevelCatalog.Get(8).FoodIds);
            Assert.Contains("pollo_deshuesado", Asadito.Runtime.MvpLevelCatalog.Get(9).FoodIds);
            var introduced = new System.Collections.Generic.HashSet<string>();
            for (int level = 1; level <= Asadito.Runtime.MvpLevelCatalog.Count; level++)
                foreach (string foodId in Asadito.Runtime.MvpLevelCatalog.Get(level).FoodIds) introduced.Add(foodId);
            Assert.AreEqual(6, Asadito.Runtime.MvpLevelCatalog.MaxPlayableLevel);
        }

        [Test]
        public void MvpLevelCatalog_ReturnsIndependentDefinitionsAndSixDistinctProfiles()
        {
            Asadito.Runtime.MvpLevelDefinition first = Asadito.Runtime.MvpLevelCatalog.Get(1);
            first.FoodIds[0] = "mutated";
            Assert.AreEqual("tira", Asadito.Runtime.MvpLevelCatalog.Get(1).FoodIds[0]);
            var allGuests = Asadito.Runtime.MvpLevelCatalog.CreateGuests(12);
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (Asadito.Runtime.GuestProfile guest in allGuests) ids.Add(guest.Id);
            Assert.AreEqual(6, ids.Count);
        }

        [Test]
        public void EveryMvpLevelCanAllocateItsMenuAcrossAllGuests()
        {
            for (int levelNumber = 1; levelNumber <= Asadito.Runtime.MvpLevelCatalog.Count; levelNumber++)
            {
                Asadito.Runtime.MvpLevelDefinition level = Asadito.Runtime.MvpLevelCatalog.Get(levelNumber);
                Asadito.Runtime.GuestProfile[] guests = Asadito.Runtime.MvpLevelCatalog.CreateGuests(levelNumber);
                var portions = new Asadito.Runtime.ServingPortion[level.FoodIds.Length];
                for (int i = 0; i < portions.Length; i++)
                    portions[i] = new Asadito.Runtime.ServingPortion
                    {
                        Id = "portion-" + i,
                        FoodId = level.FoodIds[i],
                        Amount = level.PortionAmounts[i],
                        Doneness = Asadito.Runtime.Doneness.A_Punto
                    };

                System.Collections.Generic.List<Asadito.Runtime.ServingAssignment> assignments = Asadito.Runtime.ServingAllocator.Allocate(guests, portions);
                var assignedGuests = new System.Collections.Generic.HashSet<string>();
                foreach (Asadito.Runtime.ServingAssignment assignment in assignments) assignedGuests.Add(assignment.GuestId);
                Assert.AreEqual(level.GuestCount, assignedGuests.Count, "Level " + levelNumber + " should give every diner one serving.");
            }
        }
    }
}
