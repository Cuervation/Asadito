using NUnit.Framework;
using UnityEngine;

namespace Asadito.Tests
{
    public sealed class HeatAndCookingTests
    {
        [Test]
        public void CharcoalGrill_MoveEmbersConservesEnergy_AndFuelDecaysOverTime()
        {
            var grill = new Asadito.Runtime.CharcoalGrillModel();
            grill.Reset();
            grill.Ignite();
            float totalBefore = SumEmbers(grill.Grid);
            float sourceBefore = grill.Grid.GetCell(0, 2).EmberEnergy;
            float targetBefore = grill.Grid.GetCell(1, 2).EmberEnergy;

            Assert.IsTrue(grill.MoveEmbers(0, 2, 1, 2));
            Assert.AreEqual(totalBefore, SumEmbers(grill.Grid), .001f);
            Assert.Less(grill.Grid.GetCell(0, 2).EmberEnergy, sourceBefore);
            Assert.Greater(grill.Grid.GetCell(1, 2).EmberEnergy, targetBefore);

            float fuelBefore = grill.FuelEnergy;
            grill.Step(10f);
            Assert.Less(grill.FuelEnergy, fuelBefore);
        }

        [Test]
        public void HeatGrid_DefaultsToEightBySix_AndSamplesOnlyOccupiedRegion()
        {
            var grid = new Asadito.Runtime.HeatGrid();
            Assert.AreEqual(8, grid.Width);
            Assert.AreEqual(6, grid.Height);
            grid.SetCell(0, 0, new Asadito.Runtime.HeatCell(100f, 40f));
            grid.SetCell(1, 0, new Asadito.Runtime.HeatCell(200f, 80f));

            Vector2 sample = grid.SampleRegion(new Rect(0f, 0f, .25f, 1f / 6f));
            Assert.AreEqual(150f, sample.x, .01f);
            Assert.AreEqual(60f, sample.y, .01f);
        }

        [Test]
        public void FoodCooking_RespondsToLocalHeat_AndTracksBurnOnExposedSide()
        {
            var state = new Asadito.Runtime.FoodState { CoreTemperatureC = 20f, SurfaceTemperatureC = 20f };
            var profile = new Asadito.Runtime.FoodCookProfile
            {
                SurfaceTransferRate = 1f,
                CoreTransferRate = 1f,
                MaillardRate = .1f,
                CharRate = .2f,
                MaillardStartsAtC = 100f,
                CharStartsAtC = 150f
            };

            Asadito.Runtime.FoodCookingModel.Step(state, profile, 240f, 5f, true);
            Asadito.Runtime.FoodFaceState firstSide = state.CurrentFace;
            Assert.Greater(state.CoreTemperatureC, 20f);
            Assert.Greater(firstSide.Maillard, 0f);
            Assert.Greater(firstSide.Char, 0f);
            Assert.AreEqual(0f, state.Faces[1].Char);

            state.Flip();
            Asadito.Runtime.FoodCookingModel.Step(state, profile, 240f, 1f, true);
            Assert.Greater(state.CurrentFace.Char, 0f);
            Assert.Greater(state.Faces[0].Char, 0f);
        }

        [Test]
        public void DonenessMatch_UsesConfiguredThermalBand()
        {
            var state = new Asadito.Runtime.FoodState { CoreTemperatureC = 57f };
            var profile = new Asadito.Runtime.FoodCookProfile();
            Assert.AreEqual(100f, Asadito.Runtime.FoodCookingModel.EvaluateDonenessMatch(state, profile, Asadito.Runtime.Doneness.A_Punto));
            Assert.Less(Asadito.Runtime.FoodCookingModel.EvaluateDonenessMatch(state, profile, Asadito.Runtime.Doneness.Jugoso), 100f);
        }

        [Test]
        public void FoodProfiles_UseDistinctDonenessBandsForChorizoAndTira()
        {
            var chorizo = Asadito.Runtime.FoodCookingModel.CreateProfile("chorizo");
            var tira = Asadito.Runtime.FoodCookingModel.CreateProfile("tira");

            Assert.AreEqual("chorizo", chorizo.FoodId);
            Assert.AreEqual("tira", tira.FoodId);
            Assert.AreNotEqual(tira.DonenessBands[0].MinimumCoreC, chorizo.DonenessBands[0].MinimumCoreC);
            Assert.AreNotEqual(tira.CoreTransferRate, chorizo.CoreTransferRate);
            Assert.AreEqual(.007f, chorizo.CoreTransferRate, .0001f);
            Assert.AreEqual(71f, chorizo.DonenessBands[1].MinimumCoreC);
            Assert.AreEqual(73f, chorizo.DonenessBands[1].MaximumCoreC);
        }

        private static float SumEmbers(Asadito.Runtime.HeatGrid grid)
        {
            float total = 0f;
            for (int y = 0; y < grid.Height; y++)
            for (int x = 0; x < grid.Width; x++)
                total += grid.GetCell(x, y).EmberEnergy;
            return total;
        }
    }
}
