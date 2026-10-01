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
        public void FoodVisualAreasAreRelativeToUnchangedChorizoAndKeepEverySpriteAspect()
        {
            Asadito.Runtime.FoodVisualReference reference = Asadito.Runtime.FoodCatalog.GetVisualReference();
            Assert.AreEqual("chorizo", reference.FoodId);
            Asadito.Runtime.FoodDefinition chorizo = Asadito.Runtime.FoodCatalog.Get("chorizo");
            Assert.AreEqual(1f, chorizo.FootprintAreaMultiplier, .001f);

            Sprite referenceSprite = null;
            foreach (Asadito.Runtime.FoodDefinition food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + food.Id);
                Sprite[] states = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(food, atlas);
                foreach (Sprite state in states) createdSprites.Add(state);
                float aspect = states[0].rect.width / states[0].rect.height;
                for (int stage = 1; stage < states.Length; stage++)
                {
                    Assert.AreEqual(states[0].rect.width, states[stage].rect.width, .001f, food.Id + " frame width drift");
                    Assert.AreEqual(states[0].rect.height, states[stage].rect.height, .001f, food.Id + " frame height drift");
                }
                if (food.Id == reference.FoodId) referenceSprite = states[0];
                // Retain sprite dimensions until the reference aspect is known below.
                Assert.Greater(aspect, 0f);
            }

            Assert.IsNotNull(referenceSprite);
            float referenceAspect = referenceSprite.rect.width / referenceSprite.rect.height;
            Vector2 baseSize = Asadito.Runtime.FoodFootprintLayout.FitAspectToBounds(referenceAspect,
                new Vector2(reference.LegacyRectWidth, reference.LegacyRectHeight) * reference.LegacyDisplayScale);
            Vector2 chorizoSize = Asadito.Runtime.FoodFootprintLayout.CalculateVisualSize(reference, referenceAspect,
                referenceAspect, chorizo.FootprintAreaMultiplier);
            Assert.AreEqual(baseSize.x, chorizoSize.x, .001f, "Chorizo must retain the existing fitted size.");
            Assert.AreEqual(baseSize.y, chorizoSize.y, .001f, "Chorizo must retain the existing fitted size.");

            foreach (Asadito.Runtime.FoodDefinition food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + food.Id);
                Sprite[] states = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(food, atlas);
                foreach (Sprite state in states) createdSprites.Add(state);
                Vector2 size = Asadito.Runtime.FoodFootprintLayout.CalculateVisualSize(reference, referenceAspect,
                    states[0].rect.width / states[0].rect.height, food.FootprintAreaMultiplier);
                Assert.AreEqual(states[0].rect.width / states[0].rect.height, size.x / size.y, .002f,
                    food.Id + " must not stretch its source silhouette.");
                Assert.AreEqual(food.FootprintAreaMultiplier, (size.x * size.y) / (baseSize.x * baseSize.y), .002f,
                    food.Id + " physical/visual area must match its authored ratio.");
            }
        }

        [Test]
        public void GrillPackingUsesRealFoodFootprintsForAllTwelveLevels()
        {
            Asadito.Runtime.FoodVisualReference reference = Asadito.Runtime.FoodCatalog.GetVisualReference();
            Asadito.Runtime.FoodDefinition referenceFood = Asadito.Runtime.FoodCatalog.Get(reference.FoodId);
            Texture2D referenceAtlas = Resources.Load<Texture2D>("Art/Foods/States/" + reference.FoodId);
            Sprite[] referenceStates = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(referenceFood, referenceAtlas);
            foreach (Sprite sprite in referenceStates) createdSprites.Add(sprite);
            float referenceAspect = referenceStates[0].rect.width / referenceStates[0].rect.height;
            Vector2 board = new Vector2(640f, 900f);

            for (int levelNumber = 1; levelNumber <= Asadito.Runtime.MvpLevelCatalog.Count; levelNumber++)
            {
                Asadito.Runtime.MvpLevelDefinition level = Asadito.Runtime.MvpLevelCatalog.Get(levelNumber);
                var sizes = new Vector2[level.FoodIds.Length];
                for (int i = 0; i < sizes.Length; i++)
                {
                    Asadito.Runtime.FoodDefinition food = Asadito.Runtime.FoodCatalog.Get(level.FoodIds[i]);
                    Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + food.Id);
                    Sprite[] sprites = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(food, atlas);
                    foreach (Sprite sprite in sprites) createdSprites.Add(sprite);
                    sizes[i] = Asadito.Runtime.FoodFootprintLayout.CalculateVisualSize(reference, referenceAspect,
                        sprites[0].rect.width / sprites[0].rect.height, food.FootprintAreaMultiplier);
                }
                Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.TryPack(board, sizes, 16f, out Vector2[] positions),
                    "L" + levelNumber + " should fit by its real piece footprints.");
                for (int i = 0; i < sizes.Length; i++)
                {
                    Vector2 center = new Vector2((positions[i].x - .5f) * board.x, (positions[i].y - .5f) * board.y);
                    Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.FitsInside(board, sizes[i], center), "L" + levelNumber + " piece outside grill.");
                    for (int j = i + 1; j < sizes.Length; j++)
                    {
                        Vector2 other = new Vector2((positions[j].x - .5f) * board.x, (positions[j].y - .5f) * board.y);
                        Assert.IsFalse(Asadito.Runtime.FoodFootprintLayout.Overlaps(center, sizes[i], other, sizes[j], 15.9f),
                            "L" + levelNumber + " pieces overlap.");
                    }
                }
            }

            Vector2 chorizoSize = Asadito.Runtime.FoodFootprintLayout.CalculateVisualSize(reference, referenceAspect,
                referenceAspect, referenceFood.FootprintAreaMultiplier);
            Vector2 vacioSize = Asadito.Runtime.FoodFootprintLayout.CalculateVisualSize(reference, referenceAspect,
                AspectFor("vacio"), Asadito.Runtime.FoodCatalog.Get("vacio").FootprintAreaMultiplier);
            var chorizos = new Vector2[10];
            var vacios = new Vector2[10];
            for (int i = 0; i < 10; i++) { chorizos[i] = chorizoSize; vacios[i] = vacioSize; }
            Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.TryPack(board, chorizos, 8f, out _), "Ten chorizos should fit.");
            Assert.IsFalse(Asadito.Runtime.FoodFootprintLayout.TryPack(board, vacios, 8f, out _), "Ten vacíos should not fit in the same grill.");
        }

        [Test]
        public void RawTrayPacksFoodBeforeStaggeringOnlyOverflow()
        {
            Vector2 tray = new Vector2(380f, 310f);
            var fitsWithoutStacking = new[] { new Vector2(220f, 140f), new Vector2(220f, 140f) };
            Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.TryPackOrStack(tray, fitsWithoutStacking, 8f,
                out Vector2[] fitCenters, out bool[] fitStacked));
            Assert.IsFalse(fitStacked[0] || fitStacked[1], "Pieces that fit should remain separate.");
            AssertPackedWithoutOverlap(tray, fitsWithoutStacking, fitCenters, fitStacked);

            var crowded = new[]
            {
                new Vector2(180f, 140f), new Vector2(180f, 140f),
                new Vector2(180f, 140f), new Vector2(180f, 140f)
            };
            Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.TryPackOrStack(tray, crowded, 8f,
                out Vector2[] crowdedCenters, out bool[] crowdedStacked));
            int overflowCount = 0;
            for (int i = 0; i < crowdedStacked.Length; i++) if (crowdedStacked[i]) overflowCount++;
            Assert.That(overflowCount, Is.GreaterThan(0).And.LessThan(crowded.Length),
                "A full tray should pack what fits before deliberately stacking the rest.");
            AssertPackedWithoutOverlap(tray, crowded, crowdedCenters, crowdedStacked);

            for (int i = 0; i < crowded.Length; i++)
            {
                if (!crowdedStacked[i]) continue;
                Vector2 stackedCenter = ToLocal(tray, crowdedCenters[i]);
                Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.FitsInside(tray, crowded[i], stackedCenter),
                    "Even intentionally stacked food should stay within the aluminum tray.");
                Assert.IsFalse(crowdedCenters[i] == crowdedCenters[0], "Stack offsets should expose the layer beneath.");
            }
        }

        [Test]
        public void EveryLevelKeepsFoodScaleOnServingBoardAndStacksOnlyOverflow()
        {
            Asadito.Runtime.FoodVisualReference reference = Asadito.Runtime.FoodCatalog.GetVisualReference();
            Asadito.Runtime.FoodDefinition referenceFood = Asadito.Runtime.FoodCatalog.Get(reference.FoodId);
            Texture2D referenceAtlas = Resources.Load<Texture2D>("Art/Foods/States/" + reference.FoodId);
            Sprite[] referenceSprites = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(referenceFood, referenceAtlas);
            createdSprites.AddRange(referenceSprites);
            float referenceAspect = referenceSprites[0].rect.width / referenceSprites[0].rect.height;

            var foodSizes = new Dictionary<string, Vector2>(System.StringComparer.OrdinalIgnoreCase);
            var foodAspects = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);
            foreach (Asadito.Runtime.FoodDefinition food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + food.Id);
                Assert.IsNotNull(atlas, "Missing food atlas " + food.Id);
                Sprite[] sprites = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(food, atlas);
                createdSprites.AddRange(sprites);
                float aspect = sprites[0].rect.width / sprites[0].rect.height;
                foodAspects.Add(food.Id, aspect);
                foodSizes.Add(food.Id, Asadito.Runtime.FoodFootprintLayout.CalculateVisualSize(reference,
                    referenceAspect, aspect, food.FootprintAreaMultiplier));
            }

            // Matches the inset cutting surface on TablaAsador.png, not the rim or handle.
            Vector2 boardArea = new Vector2(380f * .74f, (380f / 1.5f) * .56f);
            // Matches the generated tray's flat center, excluding the rolled aluminum rim.
            Vector2 rawTrayArea = new Vector2(380f * .84f, (380f / 1.5f) * .71f);
            const float rawTrayFoodGap = 2f;
            for (int levelNumber = 1; levelNumber <= Asadito.Runtime.MvpLevelCatalog.Count; levelNumber++)
            {
                string[] foodIds = Asadito.Runtime.MvpLevelCatalog.Get(levelNumber).FoodIds;
                var sizes = new Vector2[foodIds.Length];
                for (int i = 0; i < sizes.Length; i++) sizes[i] = foodSizes[foodIds[i]];

                float sharedScale = Mathf.Min(
                    Asadito.Runtime.FoodFootprintLayout.GetMaxUniformFitScale(rawTrayArea, sizes, rawTrayFoodGap),
                    Asadito.Runtime.FoodFootprintLayout.GetMaxUniformFitScale(boardArea, sizes, 8f));
                Assert.That(sharedScale, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f),
                    "L" + levelNumber + " needs one valid shared tray/grill/board scale.");
                for (int i = 0; i < sizes.Length; i++) sizes[i] *= sharedScale;

                Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.TryPackOrStack(boardArea, sizes, 8f,
                    out Vector2[] centers, out bool[] stacked), "L" + levelNumber + " portions should be positioned on the board.");
                Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.TryPackOrStack(rawTrayArea, sizes, rawTrayFoodGap,
                    out Vector2[] rawCenters, out bool[] rawStacked), "L" + levelNumber + " portions should be positioned on the raw tray.");
                Assert.That(centers.Length, Is.EqualTo(sizes.Length));
                Assert.That(stacked.Length, Is.EqualTo(sizes.Length));
                for (int i = 0; i < sizes.Length; i++)
                {
                    Vector2 displayedSize = sizes[i];
                    Assert.That(displayedSize.x / displayedSize.y, Is.EqualTo(foodAspects[foodIds[i]]).Within(.001f),
                        foodIds[i] + " must keep its original food-sprite aspect on the board.");
                    Vector2 center = ToLocal(boardArea, centers[i]);
                    Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.FitsInside(boardArea, displayedSize, center),
                        "L" + levelNumber + " " + foodIds[i] + " must stay fully on the board's cutting surface, including stacked layers.");
                    Vector2 rawCenter = ToLocal(rawTrayArea, rawCenters[i]);
                    Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.FitsInside(rawTrayArea, displayedSize, rawCenter),
                        "L" + levelNumber + " " + foodIds[i] + " must stay fully inside the aluminum tray, including stacked layers.");
                    for (int j = i + 1; j < sizes.Length; j++)
                    {
                        Vector2 otherCenter = ToLocal(boardArea, centers[j]);
                        bool overlap = Asadito.Runtime.FoodFootprintLayout.Overlaps(center, displayedSize,
                            otherCenter, sizes[j], 7.9f);
                        if (overlap)
                            Assert.IsTrue(stacked[i] || stacked[j],
                                "L" + levelNumber + " portions may overlap only when the layout marks overflow as stacked.");

                        Vector2 otherRawCenter = ToLocal(rawTrayArea, rawCenters[j]);
                        bool rawOverlap = Asadito.Runtime.FoodFootprintLayout.Overlaps(rawCenter, displayedSize,
                            otherRawCenter, sizes[j], rawTrayFoodGap - .1f);
                        if (rawOverlap)
                            Assert.IsTrue(rawStacked[i] || rawStacked[j],
                                "L" + levelNumber + " raw portions may overlap only when marked as stacked overflow.");
                    }
                }
            }
        }

        private static void AssertPackedWithoutOverlap(Vector2 area, Vector2[] sizes, Vector2[] normalizedCenters,
            bool[] stacked)
        {
            for (int i = 0; i < sizes.Length; i++)
            {
                if (stacked[i]) continue;
                Vector2 center = ToLocal(area, normalizedCenters[i]);
                Assert.IsTrue(Asadito.Runtime.FoodFootprintLayout.FitsInside(area, sizes[i], center),
                    "Every unstacked piece must fit fully on the tray.");
                for (int j = i + 1; j < sizes.Length; j++)
                {
                    if (stacked[j]) continue;
                    Assert.IsFalse(Asadito.Runtime.FoodFootprintLayout.Overlaps(center, sizes[i],
                        ToLocal(area, normalizedCenters[j]), sizes[j], 7.9f),
                        "Unstacked food pieces must have a visible gap.");
                }
            }
        }

        private static Vector2 ToLocal(Vector2 area, Vector2 normalized) =>
            new Vector2((normalized.x - .5f) * area.x, (normalized.y - .5f) * area.y);

        private static float AspectFor(string foodId)
        {
            Asadito.Runtime.FoodDefinition food = Asadito.Runtime.FoodCatalog.Get(foodId);
            Texture2D atlas = Resources.Load<Texture2D>("Art/Foods/States/" + foodId);
            Sprite[] states = Asadito.Runtime.FoodSpriteLibrary.CreateStateSprites(food, atlas);
            float aspect = states[0].rect.width / states[0].rect.height;
            foreach (Sprite sprite in states) Object.DestroyImmediate(sprite);
            return aspect;
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
        public void DefaultTwentyXSimulationLeavesAtLeastOneAndQuarterSecondsInApointForEveryFood()
        {
            const int maxFrames = 60 * 300;
            const float frameMinutesAtSixtyFps = 20f / 60f / 60f;
            Assert.AreEqual(20f, new Asadito.Runtime.MvpSettings().SimulationTimeScale);

            foreach (Asadito.Runtime.FoodDefinition food in Asadito.Runtime.FoodCatalog.GetAll())
            {
                var state = NewWarmState();
                int frames = 0;
                bool flipped = false;
                while (Asadito.Runtime.FoodCookingModel.GetDoneness(state, food.Profile) != Asadito.Runtime.Doneness.A_Punto && frames < maxFrames)
                {
                    Asadito.Runtime.FoodCookingModel.Step(state, food.Profile, 210f, frameMinutesAtSixtyFps, true);
                    frames++;
                    if (!flipped && state.CoreTemperatureC >= 38f)
                    {
                        state.Flip();
                        flipped = true;
                    }
                }
                Assert.Less(frames, maxFrames, food.Id + " should reach a preferred doneness when started and flipped normally.");

                int aPointFrames = 0;
                while (Asadito.Runtime.FoodCookingModel.GetDoneness(state, food.Profile) == Asadito.Runtime.Doneness.A_Punto && aPointFrames < maxFrames)
                {
                    Asadito.Runtime.FoodCookingModel.Step(state, food.Profile, 210f, frameMinutesAtSixtyFps, true);
                    aPointFrames++;
                }
                Assert.GreaterOrEqual(aPointFrames / 60f, 1.25f,
                    food.Id + " A_Punto window should allow a deliberate mobile touch at the 20x default.");
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
