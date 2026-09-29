using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asadito.Tests
{
    public sealed class FoodCatalogTests
    {
        private readonly List<Sprite> createdSprites = new List<Sprite>();

        [TearDown]
        public void TearDown()
        {
            foreach (Sprite sprite in createdSprites)
                if (sprite != null) Object.DestroyImmediate(sprite);
            createdSprites.Clear();
        }

        [Test]
        public void Catalog_HasEighteenUniqueDefinitionsAndValidDistinctThermalProfiles()
        {
            string[] errors;
            Asadito.Runtime.FoodCatalog.Validate(out errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
            Asadito.Runtime.FoodDefinition[] foods = Asadito.Runtime.FoodCatalog.GetAll();
            Assert.AreEqual(18, foods.Length);

            var ids = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var thermalSignatures = new HashSet<string>();
            foreach (Asadito.Runtime.FoodDefinition food in foods)
            {
                Assert.IsTrue(ids.Add(food.Id), "Duplicate FoodId " + food.Id);
                Assert.AreEqual(food.Id, food.Profile.FoodId);
                Assert.IsTrue(Asadito.Runtime.FoodCookingModel.IsProfileValid(food.Profile, out string reason), food.Id + ": " + reason);
                Assert.IsTrue(thermalSignatures.Add(string.Join("|", food.Profile.CoreTransferRate, food.Profile.SurfaceTransferRate,
                    food.Profile.MoistureLossRate, food.Profile.MaillardRate, food.Profile.CharRate, food.Profile.FatRenderRate,
                    food.Profile.ThermalMass, food.Profile.ThicknessCm, food.Profile.SplitRiskRate)), "Profile cloned for " + food.Id);
                Asadito.Runtime.FoodCookProfile copy = Asadito.Runtime.FoodCookingModel.CreateProfile(food.Id);
                copy.DonenessBands[0].MinimumCoreC = -10f;
                Assert.Greater(Asadito.Runtime.FoodCatalog.Get(food.Id).Profile.DonenessBands[0].MinimumCoreC, 0f,
                    "Callers must not be able to mutate shared catalog data.");
            }
            Assert.IsFalse(Asadito.Runtime.FoodCatalog.TryGet("not-a-cut", out _));
            Assert.Throws<System.ArgumentException>(() => Asadito.Runtime.FoodCookingModel.CreateProfile("not-a-cut"));
        }

        [Test]
        public void EveryFoodLoadsSixRequiredSpritesFromItsOwnAtlas()
        {
            foreach (Asadito.Runtime.FoodDefinition food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + food.Id);
                Assert.IsNotNull(atlas, "Missing atlas for " + food.Id);
                Assert.GreaterOrEqual(atlas.height, Asadito.Runtime.FoodSpriteLibrary.StateCount,
                    food.Id + " atlas needs at least six vertical state rows.");
                Sprite[] sprites = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(food, atlas);
                Assert.AreEqual(Asadito.Runtime.FoodSpriteLibrary.StateCount, sprites.Length, food.Id);
                for (int stage = 0; stage < sprites.Length; stage++)
                {
                    Assert.IsNotNull(sprites[stage], food.Id + " missing " + Asadito.Runtime.FoodSpriteLibrary.StateNames[stage]);
                    Assert.Greater(sprites[stage].rect.width, 0f);
                    Assert.Greater(sprites[stage].rect.height, 0f);
                    Assert.AreEqual(food.Id + "_" + Asadito.Runtime.FoodSpriteLibrary.StateNames[stage], sprites[stage].name);
                    createdSprites.Add(sprites[stage]);
                }
            }
        }

        [Test]
        public void EveryFoodReachesIdealOvercookedAndBurntInControlledSimulation()
        {
            foreach (Asadito.Runtime.FoodDefinition food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                Asadito.Runtime.FoodCookProfile profile = food.Profile;
                var state = NewWarmState();
                bool warming = false, browning = false, ideal = false, overcooked = false;
                for (int step = 0; step < 900; step++)
                {
                    Asadito.Runtime.FoodCookingModel.Step(state, profile, profile.PreferredHeatC, .5f, true);
                    Asadito.Runtime.FoodCookVisualStage stage = Asadito.Runtime.FoodCookingModel.GetVisualStage(state, profile);
                    warming |= stage >= Asadito.Runtime.FoodCookVisualStage.Warming;
                    browning |= stage >= Asadito.Runtime.FoodCookVisualStage.Browning;
                    ideal |= stage == Asadito.Runtime.FoodCookVisualStage.Ideal;
                    overcooked |= stage == Asadito.Runtime.FoodCookVisualStage.Overcooked;
                }
                Assert.IsTrue(warming && browning && ideal && overcooked, food.Id + " did not traverse warming/browning/ideal/overcooked.");

                state = NewWarmState();
                for (int step = 0; step < 700; step++)
                    Asadito.Runtime.FoodCookingModel.Step(state, profile, 300f, .5f, true);
                Assert.AreEqual(Asadito.Runtime.FoodCookVisualStage.Burnt,
                    Asadito.Runtime.FoodCookingModel.GetVisualStage(state, profile), food.Id + " should be burnable.");
            }
        }

        [Test]
        public void MorcillaHasRealCasingRiskAndFavoritesUseFullPreferenceScale()
        {
            Asadito.Runtime.FoodCookProfile morcilla = Asadito.Runtime.FoodCookingModel.CreateProfile("morcilla");
            var fragile = NewWarmState();
            Asadito.Runtime.FoodCookingModel.Step(fragile, morcilla, 280f, 10f, true);
            Assert.Greater(fragile.SplitRisk, 0f);
            Assert.AreEqual(0f, Asadito.Runtime.FoodCookingModel.CreateProfile("tira").SplitRiskRate);

            var guest = new Asadito.Runtime.GuestProfile { Id = "taste-test" };
            guest.DislikedFoods.Add("vacio");
            guest.LikedFoods.Add("tira");
            guest.FavoriteFoods.Add("chorizo");
            Assert.AreEqual(0f, Asadito.Runtime.ServingAllocator.GetFoodPreferenceScore(guest, "vacio"));
            Assert.AreEqual(40f, Asadito.Runtime.ServingAllocator.GetFoodPreferenceScore(guest, "provoleta"));
            Assert.AreEqual(70f, Asadito.Runtime.ServingAllocator.GetFoodPreferenceScore(guest, "tira"));
            Assert.AreEqual(100f, Asadito.Runtime.ServingAllocator.GetFoodPreferenceScore(guest, "chorizo"));
        }

        [Test]
        public void SixDinerRosterVariesAppetiteBodyAgeAndIncludesLikesAndDislikes()
        {
            Asadito.Runtime.GuestProfile[] guests = Asadito.Runtime.MvpLevelCatalog.CreateGuests(5);
            var appetites = new HashSet<float>();
            var weights = new HashSet<float>();
            var ages = new HashSet<int>();
            bool hasDislikes = false;
            foreach (Asadito.Runtime.GuestProfile guest in guests)
            {
                appetites.Add(guest.Appetite);
                weights.Add(guest.Weight);
                ages.Add(guest.Age);
                hasDislikes |= guest.DislikedFoods.Count > 0;
            }
            Assert.Greater(appetites.Count, 4);
            Assert.Greater(weights.Count, 4);
            Assert.Greater(ages.Count, 4);
            Assert.IsTrue(hasDislikes);
        }

        private static Asadito.Runtime.FoodState NewWarmState()
        {
            var state = new Asadito.Runtime.FoodState();
            state.Reset();
            state.CoreTemperatureC = 20f;
            state.SetCurrentFace(new Asadito.Runtime.FoodFaceState { SurfaceTemperatureC = 20f });
            state.Flip();
            state.SetCurrentFace(new Asadito.Runtime.FoodFaceState { SurfaceTemperatureC = 20f });
            state.Flip();
            return state;
        }
    }
}
