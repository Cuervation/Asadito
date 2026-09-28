using NUnit.Framework;
using UnityEngine;

namespace Asadito.Tests
{
    public sealed class ServingAndProgressTests
    {
        [Test]
        public void ServingAllocator_IsDeterministic_AndUsesFoodPreferenceAfterCoverage()
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
        public void SaveData_TracksFiveLevelProgressWithoutSavingExtraFields()
        {
            var save = new Asadito.Runtime.MvpSaveData();
            save.RecordLevelResult(1, 15, 0);
            Assert.AreEqual(1, save.MaxUnlockedLevel, "A level without a star must not advance progression.");
            save.RecordLevelResult(1, 130, 2);
            Assert.AreEqual(Asadito.Runtime.MvpSaveData.CurrentVersion, save.Version);
            Assert.AreEqual(2, save.StarsByLevel[0]);
            Assert.AreEqual(130, save.BestScoreByLevel[0]);
            Assert.AreEqual(2, save.MaxUnlockedLevel);
            Assert.AreEqual(5, save.StarsByLevel.Length);
        }

        [Test]
        public void MvpLevelCatalog_DefinesProgressivePlayableContentForAllFiveLevels()
        {
            Assert.AreEqual(5, Asadito.Runtime.MvpLevelCatalog.Count);
            int[] expectedGuests = { 2, 3, 4, 4, 6 };
            for (int level = 1; level <= 5; level++)
            {
                Asadito.Runtime.MvpLevelDefinition content = Asadito.Runtime.MvpLevelCatalog.Get(level);
                Assert.AreEqual(expectedGuests[level - 1], content.GuestCount);
                Assert.AreEqual(content.GuestCount, content.FoodIds.Length);
                Assert.AreEqual(content.GuestCount, content.PortionAmounts.Length);
                Assert.AreEqual(content.GuestCount, Asadito.Runtime.MvpLevelCatalog.CreateGuests(level).Length);
            }
            Assert.Contains("vacio", Asadito.Runtime.MvpLevelCatalog.Get(4).FoodIds);
            Assert.Contains("provoleta", Asadito.Runtime.MvpLevelCatalog.Get(5).FoodIds);
        }

        [Test]
        public void MvpLevelCatalog_ReturnsIndependentDefinitionsAndSixDistinctProfiles()
        {
            Asadito.Runtime.MvpLevelDefinition first = Asadito.Runtime.MvpLevelCatalog.Get(1);
            first.FoodIds[0] = "mutated";
            Assert.AreEqual("tira", Asadito.Runtime.MvpLevelCatalog.Get(1).FoodIds[0]);
            var allGuests = Asadito.Runtime.MvpLevelCatalog.CreateGuests(5);
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
