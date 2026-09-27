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
    }
}
